using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly BackgroundWorker loadPhotoFolderWorker = new() { WorkerSupportsCancellation = true };

        /// <summary>
        /// コンテンツをリロードするためのフラグ
        /// </summary>
        private bool isReloadContents;

        /// <summary>
        /// 写真一覧リスト
        /// </summary>
        public ObservableCollection<PhotoInfo> PhotoList { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhotoFolderLoader()
        {
            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(PhotoList, new object());

            // バックグラウンドスレッドの設定
            loadPhotoFolderWorker.DoWork += LoadPhotoFolderDoWork;
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
            if (!FileUtil.IsDirectory(folderPath))
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }

            if (!FileUtil.CheckFolderPath(folderPath))
            {
                return;
            }

            var folder = new DirectoryInfo(folderPath);
            var files = Const.SupportPictureExtensions
                .AsParallel()
                .WithDegreeOfParallelism(Math.Max(2, Environment.ProcessorCount / 2))
                .SelectMany(ext => folder.EnumerateFiles($"*{ext}", SearchOption.TopDirectoryOnly))
                .ToList();

            if (files.Count == 0)
            {
                return;
            }

            var photoInfos = files
                .AsParallel()
                .WithDegreeOfParallelism(Math.Max(2, Environment.ProcessorCount / 2))
                .Select(file =>
                {
                    if (sender is BackgroundWorker worker && worker.CancellationPending)
                    {
                        throw new OperationCanceledException();
                    }

                    try
                    {
                        return new PhotoInfo(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ファイル読み込み失敗: {file.FullName} ({ex.Message})");
                        return null;
                    }
                })
                .Where(photo => photo != null)
                .ToList();

            photoInfos.Sort(new NaturalFileInfoNameComparer());

            bool firstImageLoaded = false;
            foreach (var photo in photoInfos)
            {
                PhotoList.Add(photo);

                if (!firstImageLoaded && PhotoList.Count >= 5)
                {
                    FirstImageLoaded?.Invoke(this, EventArgs.Empty);
                    firstImageLoaded = true;
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
    }
}
