using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;

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
        /// プリフェッチ
        /// </summary>
        private bool prefetchStarted = false;

        /// <summary>
        /// 画像読み込みフラグ
        /// </summary>
        private bool firstImageLoaded = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhotoFolderLoader()
        {
            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(PhotoList, new object());

            // バックグラウンドスレッドの設定
            loadPhotoFolderWorker.DoWork += LoadPhotoFolderDoWork;
            loadPhotoFolderWorker.ProgressChanged += LoadPhotoFolderProgressChanged;
            loadPhotoFolderWorker.RunWorkerCompleted += RunWorkerCompleted;
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
            if (loadPhotoFolderWorker is { IsBusy: true })
            {
                loadPhotoFolderWorker.CancelAsync();
                isReloadContents = true;
                return;
            }

            prefetchStarted = false;
            firstImageLoaded = false;

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

            const int batchSize = 20;
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

                batch.Add(photo);

                if (batch.Count >= batchSize)
                {
                    worker.ReportProgress(0, new List<PhotoInfo>(batch)); // Deep copyして渡す
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
            if (e.UserState is List<PhotoInfo> batch)
            {
                foreach (var photo in batch)
                {
                    PhotoList.Add(photo);

                    if (!firstImageLoaded)
                    {
                        firstImageLoaded = true;
                        FirstImageLoaded?.Invoke(this, EventArgs.Empty);
                    }
                }

                if (!prefetchStarted && PhotoList.Count >= 20)
                {
                    StartPrefetch();
                    prefetchStarted = true;
                }
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

        /// <summary>
        /// サムネイルのプリフェッチを開始する
        /// </summary>
        private void StartPrefetch()
        {
            var prefetchTargets = PhotoList
                .Skip(20)
                .Take(40)
                .Select(p => p.FilePath)
                .ToList();

            ThumbnailCache.PrefetchThumbnails(prefetchTargets, ThumbnailQuality.Small);
        }
    }
}
