using Kchary.PhotoViewer.Models;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// 画像を管理するユーティリティクラス
    /// </summary>
    public static class ImageUtil
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            [In] string pszPath,
            [In] IntPtr pbc,
            [In] ref Guid riid,
            [Out] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        private static readonly Guid IShellItemImageFactoryGuid = new("bcc18b79-ba16-442f-80c4-8a59c30c463b");

        /// <summary>
        /// OS標準のキャッシュされたサムネイル（ThumbCache）を使用してサムネイル画像を作成します
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="size">画像サイズ</param>
        /// <returns>サムネイル画像</returns>
        public static BitmapSource GetThumbnail(string filePath, int size)
        {
            var localGuid = IShellItemImageFactoryGuid;
            SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref localGuid, out var factory);
            if (factory == null) return null;

            var sz = new SIZE { cx = size, cy = size };
            factory.GetImage(sz, SIIGBF.RESIZETOFIT, out var hBitmap);

            if (hBitmap == IntPtr.Zero) return null;

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            source.Freeze();
            return source;
        }

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItemImageFactory
        {
            void GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SIZE
        {
            public int cx;
            public int cy;
        }

        [Flags]
        internal enum SIIGBF
        {
            RESIZETOFIT = 0x00,
            BIGGERSIZEOK = 0x01,
            MEMORYONLY = 0x02,
            ICONONLY = 0x04,
            THUMBNAILONLY = 0x08,
            INCACHEONLY = 0x10,
        }

        /// <summary>
        /// サムネイル画像をロードする
        /// </summary>
        /// <returns>True: 成功、False: 失敗</returns>
        public static async Task<BitmapSource> LoadThumbnailAsync(PhotoInfo photo, CancellationToken cancellationToken)
        {
            try
            {
                return await Task.Run(() => ThumbnailCache.GetOrCreate(photo.FilePath, ThumbnailQuality.Small), cancellationToken);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 画像をデコードする
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="longSideLength">長辺の長さ(この長さにあわせて画像がリサイズされる)</param>
        /// <param name="isRawImage">RAW画像フラグ</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource DecodePicture(string filePath, int longSideLength, bool isRawImage = false, CancellationToken cancellationToken = default)
        {
            ImageReaderWrapper imageReaderWrapper = new();
            BitmapSource image;
            try
            {
                // 画像を読み込む
                const bool isThumbnailMode = true;
                ImageReaderSettingsWrapper imageReadSettings = new()
                {
                    IsRawImage = isRawImage,
                    IsThumbnailMode = isThumbnailMode,
                    ResizeLongSideLength = longSideLength,
                };

                ImageDataWrapper imageData = new();
                if (!imageReaderWrapper.GetImageData(filePath, imageReadSettings, imageData))
                {
                    throw new Exception("Failed to get image");
                }

                image = CreateBitmapSourceFromImageStruct(imageData, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 正常キャンセル → nullを返す
                image = null;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Cannot decode picture.", "Picture decode error");
                image = null;
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
        /// 画像データ情報からWriteableBitmapを作成する
        /// </summary>
        /// <param name="imageData">画像データ情報</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>WriteableBitmap</returns>
        private static WriteableBitmap CreateBitmapSourceFromImageStruct(ImageDataWrapper imageData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imgData = imageData.Buffer;
            if (imgData == null)
            {
                return null;
            }

            var bitmap = new WriteableBitmap(imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr24, null);
            bitmap.WritePixels(new Int32Rect(0, 0, imageData.Width, imageData.Height), imgData, imageData.Stride, 0, 0);
            bitmap.Freeze();

            return bitmap;
        }

        /// <summary>
        /// 画像を回転させる
        /// </summary>
        private static TransformedBitmap TransformBitmap(BitmapSource source, Transform transform)
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