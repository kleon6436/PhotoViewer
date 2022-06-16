using Kchary.PhotoViewer.Data;
using Kchary.PhotoViewer.Models;
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
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class MainWindowViewModel : BindableBase, IDisposable
    {
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

        /// <summary>
        /// コンテキストメニューのコマンド
        /// </summary>
        public ReactiveCommand<string> ContextMenuCommand { get; }

        #endregion Command

        /// <summary>
        /// IDisposableをまとめるCompositeDisposable
        /// </summary>
        private readonly CompositeDisposable disposables = new();

        /// <summary>
        /// バックグラウンドでコンテンツをロードするためのワーカー
        /// </summary>
        private static BackgroundWorker LoadContentsBackgroundWorker { get; } = new() { WorkerSupportsCancellation = true };

        /// <summary>
        /// メディアロードタスクリスト
        /// </summary>
        private static Task[] LoadMediaTasks;

        /// <summary>
        /// ロード停止フラグ
        /// </summary>
        private static volatile bool StopLoading;

        /// <summary>
        /// メディアのロード中フラグ
        /// </summary>
        private static bool LoadingMedia;

        /// <summary>
        /// コンテンツをリロードするためのフラグ
        /// </summary>
        private static bool IsReloadContents { get; set; }

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
            BluetoothButtonCommand = new ReactiveCommand().WithSubscribe(BluetoothButtonClicked).AddTo(disposables);
            OpenFolderButtonCommand = new ReactiveCommand().WithSubscribe(OpenFolderButtonClicked).AddTo(disposables);
            ReloadButtonCommand = new ReactiveCommand().WithSubscribe(ReloadButtonClicked).AddTo(disposables);
            SettingButtonCommand = new ReactiveCommand().WithSubscribe(SettingButtonClicked).AddTo(disposables);
            ImageEditButtonCommand = new ReactiveCommand().WithSubscribe(ImageEditButtonClicked).AddTo(disposables);
            ContextMenuCommand = new ReactiveCommand<string>().WithSubscribe(ContextMenuClicked).AddTo(disposables);

            // プロパティ変更に紐づく処理の設定
            SelectedMedia.Subscribe(LoadMedia).AddTo(disposables);

            // バックグラウンドスレッドの設定
            LoadContentsBackgroundWorker.DoWork += LoadContentsDoWork;
            LoadContentsBackgroundWorker.RunWorkerCompleted += RunWorkerCompleted;

            // 設定ファイルの読み込み
            AppConfigManager.GetInstance().Import();

            // エクスプローラーツリーの設定
            ExplorerViewModel = new ExplorerViewModel();
            disposables.Add(ExplorerViewModel);
            ExplorerViewModel.ChangeSelectItemEvent += ChangeSelectItemEvent;
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
            if (linkageAppList?.Any() == true)
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
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="mediaInfo">選択されたメディア情報</param>
        public void LoadMedia(MediaInfo mediaInfo)
        {
            if (mediaInfo == null)
            {
                return;
            }

            if (LoadingMedia)
            {
                StopLoading = true;
                if (LoadMediaTasks is not null)
                {
                    foreach (var task in LoadMediaTasks)
                    {
                        task.Wait();
                    }
                }
                StopLoading = false;
            }

            if (!File.Exists(mediaInfo.FilePath))
            {
                App.ShowErrorMessageBox("File not exist.", "File access error");
                return;
            }

            IsEnableImageEditButton.Value = false;
            LoadingMedia = true;

            switch (mediaInfo.ContentMediaType)
            {
                case MediaInfo.MediaType.Picture:
                    LoadPictureImage(mediaInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(mediaInfo.ContentMediaType.ToString(), nameof(mediaInfo.ContentMediaType));
            }

            LoadingMedia = false;
        }

        /// <summary>
        /// 実行中のスレッド、タスクの停止を要請する
        /// </summary>
        /// <returns>まだ実行中: False, 停止完了: True</returns>
        public static bool StopThreadAndTask()
        {
            if (LoadContentsBackgroundWorker is not { IsBusy: true })
            {
                return true;
            }

            LoadContentsBackgroundWorker.CancelAsync();
            return false;
        }

        /// <summary>
        /// Bluetoothボタンを押下時の処理
        /// </summary>
        private static void BluetoothButtonClicked()
        {
            App.CallMouseBlockMethod(() => Process.Start("fsquirt.exe", "-send"),
            "Bluetooth transmission error", "Not support Bluetooth transmission.");
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

            App.CallMouseBlockMethod(() =>
            {
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
            },
            "Open folder error", "Explorer is not started.");
        }

        /// <summary>
        /// 再読み込みボタンを押下時の処理
        /// </summary>
        private void ReloadButtonClicked()
        {
            // Exif情報、画像表示をクリア
            PictureImageSource.Value = null;
            SelectedMedia.Value = null;
            ExifInfoViewModel.ClearExif();

            // 編集ボタンを非活性にする
            IsEnableImageEditButton.Value = false;

            // ディレクトリパスを表示する
            if ((File.GetAttributes(SelectFolderPath.Value) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                SelectFolderPath.Value = Path.GetDirectoryName(SelectFolderPath.Value);
            }

            ExplorerViewModel.UpdateDriveTreeItem();
            ExplorerViewModel.ExpandPreviousPath(ExplorerViewModel.ShowExplorerPath);
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

            var vm = new ResizeImageViewModel();
            vm.SetEditFileData(SelectedMedia.Value.FilePath);

            var imageEditToolDialog = new ImageEditToolView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            imageEditToolDialog.ShowDialog();
        }

        /// <summary>
        /// コンテキストメニューを押下時の処理
        /// </summary>
        /// <param name="appName">アプリ名</param>
        private void ContextMenuClicked(string appName)
        {
            var linkageAppList = AppConfigManager.GetInstance().ConfigData.LinkageAppList;
            if (linkageAppList.All(x => x.AppName != appName))
            {
                return;
            }

            App.CallMouseBlockMethod(() =>
            {
                var appPath = linkageAppList.Find(x => x.AppName == appName)?.AppPath;
                if (!string.IsNullOrEmpty(appPath))
                {
                    Process.Start(appPath, SelectedMedia.Value.FilePath);
                }
                else
                {
                    App.ShowErrorMessageBox("Linkage app path is not found.", "Process start error");
                }
            },
            "Process start error", "Linked app is not started.");
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
            if (linkageAppList?.Any() != true)
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
        private void ChangeSelectItemEvent(object sender, EventArgs e)
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
            var previousFolderPath = DefaultPicturePath;
            if (!string.IsNullOrEmpty(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath) && Directory.Exists(AppConfigManager.GetInstance().ConfigData.PreviousFolderPath))
            {
                previousFolderPath = AppConfigManager.GetInstance().ConfigData.PreviousFolderPath;
            }
            ExplorerViewModel.UpdateDriveTreeItem();
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
            if (LoadContentsBackgroundWorker is { IsBusy: true })
            {
                LoadContentsBackgroundWorker.CancelAsync();
                IsReloadContents = true;
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
            LoadContentsBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// コンテンツの読み込み処理(別スレッドで動作する)
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadContentsDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                LoadContentsWorker(sender, e);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Failed to load media file.", "File read error");
            }
        }

        /// <summary>
        /// 画像読み込み処理が完了、キャンセルした場合の処理
        /// </summary>
        /// <param name="sender">LoadContentsWorker</param>
        /// <param name="e">引数情報</param>
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                LoadContentsBackgroundWorker?.Dispose();

                // 再読み込み時は、読み込み処理を再開
                if (IsReloadContents)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateContents();
                        IsReloadContents = false;
                    });
                }
            }
            else
            {
                LoadContentsBackgroundWorker?.Dispose();

                if (SelectedMedia.Value == null && MediaInfoList.Any())
                {
                    SelectedMedia.Value = MediaInfoList.ElementAt(0);
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
            var folderPath = SelectFolderPath.Value;
            if ((File.GetAttributes(folderPath) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }

            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            var queue = new LinkedList<MediaInfo>();
            var tick = Environment.TickCount;
            var count = 0;

            // 選択されたフォルダ内でサポート対象の拡張子を順番にチェック
            foreach (var supportExtension in MediaChecker.GetSupportExtentions())
            {
                // サポート対象のファイルを順番に読み込む
                foreach (var supportFile in Directory.EnumerateFiles(folderPath, $"*{supportExtension}").OrderBy(Path.GetFileName))
                {
                    if (sender is BackgroundWorker { CancellationPending: true })
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

                    if (queue.Count == 0)
                    {
                        continue;
                    }

                    var duration = Environment.TickCount - tick;
                    if ((count > 100 || duration <= 250) && duration <= 500)
                    {
                        continue;
                    }

                    if (sender is BackgroundWorker { CancellationPending: true })
                    {
                        e.Cancel = true;
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var queueData in queue)
                        {
                            MediaInfoList.Add(queueData);

                            // 選択中のメディアがない場合は、リストの最初のアイテムを選択する
                            if (SelectedMedia.Value == null)
                            {
                                SelectedMedia.Value = MediaInfoList.First();
                            }
                        }
                        queue.Clear();
                        tick = Environment.TickCount;
                    });
                }
            }

            if (sender is BackgroundWorker { CancellationPending: true })
            {
                e.Cancel = true;
                return;
            }

            if (queue.Count == 0)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() => MediaInfoList.AddRange(queue));
            queue.Clear();
        }

        /// <summary>
        /// 選択されたメディア情報を非同期で読み込み、画像、Exifを表示する
        /// </summary>
        /// <param name="mediaInfo">選択されたメディア情報</param>
        /// <returns>読み込み成功: True、読み込み失敗: False</returns>
        private void LoadPictureImage(MediaInfo mediaInfo)
        {
            App.CallMouseBlockMethod(async () =>
            {
                // 画像とExifを読み込むタスクを作成する
                var loadPictureTask = Task.Run(() =>
                {
                    if (StopLoading)
                    {
                        return;
                    }
                    PictureImageSource.Value = ImageController.CreatePictureViewImage(mediaInfo.FilePath, StopLoading);
                });
                var setExifInfoTask = Task.Run(() =>
                {
                    if (StopLoading)
                    {
                        return;
                    }
                    ExifInfoViewModel.SetExif(mediaInfo.FilePath, StopLoading);
                });

                // タスクを実行し、処理完了まで待つ
                LoadMediaTasks = new[] { loadPictureTask, setExifInfoTask };
                await Task.WhenAll(LoadMediaTasks);

                // 編集ボタンの状態を更新(Raw画像以外は活性状態とする)
                IsEnableImageEditButton.Value = !MediaChecker.CheckRawFileExtension(Path.GetExtension(mediaInfo.FilePath)?.ToLower());

                // パス表示を更新
                SelectFolderPath.Value = mediaInfo.FilePath;
            },
            "File access error", "File access error occurred");
        }
    }
}