using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Prism.Mvvm;
using PhotoViewer.Model;


namespace PhotoViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region ViewModels
        public ExplorerViewModel ExplorerViewModel { get; set; }
        #endregion

        #region UI binding parameters
        private string selectFolderPath;
        /// <summary>
        /// 表示中のフォルダパス
        /// </summary>
        public string SelectFolderPath
        {
            get { return selectFolderPath; }
            set { SetProperty(ref selectFolderPath, value); }
        }

        private ObservableCollection<MediaInfo> mediaInfoList = new ObservableCollection<MediaInfo>();
        public ObservableCollection<MediaInfo> MediaInfoList
        {
            get { return mediaInfoList; }
            set { SetProperty(ref mediaInfoList, value); }
        }
        #endregion

        // メディア情報の読み込みスレッド
        private BackgroundWorker LoadContentsBackgroundWorker;
        // メディアリストのリロードフラグ
        private bool IsReloadContents;

        public MainWindowViewModel()
        {
            ExplorerViewModel = new ExplorerViewModel();
            ExplorerViewModel.ChangeSelectItemEvent += ExplorerViewModel_ChangeSelectItemEvent;
            UpdateExplorerTree();

            string defaultPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);
            ChangeContents(defaultPicturePath);
        }

        /// <summary>
        /// ExplorerViewでフォルダ選択変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerViewModel_ChangeSelectItemEvent(object sender, EventArgs e)
        {
            var selectedExplorerItem = ExplorerViewModel.SelectedItem;
            ChangeContents(selectedExplorerItem.ExplorerItemPath);
        }

        /// <summary>
        /// エクスプローラーのツリー表示を更新
        /// </summary>
        private void UpdateExplorerTree()
        {
            List<DriveInfo> allDriveList = DriveInfo.GetDrives().ToList();
            ExplorerViewModel.CreateDriveTreeItem(allDriveList);
        }

        /// <summary>
        /// メディアリストに表示するフォルダを変更
        /// </summary>
        /// <param name="folderPath">メディアリストを表示するフォルダパス</param>
        private void ChangeContents(string folderPath)
        {
            if (!Directory.Exists(folderPath) || SelectFolderPath == folderPath)
            {
                return;
            }

            // フォルダパスを更新して、リスト更新
            SelectFolderPath = folderPath;
            UpdateContents();
        }

        private void UpdateContents()
        {
            if (LoadContentsBackgroundWorker != null && LoadContentsBackgroundWorker.IsBusy)
            {
                // すでにスレッド動作中の場合、一旦スレッド処理をキャンセルし、キャンセル後に再実行する
                LoadContentsBackgroundWorker.CancelAsync();
                IsReloadContents = true;
                return;
            }

            LoadContentsList();
        }

        private void LoadContentsList()
        {
            // 読み込み前に表示リストをクリア
            MediaInfoList.Clear();

            // 時間がかかるため、別スレッドでメディアリストを読み込む
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += LoadContentsWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += LoadContentsWorker_RunWorkerCompleted;

            // 読み込みスレッド開始
            LoadContentsBackgroundWorker = backgroundWorker;
            LoadContentsBackgroundWorker.RunWorkerAsync();
        }

        private void LoadContentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                LoadContentsWorker(sender, e);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("メディアの読み込みに失敗しました。", "読み込みエラー");
            }
        }

        /// <summary>
        /// コンテンツ読み込みスレッドでの処理完了時
        /// </summary>
        /// <param name="sender">LoadContentsWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadContentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StopContentsWorker();

                if (IsReloadContents)
                {
                    App.Current.Dispatcher.BeginInvoke((Action)(() => 
                    { 
                        UpdateContents();
                        IsReloadContents = false;
                    }), DispatcherPriority.Normal);
                }
            }
            else
            {
                StopContentsWorker();
            }
        }

        private void LoadContentsWorker(object sender, DoWorkEventArgs e)
        {
            List<string> filePaths = new List<string>();
            int tick = Environment.TickCount;

            // 選択されたフォルダ内のサポートされるファイルを全て取得
            foreach (string supportExtension in MediaChecker.GetSupportExtentions())
            {
                var worker = sender as BackgroundWorker;
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                filePaths.AddRange(Directory.GetFiles(SelectFolderPath, "*" + supportExtension).ToList());
            }

            
            var readyFiles = new Queue<MediaInfo>();
            foreach (var filePath in filePaths)
            {
                var worker = sender as BackgroundWorker;
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var mediaInfo = new MediaInfo();
                mediaInfo.FilePath = filePath;
                mediaInfo.FileName = Path.GetFileName(mediaInfo.FilePath);
                try
                {
                    mediaInfo.CreateThumbnailImage();
                }
                catch { }

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

            if (readyFiles.Count > 0)
            {
                App.Current.Dispatcher.Invoke((Action)(() => { foreach (var readyFile in readyFiles) MediaInfoList.Add(readyFile); }));
            }
        }

        /// <summary>
        /// コンテンツ読み込みスレッドの完全停止
        /// </summary>
        private void StopContentsWorker()
        {
            if (LoadContentsBackgroundWorker != null)
            {
                LoadContentsBackgroundWorker.Dispose();
            }
        }
    }
}
