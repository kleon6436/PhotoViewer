using Kchary.PhotoViewer.Model;
using Kchary.PhotoViewer.Views;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class MainWindowViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable disposables = new();

        #region ViewModels

        /// <summary>
        /// エクスプローラーツリーのViewModel
        /// </summary>
        public ExplorerViewModel ExplorerViewModel { get; }

        /// <summary>
        /// Exif情報のViewModel
        /// </summary>
        public ExifInfoViewModel ExifInfoViewModel { get; }

        #endregion ViewModels

        #region UI binding parameters

        /// <summary>
        /// メディアリスト(画像一覧で表示される)
        /// </summary>
        public ObservableCollection<MediaInfo> MediaInfoList { get; } = new();

        /// <summary>
        /// コンテキストメニューに表示するリスト
        /// </summary>
        public ObservableCollection<ContextMenuInfo> ContextMenuCollection { get; } = new();

        /// <summary>
        /// 選択されたフォルダパス(絶対パス)
        /// </summary>
        public ReactivePropertySlim<string> SelectFolderPath { get; } = new();

        /// <summary>
        /// 選択した画像ファイルデータ
        /// </summary>
        public ReactivePropertySlim<MediaInfo> SelectedMedia { get; } = new();

        /// <summary>
        /// 表示する画像データ
        /// </summary>
        public ReactivePropertySlim<BitmapSource> PictureImageSource { get; } = new();

        /// <summary>
        /// コンテキストメニューの表示非表示フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsShowContextMenu { get; } = new();

        /// <summary>
        /// 編集ボタンの有効無効フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsEnableImageEditButton { get; } = new();

        #endregion UI binding parameters

        #region Command

        /// <summary>
        /// Bluetoothボタンのコマンド
        /// </summary>
        public ReactiveCommand BluetoothButtonCommand { get; }
        
        /// <summary>
        /// エクスプローラー表示ボタンのコマンド
        /// </summary>
        public ReactiveCommand OpenFolderButtonCommand { get; }

        /// <summary>
        /// 再読み込みボタンのコマンド
        /// </summary>
        public ReactiveCommand ReloadButtonCommand { get; }

        /// <summary>
        /// 設定ボタンのコマンド
        /// </summary>
        public ReactiveCommand SettingButtonCommand { get; }
        
        /// <summary>
        /// 編集ボタンのコマンド
        /// </summary>
        public ReactiveCommand ImageEditButtonCommand { get; }

        #endregion Command

        /// <summary>
        /// バックグラウンドでコンテンツをロードするためのワーカー
        /// </summary>
        private BackgroundWorker loadContentsBackgroundWorker;

        /// <summary>
        /// コンテンツをリロードするためのフラグ
        /// </summary>
        private bool isReloadContents;

        /// <summary>
        /// 既定のピクチャフォルダパス
        /// </summary>
        /// <remarks>
        /// パブリックユーザーのピクチャフォルダを既定とする
        /// </remarks>
        private static readonly string DefaultPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // コマンドの設定
            BluetoothButtonCommand  = new ReactiveCommand().WithSubscribe(BluetoothButtonClicked).AddTo(disposables);
            OpenFolderButtonCommand = new ReactiveCommand().WithSubscribe(OpenFolderButtonClicked).AddTo(disposables);
            ReloadButtonCommand     = new ReactiveCommand().WithSubscribe(ReloadButtonClicked).AddTo(disposables);
            SettingButtonCommand    = new ReactiveCommand().WithSubscribe(SettingButtonClicked).AddTo(disposables);
            ImageEditButtonCommand  = new ReactiveCommand().WithSubscribe(ImageEditButtonClicked).AddTo(disposables);

            // 設定ファイルの読み込み
            AppConfigManager.GetInstance().Import();

            // エクスプローラーツリーの設定
            ExplorerViewModel = new ExplorerViewModel();
            ExplorerViewModel.ChangeSelectItemEvent += ExplorerViewModel_ChangeSelectItemEvent;
            UpdateExplorerTree();

            // Exif情報表示の設定
            ExifInfoViewModel = new ExifInfoViewModel();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => disposables.Dispose();

        /// <summary>
        /// 表示を初期化する
        /// </summary>
        /// <remarks>
        /// 画像一覧への読み込み処理の開始などを実施する
        /// </remarks>
        public void InitViewFolder()
        {
            var linkageAppList = AppConfigManager.GetInstance().ConfigData.LinkageAppList;
            if (linkageAppList != null && linkageAppList.Any())
            {
                // リンク先がないものはすべて削除
                linkageAppList.RemoveAll(x => !File.Exists(x.AppPath));

                foreach (var linkageApp in linkageAppList)
                {
                    // アプリケーションアイコンをロード
                    var appIcon = Icon.ExtractAssociatedIcon(linkageApp.AppPath);
                    if (appIcon != null)
                    {
                        var iconBitmapSource = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        // コンテキストメニューに追加
                        var contextMenu = new ContextMenuInfo { DisplayName = linkageApp.AppName, ContextIcon = iconBitmapSource };
                        ContextMenuCollection.Add(contextMenu);
                    }

                    IsShowContextMenu.Value = true;
                }
            }

            // 画像フォルダの読み込み
            var picturePath = DefaultPicturePath;
            if (!string.IsNullOrEmpty(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath) 
                && Directory.Exists(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath))
            {
                picturePath = AppConfigManager.GetInstance().ConfigData.PreviousFolderPath;
            }
            ChangeContents(picturePath);
        }

        /// <summary>
        /// コンテキストメニューを実行したときの処理
        /// </summary>
        /// <param name="appName">Application name</param>
        public void ExecuteContextMenu(string appName)
        {
            var linkageAppList = AppConfigManager.GetInstance().ConfigData.LinkageAppList;
            if (linkageAppList.All(x => x.AppName != appName))
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var appPath = linkageAppList.Find(x => x.AppName == appName)?.AppPath;
                if (!string.IsNullOrEmpty(appPath))
                {
                    Process.Start(appPath, SelectedMedia.Value.FilePath);
                }
                else
                {
                    App.ShowErrorMessageBox("Linkage app path is not found.", "Process start error");
                }
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
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="mediaInfo">選択されたメディア情報</param>
        public async Task<bool> LoadMediaAsync(MediaInfo mediaInfo)
        {
            if (!File.Exists(mediaInfo.FilePath))
            {
                App.ShowErrorMessageBox("File not exist.", "File access error");
            }

            IsEnableImageEditButton.Value = false;

            return mediaInfo.ContentMediaType switch
            {
                MediaInfo.MediaType.Picture => await LoadPictureImageAsync(mediaInfo),
                _ => false,
            };
        }

        /// <summary>
        /// 実行中のスレッド、タスクの停止を要請する
        /// </summary>
        /// <returns>まだ実行中: False, 停止完了: True</returns>
        public bool StopThreadAndTask()
        {
            if (loadContentsBackgroundWorker is not {IsBusy: true})
            {
                return true;
            }

            loadContentsBackgroundWorker.CancelAsync();
            return false;

        }

        /// <summary>
        /// Bluetoothボタンを押下時の処理
        /// </summary>
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
                App.ShowErrorMessageBox("Not support Bluetooth transmission.", "Bluetooth transmission error");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// エクスプローラーを開くボタンを押下時の処理
        /// </summary>
        private void OpenFolderButtonClicked()
        {
            if (string.IsNullOrEmpty(SelectFolderPath.Value))
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var selectPath = (File.GetAttributes(SelectFolderPath.Value) & FileAttributes.Directory) == FileAttributes.Directory 
                    ? SelectFolderPath.Value : Path.GetDirectoryName(SelectFolderPath.Value);

                const string Explorer = "EXPLORER.EXE";
                if (!string.IsNullOrEmpty(selectPath))
                {
                    Process.Start(Explorer, selectPath);
                }
                else
                {
                    App.ShowErrorMessageBox("Select path is not found.", "Process start error");
                }
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
        /// 再読み込みボタンを押下時の処理
        /// </summary>
        private void ReloadButtonClicked()
        {
            // Exif情報、画像表示をクリア
            PictureImageSource.Value = null;
            ExifInfoViewModel.ClearExif();

            // 編集ボタンを非活性にする
            IsEnableImageEditButton.Value = false;

            // ディレクトリパスを表示する
            if ((File.GetAttributes(SelectFolderPath.Value) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                SelectFolderPath.Value = Path.GetDirectoryName(SelectFolderPath.Value);
            }
            UpdateContents();
        }

        /// <summary>
        /// 設定ボタンを押下時の処理
        /// </summary>
        private void SettingButtonClicked()
        {
            var vm = new SettingViewModel();
            vm.ReloadContextMenuEvent += ReloadContextMenu;

            var settingDialog = new SettingView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            settingDialog.ShowDialog();
        }

        /// <summary>
        /// 編集ボタン押下時の処理
        /// </summary>
        private void ImageEditButtonClicked()
        {
            if (SelectedMedia.Value == null)
            {
                return;
            }

            var vm = new ImageEditToolViewModel();
            vm.SetEditFileData(SelectedMedia.Value.FilePath);

            var imageEditToolDialog = new ImageEditToolView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            imageEditToolDialog.ShowDialog();
        }

        /// <summary>
        /// コンテキストメニューの再読み込み
        /// </summary>
        /// <param name="sender">SettingViewModel</param>
        /// <param name="e">引数情報</param>
        private void ReloadContextMenu(object sender, EventArgs e)
        {
            // コンテキストメニューをクリア
            ContextMenuCollection.Clear();
            IsShowContextMenu.Value = false;

            // 登録アプリをコンテキストメニューに再登録
            var linkageAppList = AppConfigManager.GetInstance().ConfigData.LinkageAppList;
            if (linkageAppList == null || !linkageAppList.Any())
            {
                return;
            }

            foreach (var linkageApp in linkageAppList)
            {
                // 登録アプリのアイコンを取得
                var appIcon = Icon.ExtractAssociatedIcon(linkageApp.AppPath);
                if (appIcon != null)
                {
                    var iconBitmapSource = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    // Set context menu.
                    var contextMenu = new ContextMenuInfo { DisplayName = linkageApp.AppName, ContextIcon = iconBitmapSource };
                    ContextMenuCollection.Add(contextMenu);
                }

                IsShowContextMenu.Value = true;
            }
        }

        /// <summary>
        /// エクスプローラーツリーの選択が変更された時の処理
        /// </summary>
        /// <param name="sender">ExplorerViewModel</param>
        /// <param name="e">引数情報</param>
        private void ExplorerViewModel_ChangeSelectItemEvent(object sender, EventArgs e)
        {
            SelectedMedia.Value = null;
            PictureImageSource.Value = null;
            ExifInfoViewModel.ClearExif();
            IsEnableImageEditButton.Value = false;

            var selectedExplorerItem = ExplorerViewModel.SelectedItem;
            ChangeContents(selectedExplorerItem.ExplorerItemPath);
        }

        /// <summary>
        /// エクスプローラーツリーの表示更新
        /// </summary>
        private void UpdateExplorerTree()
        {
            ExplorerViewModel.CreateDriveTreeItem();

            var previousFolderPath = DefaultPicturePath;
            if (!string.IsNullOrEmpty(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath) && Directory.Exists(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath))
            {
                previousFolderPath = AppConfigManager.GetInstance().ConfigData.PreviousFolderPath;
            }
            ExplorerViewModel.ExpandPreviousPath(previousFolderPath);
        }

        /// <summary>
        /// 画像フォルダパスが変更された時にコンテンツリストの画像パスを変更する
        /// </summary>
        /// <param name="folderPath">画像フォルダパス</param>
        private void ChangeContents(string folderPath)
        {
            if (!Directory.Exists(folderPath) || SelectFolderPath.Value == folderPath)
            {
                return;
            }

            SelectFolderPath.Value = folderPath;
            UpdateContents();

            AppConfigManager.GetInstance().ConfigData.PreviousFolderPath = SelectFolderPath.Value;
        }

        /// <summary>
        /// コンテンツリストを更新する
        /// </summary>
        private void UpdateContents()
        {
            if (loadContentsBackgroundWorker is {IsBusy: true})
            {
                loadContentsBackgroundWorker.CancelAsync();
                isReloadContents = true;
                return;
            }

            LoadContentsList();
        }

        /// <summary>
        /// コンテンツリストの読み込み処理
        /// </summary>
        private void LoadContentsList()
        {
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
        /// コンテンツの読み込み処理(別スレッドで動作する)
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadContentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                LoadContentsWorker(sender, e);
            }
            catch (Exception ex)
            {
                App.LogException(ex);

                const string MediaReadErrorMessage = "Failed to load media file.";
                const string MediaReadErrorTitle = "File read error";
                App.ShowErrorMessageBox(MediaReadErrorMessage, MediaReadErrorTitle);
            }

            App.RunGc();
        }

        /// <summary>
        /// 画像読み込み処理が完了、キャンセルした場合の処理
        /// </summary>
        /// <param name="sender">LoadContentsWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadContentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                loadContentsBackgroundWorker?.Dispose();

                // 再読み込み時は、読み込み処理を再開
                if (isReloadContents)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        UpdateContents();
                        isReloadContents = false;
                    }), DispatcherPriority.Normal);
                }
            }
            else
            {
                loadContentsBackgroundWorker?.Dispose();

                if (SelectedMedia == null && MediaInfoList.Any())
                {
                    SelectedMedia.Value = MediaInfoList[0];
                }
            }
        }

        /// <summary>
        /// 画像読み込み処理の実処理
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadContentsWorker(object sender, CancelEventArgs e)
        {
            var queue = new LinkedList<MediaInfo>();
            var tick = Environment.TickCount;
            var count = 0;

            // 選択されたフォルダ内でサポート対象の拡張子を順番にチェック
            foreach (var supportExtension in MediaChecker.GetSupportExtentions())
            {
                var folderPath = SelectFolderPath.Value;
                if ((File.GetAttributes(folderPath) & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    folderPath = Path.GetDirectoryName(folderPath);
                }

                if (string.IsNullOrEmpty(folderPath))
                {
                    continue;
                }

                // サポート対象のファイルを順番に読み込む
                foreach (var supportFile in Directory.EnumerateFiles(folderPath, $"*{supportExtension}").OrderBy(Path.GetFileName))
                {
                    if (sender is BackgroundWorker {CancellationPending: true})
                    {
                        e.Cancel = true;
                        return;
                    }

                    var mediaInfo = new MediaInfo
                    {
                        FilePath = supportFile
                    };
                    mediaInfo.FileName = Path.GetFileName(mediaInfo.FilePath);

                    if (!mediaInfo.CreateThumbnailImage())
                    {
                        continue;
                    }

                    queue.AddLast(mediaInfo);
                    count++;

                    if (!queue.Any())
                    {
                        continue;
                    }

                    var duration = Environment.TickCount - tick;
                    if ((count > 50 || duration <= 250) && duration <= 500)
                    {
                        continue;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (sender is BackgroundWorker {CancellationPending: true})
                        {
                            e.Cancel = true;
                            return;
                        }

                        MediaInfoList.AddRange(queue);

                        // 選択中のメディアがない場合は、リストの最初のアイテムを選択する
                        if (!MediaInfoList.Any() || SelectedMedia.Value != null)
                        {
                            return;
                        }

                        var firstImageData = MediaInfoList.First();
                        if (!MediaChecker.CheckRawFileExtension(Path.GetExtension(firstImageData.FilePath)?.ToLower()))
                        {
                            SelectedMedia.Value = firstImageData;
                        }
                    });

                    queue.Clear();
                    tick = Environment.TickCount;
                }
            }

            if (queue.Any())
            {
                Application.Current.Dispatcher.Invoke(() => { MediaInfoList.AddRange(queue); });
            }
        }

        /// <summary>
        /// 選択されたメディア情報を非同期で読み込み、画像、Exifを表示する
        /// </summary>
        /// <param name="mediaInfo">選択されたメディア情報</param>
        /// <returns>読み込み成功: True、読み込み失敗: False</returns>
        private async Task<bool> LoadPictureImageAsync(MediaInfo mediaInfo)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // 画像とExifを読み込むタスクを作成する
                var loadPictureTask = Task.Run(() => { PictureImageSource.Value = ImageController.CreatePictureViewImage(mediaInfo.FilePath); });
                var setExifInfoTask = Task.Run(() => { ExifInfoViewModel.SetExif(mediaInfo.FilePath); });

                // タスクを実行し、処理完了まで待つ
                await Task.WhenAll(loadPictureTask, setExifInfoTask);

                // 編集ボタンの状態を更新(Raw画像以外は活性状態とする)
                IsEnableImageEditButton.Value = !MediaChecker.CheckRawFileExtension(Path.GetExtension(mediaInfo.FilePath)?.ToLower());

                // パス表示を更新
                SelectFolderPath.Value = mediaInfo.FilePath;

                // WritableBitmapのメモリを解放
                App.RunGc();

                return true;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("File access error occurred", "File access error");

                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}