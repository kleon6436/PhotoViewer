using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 写真情報クラス
    /// </summary>
    public sealed partial class PhotoInfo : ObservableObject
    {
        #region Media Parameters

        /// <summary>
        /// サムネイル画像
        /// </summary>
        [ObservableProperty]
        private BitmapSource thumbnailImage;

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        private readonly SemaphoreSlim thumbnailLoadSemaphore = new(4); // 同時4個までに制限

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public PhotoInfo(string filePath)
        {
            if (!FileUtil.CheckFilePath(filePath))
            {
                throw new FileNotFoundException();
            }

            FilePath = filePath;
            FileName = FileUtil.GetFileName(filePath, false);
        }

        /// <summary>
        /// サポート画像フラグ
        /// </summary>
        public bool IsSupportImage
        {
            get { return Const.SupportPictureExtensions.Any(supportExtension => supportExtension == FileExtension); }
        }

        /// <summary>
        /// RAW画像フラグ
        /// </summary>
        public bool IsRawImage
        {
            get { return Const.SupportRawPictureExtensions.Any(x => x == FileExtension); }
        }

        /// <summary>
        /// ファイルの拡張子
        /// </summary>
        public string FileExtension
        {
            get { return FileUtil.GetFileExtensions(FilePath); }
        }

        /// <summary>
        /// ファイル拡張子タイプ
        /// </summary>
        public FileExtensionType FileExtensionType
        {
            get
            {
                if (!IsSupportImage)
                {
                    return FileExtensionType.Unknown;
                }

                return !Const.SupportExtensionMap.TryGetValue(FileExtension, out var extensionType) ? FileExtensionType.Unknown : extensionType;
            }
        }

        /// <summary>
        /// ピクチャビューに表示する画像を作成する
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>BitmapSource</returns>
        public BitmapSource CreatePictureViewImage(CancellationToken cancellationToken)
        {
            const int LongSideLength = 2200;
            return ImageUtil.DecodePicture(FilePath, LongSideLength, IsRawImage, cancellationToken);
        }

        /// <summary>
        /// 編集画面に表示するサムネイル画像を作成する
        /// </summary>
        /// <param name="defaultPictureWidth">DefaultPictureWidth</param>
        /// <param name="defaultPictureHeight">DefaultPictureHeight</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>BitmapSource</returns>
        public BitmapSource CreateEditViewImage(out int defaultPictureWidth, out int defaultPictureHeight, out uint rotation)
        {
            using FileStream sourceStream = new(FilePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            defaultPictureWidth = bitmapFrame.PixelWidth;
            defaultPictureHeight = bitmapFrame.PixelHeight;
            rotation = ImageUtil.GetRotation(bitmapFrame.Metadata as BitmapMetadata);

            var longSideLength = rotation is 5 or 6 or 7 or 8 ? 240 : 350;
            return ImageUtil.DecodePicture(FilePath, longSideLength, IsRawImage);
        }

        /// <summary>
        /// 保存用の画像を作成する
        /// </summary>
        /// <param name="scale">scale</param>
        /// <returns>BitmapSource</returns>
        public BitmapSource CreateSaveImage(double scale)
        {
            using var sourceStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            // 回転情報をメタデータから取得
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // 画像を指定サイズにリサイズ
            BitmapSource saveImage = bitmapFrame;
            saveImage = new TransformedBitmap(saveImage, new ScaleTransform(scale, scale));

            // リサイズ後に回転
            saveImage = ImageUtil.RotateImage(metaData, saveImage);

            // リサイズ、回転した画像を書き出す
            var decodeImage = new WriteableBitmap(saveImage);
            decodeImage.Freeze();

            return decodeImage;
        }

        /// <summary>
        /// サムネイル画像を作成する
        /// </summary>
        /// <returns>True: 成功、False: 失敗</returns>
        public async Task LoadThumbnailAsync(CancellationToken cancellationToken)
        {
            try
            {
                await thumbnailLoadSemaphore.WaitAsync(cancellationToken);

                var thumbnail = await Task.Run(() => ThumbnailCache.GetOrCreate(FilePath, ThumbnailQuality.Small), cancellationToken);
                if (thumbnail != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ThumbnailImage = thumbnail;
                    }, System.Windows.Threading.DispatcherPriority.Normal, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
            finally
            {
                thumbnailLoadSemaphore.Release();
            }
        }
    }
}