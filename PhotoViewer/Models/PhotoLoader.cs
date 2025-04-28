using Kchary.PhotoViewer.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 1枚の写真情報から写真・Exif情報を読み込むクラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="exifLoader">Exif情報をロードするためのクラスインスタンス</param>
    public sealed class PhotoLoader(ExifLoader exifLoader)
    {
        /// <summary>
        /// Exif情報を読み込み用クラスインスタンス
        /// </summary>
        private readonly ExifLoader exifLoader = exifLoader;

        /// <summary>
        /// 写真情報
        /// </summary>
        public PhotoInfo PhotoInfo { private get; set; }

        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationTokenSource cancellationTokenSource = new();

        /// <summary>
        /// セマフォ
        /// </summary>
        private readonly SemaphoreSlim loadPhotoSemaphore = new(1, 1);

        /// <summary>
        /// 写真とExif情報を読み込む
        /// </summary>
        /// <returns>写真とExif情報</returns>
        public async Task<(BitmapSource Image, ExifInfo[] ExifInfos)> LoadPhotoAsync()
        {
            if (PhotoInfo == null)
            {
                throw new ArgumentNullException(nameof(PhotoInfo));
            }

            await loadPhotoSemaphore.WaitAsync();
            try
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = new CancellationTokenSource();

                if (!FileUtil.CheckFilePath(PhotoInfo.FilePath))
                {
                    throw new FileNotFoundException($"File not found: {PhotoInfo.FilePath}");
                }

                var (image, exifInfos) = await LoadImageAndExifAsync(cancellationTokenSource.Token);

                if (image == null || exifInfos == null || exifInfos.Length == 0)
                {
                    throw new FieldAccessException("Failed to load image or EXIF information.");
                }

                return (image, exifInfos);
            }
            finally
            {
                loadPhotoSemaphore.Release();
            }
        }

        /// <summary>
        /// 選択されたメディア情報を非同期で読み込み、画像、Exif情報を取得する
        /// </summary>
        /// <returns>画像とExif情報</returns>
        private async Task<(BitmapSource Image, ExifInfo[] ExifInfos)> LoadImageAndExifAsync(CancellationToken cancellationToken)
        {
            var loadImageTask = Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return PhotoInfo.CreatePictureViewImage(cancellationToken);
            }, cancellationToken);

            var loadExifTask = Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                exifLoader.PhotoInfo = PhotoInfo;
                return exifLoader.CreateExifInfoList(cancellationToken);
            }, cancellationToken);

            await Task.WhenAll(loadImageTask, loadExifTask);
            return (await loadImageTask, await loadExifTask);
        }
    }
}
