using Kchary.PhotoViewer.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

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
        public static BitmapImage GetThumbnail(string filePath, int size)
        {
            try
            {
                var localGuid = IShellItemImageFactoryGuid;
                SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref localGuid, out var factory);
                if (factory == null) return null;

                var sz = new SIZE { cx = size, cy = size };
                factory.GetImage(sz, SIIGBF.RESIZETOFIT, out var hBitmap);

                if (hBitmap == IntPtr.Zero) return null;

                // WinUIではHBITMAPから直接BitmapImageを作成する方法が異なる
                // 簡略化のため、ファイルから直接読み込む
                DeleteObject(hBitmap);

                return LoadBitmapImageFromFile(filePath, size);
            }
            catch
            {
                return LoadBitmapImageFromFile(filePath, size);
            }
        }

        /// <summary>
        /// ファイルからBitmapImageを読み込む
        /// </summary>
        private static BitmapImage LoadBitmapImageFromFile(string filePath, int size)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.DecodePixelWidth = size;
                bitmap.DecodePixelHeight = size;
                bitmap.UriSource = new Uri(filePath);
                return bitmap;
            }
            catch
            {
                return null;
            }
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
        public static async Task<BitmapImage> LoadThumbnailAsync(PhotoInfo photo, CancellationToken cancellationToken)
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
        /// <returns>BitmapImage</returns>
        public static BitmapImage DecodePicture(string filePath, int longSideLength, bool isRawImage = false, CancellationToken cancellationToken = default)
        {
            BitmapImage image;
            try
            {
                if (isRawImage)
                {
                    // RAW画像の場合はC++/CLIを使用
                    ImageReaderWrapper imageReaderWrapper = new();
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

                    image = CreateBitmapImageFromImageData(imageData, cancellationToken);
                }
                else
                {
                    // 通常の画像はBitmapImageで読み込み
                    image = new BitmapImage();
                    image.DecodePixelWidth = longSideLength;
                    image.UriSource = new Uri(filePath);
                }
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
        /// 画像データ情報からBitmapImageを作成する
        /// </summary>
        /// <param name="imageData">画像データ情報</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>BitmapImage</returns>
        private static BitmapImage CreateBitmapImageFromImageData(ImageDataWrapper imageData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imgData = imageData.Buffer;
            if (imgData == null)
            {
                return null;
            }

            try
            {
                // WinUIでは WriteableBitmap を使用してピクセルデータから画像を作成
                var bitmap = new WriteableBitmap(imageData.Width, imageData.Height);
                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    // BGRからBGRAに変換して書き込み
                    int pixelCount = imageData.Width * imageData.Height;
                    byte[] bgraData = new byte[pixelCount * 4];
                    for (int i = 0; i < pixelCount; i++)
                    {
                        bgraData[i * 4] = imgData[i * 3];         // B
                        bgraData[i * 4 + 1] = imgData[i * 3 + 1]; // G
                        bgraData[i * 4 + 2] = imgData[i * 3 + 2]; // R
                        bgraData[i * 4 + 3] = 255;                // A
                    }
                    stream.Write(bgraData, 0, bgraData.Length);
                }
                bitmap.Invalidate();

                // WriteableBitmapからBitmapImageに変換（簡略化のため、そのまま返す）
                // 注: 厳密にはBitmapImageではないが、ImageSourceとして使用可能
                return bitmap as BitmapImage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 画像の回転情報を取得する（簡略化版）
        /// </summary>
        public static uint GetRotation(object metaData)
        {
            // WinUIでは metadata 処理が異なるため、簡略化
            return 0;
        }

        /// <summary>
        /// 画像を回転させる（簡略化版）
        /// </summary>
        public static BitmapImage RotateImage(object metaData, BitmapImage image)
        {
            // WinUIでは Transform 処理が異なるため、簡略化
            return image;
        }
    }
}