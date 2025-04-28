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
using System.Windows.Data;
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
        private bool firstImageLoaded = false;

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
        private readonly DispatcherTimer thumbnailTimer;

        /// <summary>
        /// 1フレームあたりのサムネイル生成数
        /// </summary>
        private const int MaxThumbnailsPerTick = 4;

        /// <summary>
        /// IO制限4並列
        /// </summary>
        private readonly SemaphoreSlim ioSemaphore = new(4);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhotoFolderLoader()
        {
            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(PhotoList, new object());

            thumbnailTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33) // だいたい1フレーム(30fps想定)
            };
            thumbnailTimer.Tick += ThumbnailTimer_TickAsync;

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
        private async void ThumbnailTimer_TickAsync(object sender, object e)
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

            foreach (var photo in itemsToLoad)
            {
                try
                {
                    await ioSemaphore.WaitAsync(thumbnailLoadCts.Token);
                    await photo.LoadThumbnailAsync(thumbnailLoadCts.Token);
                }
                catch (Exception ex)
                {
                    App.LogException(ex);
                }
                finally
                {
                    ioSemaphore.Release();
                }
            }

            if (thumbnailQueue.Count == 0)
            {
                thumbnailTimer.Stop(); // もうやるものがなければ止める
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
            firstImageLoaded = false;

            if (!RequestStopThreadAndTask())
            {
                isReloadContents = true;
                return;
            }

            PhotoList.Clear();
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

            var folder = new DirectoryInfo(folderPath);

            var fileList = Const.SupportPictureExtensions
                .SelectMany(ext => folder.EnumerateFiles($"*{ext}", SearchOption.TopDirectoryOnly))
                .OrderBy(f => f, new NaturalFileInfoNameComparer())
                .ToList();

            const int batchSize = 5;
            var batch = new List<PhotoInfo>(batchSize);

            foreach (var file in fileList)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                PhotoInfo photo;
                try
                {
                    photo = new PhotoInfo(file.FullName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ファイル読み込み失敗: {file.FullName} ({ex.Message})");
                    continue;
                }

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                batch.Add(photo);

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
            var worker = (BackgroundWorker)sender;

            if (e.UserState is List<PhotoInfo> batch)
            {
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

                if (!firstImageLoaded)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }

                    firstImageLoaded = true;
                    FirstImageLoaded?.Invoke(this, EventArgs.Empty);
                }

                // タイマー起動
                if (!thumbnailTimer.IsEnabled)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }

                    thumbnailTimer.Start();
                }
            }
        }

        /// <summary>
        /// サムネイル画像の読み込み処理をキャンセル
        /// </summary>
        private void CancelThumbnailLoad()
        {
            // タイマーを停止し、キューをクリーンアップ
            thumbnailTimer.Stop();
            lock (thumbnailQueue)
            {
                thumbnailQueue.Clear();
            }

            // キャンセルトークンを発行
            thumbnailLoadCts.Cancel();
            thumbnailLoadCts.Dispose();
            thumbnailLoadCts = new CancellationTokenSource();
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
