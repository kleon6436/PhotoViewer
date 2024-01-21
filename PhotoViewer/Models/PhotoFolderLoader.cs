using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
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

            // 選択されたフォルダ内でサポート対象の拡張子を順番にチェック
            var firstImageLoadEventCalled = false;
            var queue = new List<PhotoInfo>();
            foreach (var supportExtension in Const.SupportPictureExtensions)
            {
                var tick = Environment.TickCount;
                var count = 0;

                // サポート対象のファイルを順番に読み込む
                foreach (var supportFile in folder.EnumerateFiles($"*{supportExtension}").OrderBy(files => files, new NaturalFileInfoNameComparer()))
                {
                    Thread.Sleep(50);   // CPU負荷低減のため

                    if (sender is BackgroundWorker { CancellationPending: true })
                    {
                        e.Cancel = true;
                        return;
                    }

                    try
                    {
                        queue.Add(new PhotoInfo(supportFile.FullName));
                        count++;
                    }
                    catch
                    {
                        // 読み込み失敗時は、その画像の読み込みはスキップする
                        continue;
                    }

                    var duration = Environment.TickCount - tick;
                    if ((count > 100 || duration <= 250) && duration <= 500)
                    {
                        continue;
                    }
                    foreach (var item in queue)
                    {
                        if (sender is BackgroundWorker { CancellationPending: true })
                        {
                            e.Cancel = true;
                            return;
                        }
                        PhotoList.Add(item);
                    }

                    if (PhotoList.Count >= 5 && !firstImageLoadEventCalled)
                    {
                        FirstImageLoaded?.Invoke(this, EventArgs.Empty);
                        firstImageLoadEventCalled = true;
                    }

                    queue.Clear();
                    tick = Environment.TickCount;
                }

                if (queue.Count != 0)
                {
                    foreach (var item in queue)
                    {
                        if (sender is BackgroundWorker { CancellationPending: true })
                        {
                            e.Cancel = true;
                            return;
                        }
                        PhotoList.Add(item);
                    }
                    queue.Clear();
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
