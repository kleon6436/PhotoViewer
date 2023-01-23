using Kchary.PhotoViewer.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    public sealed class SinglePhotoLoader
    {
        /// <summary>
        /// Exif情報を読み込み用クラスインスタンス
        /// </summary>
        private readonly ExifLoader exifLoader;

        /// <summary>
        /// 写真ロード中フラグ
        /// </summary>
        private bool loadingPhoto;

        /// <summary>
        /// 停止要求フラグ
        /// </summary>
        private volatile bool stopRequest;

        /// <summary>
        /// 写真ロード時のタスクリスト
        /// </summary>
        private Task[] loadPhotoTasks;

        /// <summary>
        /// 写真情報
        /// </summary>
        public PhotoInfo PhotoInfo { private get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exifLoader">Exif情報をロードするためのクラスインスタンス</param>
        public SinglePhotoLoader(ExifLoader exifLoader)
        {
            this.exifLoader = exifLoader;
        }

        /// <summary>
        /// 写真とExif情報を読み込む
        /// </summary>
        /// <returns>写真とExif情報</returns>
        public async Task<Tuple<BitmapSource, ExifInfo[]>> LoadPhoto()
        {
            if (PhotoInfo == null)
            {
                throw new ArgumentNullException(nameof(PhotoInfo));
            }

            if (loadingPhoto)
            {
                stopRequest = true;
                if (loadPhotoTasks is not null)
                {
                    foreach (var task in loadPhotoTasks)
                    {
                        // 終了待機
                        task.Wait();
                    }
                }
                stopRequest = false;
            }

            if (!FileUtil.CheckFilePath(PhotoInfo.FilePath))
            {
                throw new FileNotFoundException();
            }

            loadingPhoto = true;
            var result= await LoadImageAndExif();
            loadingPhoto = false;

            if (result.Item1 == null || result.Item2.Length == 0)
            {
                throw new FieldAccessException();
            }

            return result;
        }

        /// <summary>
        /// 選択されたメディア情報を非同期で読み込み、画像、Exif情報を取得する
        /// </summary>
        /// <returns>画像とExif情報</returns>
        private async Task<Tuple<BitmapSource, ExifInfo[]>> LoadImageAndExif()
        {
            BitmapSource image = null;
            ExifInfo[] exifInfos = Array.Empty<ExifInfo>();

            // 画像とExifを読み込むタスクを作成する
            var loadPictureTask = Task.Run(() =>
            {
                if (stopRequest)
                {
                    return;
                }
                image = PhotoInfo.CreatePictureViewImage(stopRequest);
            });
            var setExifInfoTask = Task.Run(() =>
            {
                if (stopRequest)
                {
                    return;
                }
                exifLoader.PhotoInfo = PhotoInfo;
                exifInfos = exifLoader.CreateExifInfoList(stopRequest);
            });

            // タスクを実行し、処理完了まで待つ
            loadPhotoTasks = new[]
            {
                    loadPictureTask,
                    setExifInfoTask
            };
            await Task.WhenAll(loadPhotoTasks);

            return new Tuple<BitmapSource, ExifInfo[]>(image, exifInfos);
        }
    }
}
