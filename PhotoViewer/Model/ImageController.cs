using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// 画像を管理するクラス
    /// </summary>
    public static class ImageController
    {
        /// <summary>
        /// 画像処理を行うネイティブメソッドを管理するクラス
        /// </summary>
        internal static class NativeMethods
        {
            /// <summary>
            /// Raw画像データを取得する
            /// </summary>
            /// <param name="path">画像ファイルパス</param>
            /// <param name="imageData">画像データ</param>
            /// <returns>0: 成功、1: 失敗</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetRawImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, out ImageData imageData);

            /// <summary>
            /// Raw画像のサムネイルデータを取得する
            /// </summary>
            /// <param name="path">画像ファイルパス</param>
            /// <param name="longSideLength">長辺の長さ(この長さにリサイズされる)</param>
            /// <param name="imageData">画像データ</param>
            /// <returns>0: 成功、1: 失敗</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetRawThumbnailImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, [In] int longSideLength, out ImageData imageData);

            /// <summary>
            /// 画像データを取得する(Raw画像以外のJpeg画像などで使用)
            /// </summary>
            /// <param name="path">画像ファイルパス</param>
            /// <param name="imageData">画像データ</param>
            /// <returns>0: 成功、1: 失敗</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetNormalImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, out ImageData imageData);

            /// <summary>
            /// 画像のサムネイルデータを取得する(Raw画像以外のJpeg画像などで使用)
            /// </summary>
            /// <param name="path">画像ファイルパス</param>
            /// <param name="longSideLength">長辺の長さ(この長さにリサイズされる)</param>
            /// <param name="imageData">画像データ</param>
            /// <returns>0: 成功、1: 失敗</returns>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetNormalThumbnailImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, [In] int longSideLength, out ImageData imageData);

            /// <summary>
            /// メモリを解放する
            /// </summary>
            /// <param name="buffer">解放するメモリバッファ</param>
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeBuffer(IntPtr buffer);

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
        }

        /// <summary>
        /// ピクチャビューに表示する画像を作成する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            const int LongSideLength = 2200;
            return DecodePicture(filePath, LongSideLength);
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

            const int LongSideLength = 240;
            return DecodePicture(filePath, LongSideLength);
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
        /// <returns>BitmapSource</returns>
        private static BitmapSource DecodePicture(string filePath, int longSideLength)
        {
            if (MediaChecker.CheckRawFileExtension(Path.GetExtension(filePath).ToLower()))
            {
                if (NativeMethods.GetRawThumbnailImageData(filePath, longSideLength, out var imageData) != 0)
                {
                    throw new FileFormatException("File format is wrong.");
                }

                return CreateBitmapSourceFromImageStruct(imageData);
            }
            else
            {
                if (NativeMethods.GetNormalThumbnailImageData(filePath, longSideLength, out var imageData) != 0)
                {
                    throw new FileFormatException("File format is wrong.");
                }

                return CreateBitmapSourceFromImageStruct(imageData);
            }
        }

        /// <summary>
        /// 画像データ情報からBitmapSourceを作成する
        /// </summary>
        /// <param name="imageData">画像データ情報</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource CreateBitmapSourceFromImageStruct(in NativeMethods.ImageData imageData)
        {
            try
            {
                var imgData = new byte[imageData.size];
                Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);

                var bitmap = new WriteableBitmap(imageData.width, imageData.height, 96, 96, PixelFormats.Bgr24, null);
                bitmap.WritePixels(new Int32Rect(0, 0, imageData.width, imageData.height), imgData, imageData.stride, 0, 0);
                bitmap.Freeze();

                return bitmap;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Cannot decode picture.", "Picture decode error");

                return null;
            }
            finally
            {
                NativeMethods.FreeBuffer(imageData.buffer);
            }
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