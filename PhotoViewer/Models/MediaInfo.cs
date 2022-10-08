using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// サポート対象のファイル拡張子のタイプ定義
    /// </summary>
    public enum FileExtensionType
    {
        Jpeg,
        Bmp,
        Png,
        Tiff,
        Gif,
        Dng,
        Nef,
        Unknown
    }

    /// <summary>
    /// メディア情報クラス
    /// </summary>
    public sealed record MediaInfo
    {
        #region Media Parameters

        /// <summary>
        /// サムネイル画像
        /// </summary>
        public BitmapSource ThumbnailImage { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        /// <summary>
        /// サポート画像フラグ
        /// </summary>
        public bool IsSupportImage
        {
            get { return AppConfigManager.SupportPictureExtensions.Any(supportExtension => supportExtension == FileExtension); }
        }

        /// <summary>
        /// RAW画像フラグ
        /// </summary>
        public bool IsRawImage
        {
            get { return AppConfigManager.SupportRawPictureExtensions.Any(x => x == FileExtension); }
        }

        /// <summary>
        /// ファイルの拡張子
        /// </summary>
        public string FileExtension
        {
            get { return Path.GetExtension(FilePath)?.ToLower(); }
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

                return !SupportExtensionMap.TryGetValue(FileExtension, out var extensionType) ? FileExtensionType.Unknown : extensionType;
            }
        }

        private static readonly IReadOnlyDictionary<string, FileExtensionType> SupportExtensionMap = new Dictionary<string, FileExtensionType>()
        {
            { ".jpg", FileExtensionType.Jpeg },
            { ".bmp", FileExtensionType.Bmp },
            { ".png", FileExtensionType.Png },
            { ".tiff", FileExtensionType.Tiff },
            { ".tif", FileExtensionType.Tiff },
            { ".gif", FileExtensionType.Gif },
            { ".dng", FileExtensionType.Dng },
            { ".nef", FileExtensionType.Nef }
        };

        /// <summary>
        /// サムネイル画像を作成する
        /// </summary>
        /// <returns>True: 成功、False: 失敗</returns>
        public bool CreateThumbnailImage()
        {
            try
            {
                if (!FileUtil.CheckFilePath(FilePath))
                {
                    return false;
                }

                ThumbnailImage = ImageUtil.CreatePictureThumbnailImage(this);
                return true;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// ピクチャビューに表示する画像を作成する
        /// </summary>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        public BitmapSource CreatePictureViewImage(bool stopLoading)
        {
            const int LongSideLength = 2200;
            return ImageUtil.DecodePicture(FilePath, LongSideLength, IsRawImage, stopLoading);
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
    }
}