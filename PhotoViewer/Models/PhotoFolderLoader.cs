using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kchary.PhotoViewer.Models
{
    public sealed class PhotoFolderLoader : ObservableObject
    {
        /// <summary>
        /// フォルダ内の最初の画像を追加できたときに発火するイベント
        /// </summary>
        public event EventHandler FirstImageLoaded;

        /// <summary>
        /// フォルダ内をすべて追加し終えた後に発火するイベント
        /// </summary>
        public event EventHandler FolderLoadCompleted;

        /// <summary>
        /// 読み込むフォルダパス
        /// </summary>
        private string folderPath;

        /// <summary>
        /// バックグラウンドでコンテンツをロードするためのワーカー
        /// </summary>
        private readonly BackgroundWorker loadPhotoFolderWorker = new() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        /// <summary>
        /// コンテンツをリロードするためのフラグ
        /// </summary>
        private bool isReloadContents;

        /// <summary>
        /// 写真一覧リスト
        /// </summary>
        public ObservableCollection<PhotoInfo> PhotoList { get; } = [];

        /// <summary>
        /// 画像読み込みフラグ
        /// </summary>
        private bool firstImageLoaded;

        /// <summary>
        /// サムネイル画像生成処理のキャンセルトークン
        /// </summary>
        private CancellationTokenSource thumbnailLoadCts = new();

        /// <summary>
        /// サムネイル画像を作成するための画像情報キュー
        /// </summary>
        private readonly Queue<PhotoInfo> thumbnailQueue = new();

        /// <summary>
        /// サムネイル画像を非同期で作成するためのタイマー処理
        /// </summary>
        private readonly System.Timers.Timer thumbnailTimer;

        /// <summary>
        /// 1フレームあたりのサムネイル生成数
        /// </summary>
        private const int MaxThumbnailsPerTick = 5;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhotoFolderLoader()
        {
            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(PhotoList, new object());

            // サムネイル読み込み用のタイマースレッドの設定
            thumbnailTimer = new System.Timers.Timer(150);  // 150msはネットワークドライブからの画像読み込み速度を考慮して設定
            thumbnailTimer.Elapsed += ThumbnailTimer_Elapsed;

            // バックグラウンドスレッドの設定
            loadPhotoFolderWorker.DoWork += LoadPhotoFolderDoWork;
            loadPhotoFolderWorker.ProgressChanged += LoadPhotoFolderProgressChanged;
            loadPhotoFolderWorker.RunWorkerCompleted += RunWorkerCompleted;
        }

        /// <summary>
        /// タイマーで動作するサムネイル画像生成処理
        /// </summary>
        /// <param name="sender">タイマー</param>
        /// <param name="e">引数情報</param>
        private async void ThumbnailTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<PhotoInfo> itemsToLoad = [];

            lock (thumbnailQueue)
            {
                while (thumbnailQueue.Count > 0 && itemsToLoad.Count < MaxThumbnailsPerTick)
                {
                    if (thumbnailLoadCts.Token.IsCancellationRequested)
                    {
                        // キャンセルされたらループを即座に抜ける
                        break;
                    }
                    itemsToLoad.Add(thumbnailQueue.Dequeue());
                }
            }

            // 読み込んだサムネイルをためる
            List<(PhotoInfo photo, BitmapSource thumbnail)> loadedThumbnails = [];
            foreach (var photo in itemsToLoad)
            {
                if (thumbnailLoadCts.Token.IsCancellationRequested)
                {
                    // キャンセルされたらループを即座に抜ける
                    break;
                }

                try
                {
                    var thumbnail = await ImageUtil.LoadThumbnailAsync(photo, thumbnailLoadCts.Token);
                    if (thumbnail != null)
                    {
                        loadedThumbnails.Add((photo, thumbnail));
                    }
                }
                catch (Exception ex)
                {
                    App.LogException(ex);
                }
            }

            // ある程度たまったら、UI側で表示処理する
            if (loadedThumbnails.Count > 0)
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var (photo, thumbnail) in loadedThumbnails)
                        {
                            photo.ThumbnailImage = thumbnail;
                        }
                    }, DispatcherPriority.Normal, thumbnailLoadCts.Token);
                }
                catch (Exception ex)
                {
                    App.LogException(ex);
                }
            }
        }

        /// <summary>
        /// 画像フォルダパスが変更された時に写真リストの画像パスを変更する
        /// </summary>
        /// <param name="folderPath">画像フォルダパス</param>
        /// <returns>True: フォルダ変更した, False: フォルダ変更していない</returns>
        public bool ChangePhotoFolder(string folderPath)
        {
            if (!FileUtil.CheckFolderPath(folderPath))
            {
                throw new Exception("Folder is not found.");
            }

            if (this.folderPath == folderPath)
            {
                // 同じフォルダの時は、フォルダ変更しない
                return false;
            }

            this.folderPath = folderPath;

            UpdatePhotoList();
            return true;
        }

        /// <summary>
        /// 設定されているフォルダパスを読み込み、リストを更新する
        /// </summary>
        public void UpdatePhotoList()
        {
            CancelThumbnailLoad();

            if (!RequestStopThreadAndTask())
            {
                isReloadContents = true;
                return;
            }

            PhotoList.Clear();
            thumbnailLoadCts?.Dispose();
            thumbnailLoadCts = new CancellationTokenSource();
            loadPhotoFolderWorker.RunWorkerAsync();
        }

        /// <summary>
        /// 実行中のスレッド、タスクの停止を要請する
        /// </summary>
        /// <returns>まだ実行中: False, 停止完了: True</returns>
        public bool RequestStopThreadAndTask()
        {
            if (loadPhotoFolderWorker is not { IsBusy: true })
            {
                return true;
            }

            loadPhotoFolderWorker.CancelAsync();
            return false;
        }

        /// <summary>
        /// サムネイル画像の読み込み処理をキャンセル
        /// </summary>
        public void CancelThumbnailLoad()
        {
            // キャンセルトークンを発行
            thumbnailLoadCts.Cancel();

            // タイマーを停止し、キューをクリーンアップ
            thumbnailTimer.Stop();
            lock (thumbnailQueue)
            {
                thumbnailQueue.Clear();
            }
        }

        /// <summary>
        /// スレッドでの読み込み処理
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadPhotoFolderDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            if (!FileUtil.IsDirectory(folderPath))
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }

            if (!FileUtil.CheckFolderPath(folderPath))
            {
                return;
            }

            var fileList = Const.SupportPictureExtensions
                .SelectMany(ext => new DirectoryInfo(folderPath).EnumerateFiles($"*{ext}", SearchOption.TopDirectoryOnly))
                .OrderBy(f => f, new NaturalFileInfoNameComparer())
                .ToList();

            const int batchSize = 20;
            var batch = new List<PhotoInfo>(batchSize);
            foreach (var file in fileList)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    batch.Add(new PhotoInfo(file.FullName));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ファイル読み込み失敗: {file.FullName} ({ex.Message})");
                    continue;
                }

                if (batch.Count >= batchSize)
                {
                    worker.ReportProgress(0, new List<PhotoInfo>(batch));
                    batch.Clear();
                }
            }

            // 最後に残った分を送信
            if (batch.Count > 0)
            {
                worker.ReportProgress(0, new List<PhotoInfo>(batch));
            }
        }

        /// <summary>
        /// スレッドでの読み込み処理が進捗したときのイベント処理
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void LoadPhotoFolderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is not List<PhotoInfo> batch)
            {
                return;
            }

            var worker = (BackgroundWorker)sender;
            foreach (var photo in batch)
            {
                if (worker.CancellationPending)
                {
                    return;
                }

                PhotoList.Add(photo);

                // サムネイル画像のためのキューに積む
                lock (thumbnailQueue)
                {
                    thumbnailQueue.Enqueue(photo);
                }
            }

            if (worker.CancellationPending)
            {
                return;
            }

            if (!firstImageLoaded)
            {
                firstImageLoaded = true;
                FirstImageLoaded?.Invoke(this, EventArgs.Empty);
            }

            // タイマー起動
            if (!thumbnailTimer.Enabled)
            {
                thumbnailTimer.Start();
            }
        }

        /// <summary>
        /// スレッドでの読み込み処理が終了したときのイベント処理
        /// </summary>
        /// <param name="sender">BackgroundWorker</param>
        /// <param name="e">引数情報</param>
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // 再読み込み時は、読み込み処理を再開
                if (isReloadContents)
                {
                    UpdatePhotoList();
                    isReloadContents = false;
                }
            }
            else
            {
                FolderLoadCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
