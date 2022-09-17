using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Helper
{
    /// <summary>
    /// 画像を管理するクラス
    /// </summary>
    public static class ImageController
    {
        private static Mutex mutex = new();

        /// <summary>
        /// 画像処理を行うネイティブメソッドを管理するクラス
        /// </summary>
        internal static class NativeMethods
        {
            /// <summary>
            /// 画像を読み込み、画像サイズを取得する
            /// </summary>
            /// <param name="imagePath">画像ファイルパス</param>
            /// <param name="imageReadSettingPtr">画像読み込み設定</param>
            /// <param name="imageSize">画像サイズ</param>
            /// <returns>成功: True, 失敗: False</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern bool LoadImageAndGetImageSize([MarshalAs(UnmanagedType.LPWStr), In] string imagePath, [In] IntPtr imageReadSettingPtr, out int imageSize);

            /// <summary>
            /// 画像データを取得する
            /// </summary>
            /// <param name="imageData">画像データ</param>
            /// <returns>成功: True, 失敗: False</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern bool GetImageData(ref ImageData imageData);

            /// <summary>
            /// サムネイルデータを取得する
            /// </summary>
            /// <param name="imageData">画像データ</param>
            /// <returns>成功: True, 失敗: False</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern bool GetThumbnailImageData(ref ImageData imageData);

            /// <summary>
            /// 画像データ
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct ImageData
            {
                public IntPtr buffer;
                public uint size;
                public int stride;
                public int width;
                public int height;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct ImageReadSettings
            {
                public int isRawImage;
                public int isThumbnailMode;
                public int resizeLongSideLength;
            }
        }

        /// <summary>
        /// ピクチャビューに表示する画像を作成する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureViewImage(string filePath, bool stopLoading)
        {
            const int LongSideLength = 2200;
            return DecodePicture(filePath, LongSideLength, stopLoading);
        }

        /// <summary>
        /// 画像一覧に表示するサムネイル画像を作成する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            const int LongSideLength = 100;

            if (MediaChecker.CheckRawFileExtension(Path.GetExtension(filePath).ToLower()))
            {
                return DecodePicture(filePath, LongSideLength);
            }
            else
            {
                using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

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
                    return DecodePicture(filePath, LongSideLength);
                }
            }
        }

        /// <summary>
        /// 編集画面に表示するサムネイル画像を作成する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="defaultPictureWidth">DefaultPictureWidth</param>
        /// <param name="defaultPictureHeight">DefaultPictureHeight</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureEditViewThumbnail(string filePath, out int defaultPictureWidth, out int defaultPictureHeight, out uint rotation)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            defaultPictureWidth = bitmapFrame.PixelWidth;
            defaultPictureHeight = bitmapFrame.PixelHeight;
            rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

            var longSideLength = rotation is 5 or 6 or 7 or 8 ? 240 : 350;
            return DecodePicture(filePath, longSideLength);
        }

        /// <summary>
        /// 保存対象の画像を作成する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="scale">scale</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreateSavePicture(string filePath, double scale)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            // 回転情報をメタデータから取得
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // 画像を指定サイズにリサイズ
            BitmapSource saveImage = bitmapFrame;
            saveImage = new TransformedBitmap(saveImage, new ScaleTransform(scale, scale));

            // リサイズ後に回転
            saveImage = RotateImage(metaData, saveImage);

            // リサイズ、回転した画像を書き出す
            var decodeImage = new WriteableBitmap(saveImage);
            decodeImage.Freeze();

            return decodeImage;
        }

        /// <summary>
        /// 画像をデコードする
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="longSideLength">長辺の長さ(この長さにあわせて画像がリサイズされる)</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource DecodePicture(string filePath, int longSideLength, bool stopLoading = false)
        {
            try
            {
                if (!mutex.WaitOne(1000, false))
                {
                    return null;
                }
                NativeMethods.ImageData imageData = new();
                var imageReadSettingPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(NativeMethods.ImageReadSettings)));
                BitmapSource image = null;

                try
                {
                    // 画像を読み込む
                    var isRawImage = MediaChecker.CheckRawFileExtension(Path.GetExtension(filePath).ToLower());
                    var isThumbnailMode = true;
                    NativeMethods.ImageReadSettings imageReadSettings = new()
                    {
                        isRawImage = Convert.ToInt32(isRawImage),
                        isThumbnailMode = Convert.ToInt32(isThumbnailMode),
                        resizeLongSideLength = longSideLength,
                    };
                    Marshal.StructureToPtr(imageReadSettings, imageReadSettingPtr, false);
                    NativeMethods.LoadImageAndGetImageSize(filePath, imageReadSettingPtr, out int imageSize);

                    // 必要なバッファを準備
                    imageData.buffer = Marshal.AllocCoTaskMem(imageSize);

                    // 画像データの取得
                    if (isThumbnailMode)
                    {
                        NativeMethods.GetThumbnailImageData(ref imageData);
                    }
                    else
                    {
                        NativeMethods.GetImageData(ref imageData);
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
                }

                return image;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 画像データ情報からBitmapSourceを作成する
        /// </summary>
        /// <param name="imageData">画像データ情報</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource CreateBitmapSourceFromImageStruct(NativeMethods.ImageData imageData, bool stopLoading = false)
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
        /// <param name="metaData">Metadata</param>
        /// <param name="image">BitmapSource</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource RotateImage(BitmapMetadata metaData, BitmapSource image)
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
        private static uint GetRotation(BitmapMetadata metaData)
        {
            const string Query = "/app1/ifd/exif:{uint=274}";
            return metaData.ContainsQuery(Query) ? Convert.ToUInt32(metaData.GetQuery(Query)) : 0;
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