using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using Kchary.PhotoViewer.Models;
using Kchary.PhotoViewer.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject, IDisposable
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

        #region UI binding parameters

        /// <summary>
        /// 選択されたフォルダパス(絶対パス)
        /// </summary>
        public ReactivePropertySlim<string> SelectFolderPath { get; } = new();

        /// <summary>
        /// 選択した画像ファイルデータ
        /// </summary>
        public ReactivePropertySlim<PhotoInfo> SelectedMedia { get; } = new();

        /// <summary>
        /// 表示する画像データ
        /// </summary>
        public ReactivePropertySlim<BitmapSource> PictureImageSource { get; } = new();

        /// <summary>
        /// 編集ボタンの有効無効フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsEnableImageEditButton { get; } = new();

        #endregion UI binding parameters

        /// <summary>
        /// コンテキストメニューの管理クラスインスタンス
        /// </summary>
        public ContextMenuCollection ContextMenuCollection { get; } = new();

        /// <summary>
        /// 写真フォルダをロードするためのクラスインスタンス
        /// </summary>
        public PhotoFolderLoader PhotoFolderLoader { get; } = new();

        /// <summary>
        /// IDisposableをまとめるCompositeDisposable
        /// </summary>
        private readonly CompositeDisposable disposables = new();

        /// <summary>
        /// 1枚の写真をロードするためのクラスインスタンス
        /// </summary>
        private readonly PhotoLoader photoLoader;

        /// <summary>
        /// Exif情報をロードするためのクラスインスタンス
        /// </summary>
        private readonly ExifLoader exifLoader;

        /// <summary>
        /// 既定のピクチャフォルダパス
        /// </summary>
        /// <remarks>
        /// パブリックユーザーのピクチャフォルダを既定とする
        /// </remarks>
        private readonly string defaultPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

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

            // 写真フォルダのローダーの設定
            PhotoFolderLoader.FirstImageLoaded += FirstPhotoReadied;
            PhotoFolderLoader.FolderLoadCompleted += FirstPhotoReadied;

            // 設定ファイルの読み込み
            AppConfig.GetInstance().Import();

            // モデルの準備
            exifLoader = new ExifLoader();
            photoLoader = new PhotoLoader(exifLoader);

            // 画像フォルダの読み込み
            var picturePath = defaultPicturePath;
            if (FileUtil.CheckFolderPath(AppConfig.GetInstance().PreviousFolderPath))
            {
                picturePath = AppConfig.GetInstance().PreviousFolderPath;
            }
            ChangePhotoFolder(picturePath);

            // エクスプローラーツリーの設定
            ExplorerViewModel = new ExplorerViewModel();
            disposables.Add(ExplorerViewModel);
            ExplorerViewModel.ChangeSelectItemEvent += ChangeSelectItemEvent;
            UpdateExplorerTree();

            // Exif情報表示の設定
            ExifInfoViewModel = new ExifInfoViewModel();

            // コンテキストメニューの設定
            ContextMenuCollection.SetContextMenuFromConfigData();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => disposables.Dispose();

        /// <summary>
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="photoInfo">選択されたメディア情報</param>
        public void LoadMedia(PhotoInfo photoInfo)
        {
            if (photoInfo == null)
            {
                return;
            }

            App.CallMouseBlockMethod(async () =>
            {
                IsEnableImageEditButton.Value = false;

                photoLoader.PhotoInfo = photoInfo;
                (BitmapSource image, ExifInfo[] exifInfos) = await photoLoader.LoadPhoto();

                PictureImageSource.Value = image;
                IsEnableImageEditButton.Value = !photoInfo.IsRawImage;  // 読み込んだ画像がRaw画像でないときは編集可能

                ExifInfoViewModel.SetExif(exifInfos);
            },
            "File access error", "File access error occured.");
        }

        /// <summary>
        /// 実行中のスレッド、タスクの停止を要請する
        /// </summary>
        /// <returns>まだ実行中: False, 停止完了: True</returns>
        public bool RequestStopThreadAndTask()
        {
            return PhotoFolderLoader.RequestStopThreadAndTask();
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
                var selectPath = FileUtil.IsDirectory(SelectFolderPath.Value)
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
            if (!FileUtil.IsDirectory(SelectFolderPath.Value))
            {
                SelectFolderPath.Value = Path.GetDirectoryName(SelectFolderPath.Value);
            }

            ExplorerViewModel.UpdateDriveTreeItem();
            ExplorerViewModel.ExpandPreviousPath(ExplorerViewModel.ShowExplorerPath);
            PhotoFolderLoader.UpdatePhotoList();
        }

        /// <summary>
        /// 設定ボタンを押下時の処理
        /// </summary>
        private void SettingButtonClicked()
        {
            var vm = new SettingViewModel();
            vm.ReloadContextMenuEvent += ContextMenuCollection.ReloadContextMenu;

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
            vm.SetEditFileData(SelectedMedia.Value);

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
            var linkageAppList = AppConfig.GetInstance().GetAvailableRegisterApps();
            if (linkageAppList.All(x => x.AppName != appName))
            {
                return;
            }

            App.CallMouseBlockMethod(() =>
            {
                var appPath = Array.Find(linkageAppList, x => x.AppName == appName)?.AppPath;
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
            ChangePhotoFolder(selectedExplorerItem.ExplorerItemPath);
        }

        /// <summary>
        /// エクスプローラーツリーの表示更新
        /// </summary>
        private void UpdateExplorerTree()
        {
            var previousFolderPath = defaultPicturePath;
            if (FileUtil.CheckFolderPath(AppConfig.GetInstance().PreviousFolderPath))
            {
                previousFolderPath = AppConfig.GetInstance().PreviousFolderPath;
            }
            ExplorerViewModel.UpdateDriveTreeItem();
            ExplorerViewModel.ExpandPreviousPath(previousFolderPath);
        }

        /// <summary>
        /// 画像フォルダパスが変更された時に写真リストの画像パスを変更する
        /// </summary>
        /// <param name="folderPath">画像フォルダパス</param>
        private void ChangePhotoFolder(string folderPath)
        {
            try
            {
                if (PhotoFolderLoader.ChangePhotoFolder(folderPath))
                {
                    // フォルダを切り替えたら、フォルダパス情報を更新
                    SelectFolderPath.Value = folderPath;
                    AppConfig.GetInstance().PreviousFolderPath = folderPath;
                }
            }
            catch
            {
                App.ShowErrorMessageBox("Failed to load photo folder.", "Folder load error");
            }
        }

        /// <summary>
        /// リストに最初の画像が準備できたときのイベント
        /// </summary>
        /// <param name="sender">PhotoFolderLoader</param>
        /// <param name="e">引数情報</param>
        /// <remarks>
        /// 最初の1枚を選んで選択状態にする
        /// </remarks>
        private void FirstPhotoReadied(object sender, EventArgs e)
        {
            if (SelectedMedia.Value == null && PhotoFolderLoader.PhotoList.Any())
            {
                Application.Current.Dispatcher.BeginInvoke(() => SelectedMedia.Value = PhotoFolderLoader.PhotoList[0]);
            }
        }
    }
}