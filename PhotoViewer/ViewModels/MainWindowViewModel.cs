using Kchary.PhotoViewer.Model;
using Kchary.PhotoViewer.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kchary.PhotoViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region ViewModels

        public ExplorerViewModel ExplorerViewModel { get; set; }
        public ExifInfoViewModel ExifInfoViewModel { get; set; }

        #endregion ViewModels

        #region UI binding parameters

        private string selectFolderPath;

        /// <summary>
        /// older path being displayed
        /// </summary>
        public string SelectFolderPath
        {
            get { return selectFolderPath; }
            set { SetProperty(ref selectFolderPath, value); }
        }

        /// <summary>
        /// Media list displayed in ListBox
        /// </summary>
        public ObservableCollection<MediaInfo> MediaInfoList { get; } = new ObservableCollection<MediaInfo>();

        private MediaInfo selectedMedia;

        /// <summary>
        /// Media selected in ListBox
        /// </summary>
        public MediaInfo SelectedMedia
        {
            get { return selectedMedia; }
            set { SetProperty(ref selectedMedia, value); }
        }

        private BitmapSource pictureImageSource;

        /// <summary>
        /// Image of enlarged media
        /// </summary>
        public BitmapSource PictureImageSource
        {
            get { return pictureImageSource; }
            set { SetProperty(ref pictureImageSource, value); }
        }

        /// <summary>
        /// Menu item list displayed by ContextMenu
        /// </summary>
        public ObservableCollection<ContextMenuInfo> ContextMenuCollection { get; } = new ObservableCollection<ContextMenuInfo>();

        private bool isShowContextMenu;

        public bool IsShowContextMenu
        {
            get { return isShowContextMenu; }
            set { SetProperty(ref isShowContextMenu, value); }
        }

        private bool isEnableImageEditButton;

        public bool IsEnableImageEditButton
        {
            get { return isEnableImageEditButton; }
            set { SetProperty(ref isEnableImageEditButton, value); }
        }

        #endregion UI binding parameters

        #region Command

        public ICommand BluetoothButtonCommand { get; private set; }
        public ICommand OpenFolderButtonCommand { get; private set; }
        public ICommand ReloadButtonCommand { get; private set; }
        public ICommand SettingButtonCommand { get; private set; }
        public ICommand ImageEditButtonCommand { get; private set; }

        #endregion Command

        // Media information read thread
        private BackgroundWorker loadContentsBackgroundWorker;

        // Reload flag of media list
        private bool isReloadContents;

        public MainWindowViewModel()
        {
            MediaInfoList.Clear();
            ContextMenuCollection.Clear();
            PictureImageSource = null;
            IsShowContextMenu = false;
            IsEnableImageEditButton = false;

            // Set command.
            BluetoothButtonCommand = new DelegateCommand(BluetoothButtonClicked);
            OpenFolderButtonCommand = new DelegateCommand(OpenFolderButtonClicked);
            ReloadButtonCommand = new DelegateCommand(ReloadButtonClicked);
            SettingButtonCommand = new DelegateCommand(SettingButtonClicked);
            ImageEditButtonCommand = new DelegateCommand(ImageEditButtonClicked);

            // Read config file.
            LoadConfigFile();

            // Set view model of explorer view.
            ExplorerViewModel = new ExplorerViewModel();
            ExplorerViewModel.ChangeSelectItemEvent += ExplorerViewModel_ChangeSelectItemEvent;
            UpdateExplorerTree();

            // Set view model of exif info view.
            ExifInfoViewModel = new ExifInfoViewModel();
        }

        /// <summary>
        /// Load the initial display folder and setting file.
        /// </summary>
        public void InitViewFolder()
        {
            // Read setting information.
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var linkageAppList = appConfigManager.ConfigData.LinkageAppList;
            if (linkageAppList != null && linkageAppList.Any())
            {
                foreach (var linkageApp in linkageAppList)
                {
                    if (!File.Exists(linkageApp.AppPath))
                    {
                        appConfigManager.ConfigData.LinkageAppList.Remove(linkageApp);
                        continue;
                    }

                    // Load app icon.
                    Icon appIcon = Icon.ExtractAssociatedIcon(linkageApp.AppPath);
                    var iconBitmapSource = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    // Set context menu.
                    var contextMenu = new ContextMenuInfo { DisplayName = linkageApp.AppName, ContextIcon = iconBitmapSource };
                    ContextMenuCollection.Add(contextMenu);
                    IsShowContextMenu = true;
                }
            }

            // Load image folder.
            string picturePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);
            if (!string.IsNullOrEmpty(appConfigManager.ConfigData.PreviousFolderPath))
            {
                picturePath = appConfigManager.ConfigData.PreviousFolderPath;
            }
            ChangeContents(picturePath);
        }

        /// <summary>
        /// Event when context menu is clicked.
        /// </summary>
        /// <param name="appName">App name</param>
        public void ExecuteContextMenu(string appName)
        {
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var linkageAppList = appConfigManager.ConfigData.LinkageAppList;
            if (!linkageAppList.Any(x => x.AppName == appName))
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string appPath = linkageAppList.Find(x => x.AppName == appName).AppPath;
                Process.Start(appPath, SelectedMedia.FilePath);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Load selected image and convert for display.
        /// </summary>
        /// <param name="mediaInfo">Selected media info</param>
        public bool LoadMedia(MediaInfo mediaInfo)
        {
            if (!File.Exists(mediaInfo.FilePath))
            {
                const string FileNotExistErrorMessage = "ファイルが存在しません。";
                const string FileNotExstErrorTitle = "ファイルアクセスエラー";
                App.ShowErrorMessageBox(FileNotExistErrorMessage, FileNotExstErrorTitle);
            }

            PictureImageSource = null;
            IsEnableImageEditButton = false;

            return mediaInfo.ContentMediaType switch
            {
                MediaInfo.MediaType.PICTURE => LoadPictureImage(mediaInfo),
                _ => false,
            };
        }

        /// <summary>
        /// Stop running threads and tasks.
        /// </summary>
        /// <returns>Run thread: False, Not run thread: True</returns>
        public bool StopThreadAndTask()
        {
            bool CanClose = true;

            // Cancel notification if content loading thread is running
            if (loadContentsBackgroundWorker != null && loadContentsBackgroundWorker.IsBusy)
            {
                loadContentsBackgroundWorker.CancelAsync();
                CanClose = false;
            }

            return CanClose;
        }

        /// <summary>
        /// Read configuration file.
        /// </summary>
        private void LoadConfigFile()
        {
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.Import();
        }

        private void BluetoothButtonClicked()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Process.Start("fsquirt.exe", "-send");
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Bluetooth送信に対応していません。", "Bluetooth送信エラー");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Open selected folder in Explorer.
        /// </summary>
        private void OpenFolderButtonClicked()
        {
            if (string.IsNullOrEmpty(SelectFolderPath))
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string selectPath;
                if ((File.GetAttributes(SelectFolderPath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    selectPath = SelectFolderPath;
                }
                else
                {
                    selectPath = Path.GetDirectoryName(SelectFolderPath);
                }

                const string Explorer = "EXPLORER.EXE";
                Process.Start(Explorer, selectPath);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Update ListBox display.
        /// </summary>
        private void ReloadButtonClicked()
        {
            // Clear picture image view source.
            if (PictureImageSource != null)
            {
                PictureImageSource = null;
            }

            // Update image edit button status.
            IsEnableImageEditButton = false;

            // If the file path is displayed, change it to the directory path and read it.
            if ((File.GetAttributes(SelectFolderPath) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                SelectFolderPath = Path.GetDirectoryName(SelectFolderPath);
            }
            UpdateContents();
        }

        /// <summary>
        /// Open the setting screen.
        /// </summary>
        private void SettingButtonClicked()
        {
            var vm = new SettingViewModel();
            vm.ReloadContextMenuEvent += ReloadContextMenu;

            var settingDialog = new SettingView
            {
                DataContext = vm,
                Owner = App.Current.MainWindow
            };
            settingDialog.ShowDialog();
        }

        /// <summary>
        /// Open image editing tool.
        /// </summary>
        private void ImageEditButtonClicked()
        {
            if (SelectedMedia == null)
            {
                return;
            }

            var vm = new ImageEditToolViewModel();
            vm.SetEditFileData(SelectedMedia.FilePath);

            var imageEditToolDialog = new ImageEditToolView
            {
                DataContext = vm,
                Owner = App.Current.MainWindow
            };
            imageEditToolDialog.ShowDialog();
        }

        /// <summary>
        /// Reread context menu
        /// </summary>
        /// <param name="sender">SettingViewModel</param>
        /// <param name="e">Argument</param>
        private void ReloadContextMenu(object sender, EventArgs e)
        {
            // Reset context menu.
            ContextMenuCollection.Clear();
            IsShowContextMenu = false;

            // Reload the information related to the linked application from the setting information.
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var linkageAppList = appConfigManager.ConfigData.LinkageAppList;
            if (linkageAppList != null && linkageAppList.Any())
            {
                foreach (var linkageApp in linkageAppList)
                {
                    // Load app icon.
                    Icon appIcon = Icon.ExtractAssociatedIcon(linkageApp.AppPath);
                    var iconBitmapSource = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    // Set context menu.
                    var contextMenu = new ContextMenuInfo { DisplayName = linkageApp.AppName, ContextIcon = iconBitmapSource };
                    ContextMenuCollection.Add(contextMenu);
                    IsShowContextMenu = true;
                }
            }
        }

        /// <summary>
        /// Event when folder selection is changed in Explorer View.
        /// </summary>
        /// <param name="sender">ExplorerViewModel</param>
        /// <param name="e">Argument</param>
        private void ExplorerViewModel_ChangeSelectItemEvent(object sender, EventArgs e)
        {
            SelectedMedia = null;
            PictureImageSource = null;
            IsEnableImageEditButton = false;

            var selectedExplorerItem = ExplorerViewModel.SelectedItem;
            ChangeContents(selectedExplorerItem.ExplorerItemPath);
        }

        /// <summary>
        /// Update explorer tree view.
        /// </summary>
        private void UpdateExplorerTree()
        {
            ExplorerViewModel.CreateDriveTreeItem();

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            string previousFolderPath = appConfigManager.ConfigData.PreviousFolderPath;
            if (!string.IsNullOrEmpty(previousFolderPath))
            {
                ExplorerViewModel.ExpandPreviousPath(appConfigManager.ConfigData.PreviousFolderPath);
            }
        }

        /// <summary>
        /// Change the folder displayed in the media list.
        /// </summary>
        /// <param name="folderPath">Folder path to display media list</param>
        private void ChangeContents(string folderPath)
        {
            if (!Directory.Exists(folderPath) || SelectFolderPath == folderPath)
            {
                return;
            }

            // Update folder path to update list.
            SelectFolderPath = folderPath;
            UpdateContents();

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.ConfigData.PreviousFolderPath = SelectFolderPath;
        }

        /// <summary>
        /// Refresh display of content list.
        /// </summary>
        private void UpdateContents()
        {
            if (loadContentsBackgroundWorker != null && loadContentsBackgroundWorker.IsBusy)
            {
                loadContentsBackgroundWorker.CancelAsync();
                isReloadContents = true;
                return;
            }

            LoadContentsList();
        }

        /// <summary>
        /// Load content list.
        /// </summary>
        private void LoadContentsList()
        {
            // Clear display list before loading.
            MediaInfoList.Clear();

            var backgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += LoadContentsWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += LoadContentsWorker_RunWorkerCompleted;

            loadContentsBackgroundWorker = backgroundWorker;
            loadContentsBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Process that operates in another thread.
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">Argument</param>
        private void LoadContentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                LoadContentsWorker(sender, e);
            }
            catch (Exception ex)
            {
                App.LogException(ex);

                const string MediaReadErrorMessage = "メディアの読み込みに失敗しました。";
                const string MedaiReadErrorTitle = "読み込みエラー";
                App.ShowErrorMessageBox(MediaReadErrorMessage, MedaiReadErrorTitle);
            }
        }

        /// <summary>
        /// Event when processing is completed in the content loading thread.
        /// </summary>
        /// <param name="sender">LoadContentsWorker</param>
        /// <param name="e">Argument</param>
        private void LoadContentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StopContentsWorker();

                if (isReloadContents)
                {
                    // Reload after loading the asynchronously loaded content list.
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        UpdateContents();
                        isReloadContents = false;
                    }), DispatcherPriority.Normal);
                }
            }
            else
            {
                StopContentsWorker();
                if (SelectedMedia == null && MediaInfoList.Any())
                {
                    SelectedMedia = MediaInfoList[0];
                }
            }
        }

        /// <summary>
        /// Actual processing of reading the content list
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">Argument</param>
        private void LoadContentsWorker(object sender, DoWorkEventArgs e)
        {
            var filePaths = new LinkedList<string>();
            int tick = Environment.TickCount;

            // Get all supported files in selected folder.
            foreach (string supportExtension in MediaChecker.GetSupportExtentions())
            {
                var worker = sender as BackgroundWorker;
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var supportFiles = Directory.GetFiles(SelectFolderPath, "*" + supportExtension);
                foreach (var supportFile in supportFiles)
                {
                    filePaths.AddLast(supportFile);
                }
            }

            // Sort the order by name.
            var sortFilePaths = filePaths.OrderBy(Path.GetFileName);

            var readyFiles = new Queue<MediaInfo>();
            foreach (var filePath in sortFilePaths)
            {
                var worker = sender as BackgroundWorker;
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var mediaInfo = new MediaInfo
                {
                    FilePath = filePath
                };
                mediaInfo.FileName = Path.GetFileName(mediaInfo.FilePath);

                try
                {
                    mediaInfo.CreateThumbnailImage();
                }
                catch (Exception ex)
                {
                    // If thumbnail images cannot be created, log output and skip reading.
                    App.LogException(ex);
                    continue;
                }

                int count = 0;
                readyFiles.Enqueue(mediaInfo);
                count++;

                int duration = Environment.TickCount - tick;
                if ((count <= 100 && duration > 500) || duration > 1000)
                {
                    var readyList = readyFiles.ToArray();
                    readyFiles.Clear();
                    App.Current.Dispatcher.BeginInvoke((Action)(() => { MediaInfoList.AddRange(readyList); }));
                }
            }

            if (readyFiles.Any())
            {
                App.Current.Dispatcher.Invoke((Action)(() => { foreach (var readyFile in readyFiles) MediaInfoList.Add(readyFile); }));
            }

            // Collection of unnecessary memory.
            App.RunGC();
        }

        /// <summary>
        /// Stop content loading thread.
        /// </summary>
        private void StopContentsWorker()
        {
            if (loadContentsBackgroundWorker != null)
            {
                loadContentsBackgroundWorker.Dispose();
            }
        }

        /// <summary>
        /// Load the image to be enlarged.
        /// </summary>
        /// <param name="mediaInfo">Selected media information</param>
        /// <returns>Successful reading: True、Failure: False</returns>
        private bool LoadPictureImage(MediaInfo mediaInfo)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Create display image.
                PictureImageSource = ImageControl.CreatePictureViewImage(mediaInfo.FilePath);

                // Memory release of Writable Bitmap.
                App.RunGC();

                // Set exif information.
                ExifInfoViewModel.SetExif(mediaInfo.FilePath);

                // Update image edit button status.
                if (!MediaChecker.CheckNikonRawFileExtension(Path.GetExtension(mediaInfo.FilePath).ToLower()))
                {
                    IsEnableImageEditButton = true;
                }
                else
                {
                    IsEnableImageEditButton = false;
                }

                // Update select path.
                SelectFolderPath = mediaInfo.FilePath;

                return true;
            }
            catch (Exception ex)
            {
                App.LogException(ex);

                const string FileAccessErrorMessage = "ファイルアクセスでエラーが発生しました。";
                const string FileAccessErrorTitle = "ファイルアクセスエラー";
                App.ShowErrorMessageBox(FileAccessErrorMessage, FileAccessErrorTitle);

                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}