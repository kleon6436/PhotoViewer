using Kchary.PhotoViewer.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// 画像を管理するユーティリティクラス
    /// </summary>
    public static class ImageUtil
    {
        /// <summary>
        /// 画像一覧に表示するサムネイル画像を作成する
        /// </summary>
        /// <param name="mediaInfo">メディア情報</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureThumbnailImage(MediaInfo mediaInfo)
        {
            const int LongSideLength = 100;

            if (mediaInfo.IsRawImage)
            {
                return DecodePicture(mediaInfo.FilePath, LongSideLength, mediaInfo.IsRawImage);
            }
            else
            {
                using var sourceStream = new FileStream(mediaInfo.FilePath, FileMode.Open, FileAccess.Read);

                sourceStream.Seek(0, SeekOrigin.Begin);
                var bitmapFrame = BitmapFrame.Create(sourceStream);
                var metadata = bitmapFrame.Metadata as BitmapMetadata;
                var thumbnail = bitmapFrame.Thumbnail;

                if (thumbnail != null)
                {
                    // リサイズ処理
                    var scale = LongSideLength / (double)thumbnail.PixelWidth;
                    if (thumbnail.PixelWidth < thumbnail.PixelHeight)
                    {
                        scale = LongSideLength / (double)thumbnail.PixelHeight;
                    }
                    thumbnail = new TransformedBitmap(thumbnail, new ScaleTransform(scale, scale));

                    // リサイズ後に回転
                    thumbnail = RotateImage(metadata, thumbnail);

                    // リサイズ、回転した画像を書き出す
                    var decodeImage = new WriteableBitmap(thumbnail);
                    decodeImage.Freeze();

                    return decodeImage;
                }
                else
                {
                    return DecodePicture(mediaInfo.FilePath, LongSideLength, mediaInfo.IsRawImage);
                }
            }
        }

        /// <summary>
        /// 画像をデコードする
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="longSideLength">長辺の長さ(この長さにあわせて画像がリサイズされる)</param>
        /// <param name="isRawImage">RAW画像フラグ</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource DecodePicture(string filePath, int longSideLength, bool isRawImage = false, bool stopLoading = false)
        {
            ImageData imageData = new();
            ImageReadLibraryWrapper imageReadLibraryWrapper = new();
            BitmapSource image = null;
            var imageReadSettingPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ImageReadSettings)));

            try
            {
                // 画像を読み込む
                var isThumbnailMode = true;
                ImageReadSettings imageReadSettings = new()
                {
                    isRawImage = Convert.ToInt32(isRawImage),
                    isThumbnailMode = Convert.ToInt32(isThumbnailMode),
                    resizeLongSideLength = longSideLength,
                };
                Marshal.StructureToPtr(imageReadSettings, imageReadSettingPtr, false);
                imageReadLibraryWrapper.LoadImageAndGetImageSize(filePath, imageReadSettingPtr, out int imageSize);

                // 必要なバッファを準備
                imageData.buffer = Marshal.AllocCoTaskMem(imageSize);

                // 画像データの取得
                if (isThumbnailMode)
                {
                    imageReadLibraryWrapper.GetThumbnailImageData(ref imageData);
                }
                else
                {
                    imageReadLibraryWrapper.GetImageData(ref imageData);
                }

                image = CreateBitmapSourceFromImageStruct(imageData, stopLoading);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Cannot decode picture.", "Picture decode error");
                image = null;
            }
            finally
            {
                Marshal.FreeCoTaskMem(imageData.buffer);
                Marshal.FreeCoTaskMem(imageReadSettingPtr);
                imageReadLibraryWrapper.Dispose();
            }

            return image;
        }

        /// <summary>
        /// 画像を回転させる
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <param name="image">BitmapSource</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource RotateImage(BitmapMetadata metaData, BitmapSource image)
        {
            var rotation = GetRotation(metaData);

            return rotation switch
            {
                1 => image,
                3 => TransformBitmap(image, new RotateTransform(180)),
                6 => TransformBitmap(image, new RotateTransform(90)),
                8 => TransformBitmap(image, new RotateTransform(270)),
                2 => TransformBitmap(image, new ScaleTransform(-1, 1, 0, 0)),
                4 => TransformBitmap(image, new ScaleTransform(1, -1, 0, 0)),
                5 => TransformBitmap(TransformBitmap(image, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0)),
                7 => TransformBitmap(TransformBitmap(image, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0)),
                _ => image,
            };
        }

        /// <summary>
        /// 画像の回転情報を取得する
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <returns>画像の回転情報</returns>
        public static uint GetRotation(BitmapMetadata metaData)
        {
            const string Query = "/app1/ifd/exif:{uint=274}";
            return metaData.ContainsQuery(Query) ? Convert.ToUInt32(metaData.GetQuery(Query)) : 0;
        }

        /// <summary>
        /// 画像データ情報からBitmapSourceを作成する
        /// </summary>
        /// <param name="imageData">画像データ情報</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource CreateBitmapSourceFromImageStruct(ImageData imageData, bool stopLoading = false)
        {
            try
            {
                var imgData = new byte[imageData.size];
                Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);

                if (stopLoading)
                {
                    return null;
                }

                var bitmap = new WriteableBitmap(imageData.width, imageData.height, 96, 96, PixelFormats.Bgr24, null);
                bitmap.WritePixels(new Int32Rect(0, 0, imageData.width, imageData.height), imgData, imageData.stride, 0, 0);
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 画像を回転させる
        /// </summary>
        private static BitmapSource TransformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            return result;
        }
    }
}