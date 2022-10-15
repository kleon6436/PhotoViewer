using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// 画像データ
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageData
    {
        public IntPtr buffer;
        public uint size;
        public int stride;
        public int width;
        public int height;
    }

    /// <summary>
    /// 画像読み込み設定
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageReadSettings
    {
        public int isRawImage;
        public int isThumbnailMode;
        public int resizeLongSideLength;
    }

    /// <summary>
    /// 画像処理を行うネイティブメソッドを管理するクラス
    /// </summary>
    internal static class ImageReadLibrary
    {
        /// <summary>
        /// 画像読み込みライブラリのインスタンスを生成する
        /// </summary>
        /// <returns>画像読み込みライブラリのインスタンス</returns>
        [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateInstance();

        /// <summary>
        /// 画像読み込みライブラリのインスタンスを破棄する
        /// </summary>
        /// <param name="handle">画像読み込みライブラリのインスタンス</param>
        [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern void DeleteInstance(IntPtr handle);

        /// <summary>
        /// 画像を読み込み、画像サイズを取得する
        /// </summary>
        /// <param name="handle">ImageReaderのハンドル</param>
        /// <param name="imagePath">画像ファイルパス</param>
        /// <param name="imageReadSettingPtr">画像読み込み設定</param>
        /// <param name="imageSize">画像サイズ</param>
        /// <returns>成功: True, 失敗: False</returns>
        [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern bool LoadImageAndGetImageSize(ImageReaderHandle handle, [MarshalAs(UnmanagedType.LPWStr), In] string imagePath, [In] IntPtr imageReadSettingPtr, out int imageSize);

        /// <summary>
        /// 画像データを取得する
        /// </summary>
        /// <param name="handle">ImageReaderのハンドル</param>
        /// <param name="imageData">画像データ</param>
        /// <returns>成功: True, 失敗: False</returns>
        [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern bool GetImageData(ImageReaderHandle handle, ref ImageData imageData);

        /// <summary>
        /// サムネイルデータを取得する
        /// </summary>
        /// <param name="handle">ImageReaderのハンドル</param>
        /// <param name="imageData">画像データ</param>
        /// <returns>成功: True, 失敗: False</returns>
        [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern bool GetThumbnailImageData(ImageReaderHandle handle, ref ImageData imageData);
    }

    /// <summary>
    /// 画像読み込みライブラリのラッパークラス
    /// </summary>
    /// <remarks>
    /// IDisposableを継承し、確実にDispose処理ができるようにしている
    /// </remarks>
    public sealed class ImageReadLibraryWrapper : IDisposable
    {
        private readonly ImageReaderHandle handle;
        private bool disposedValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImageReadLibraryWrapper()
        {
            handle = ImageReaderHandle.Create();

            if (handle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Create");
            }
        }

        /// <summary>
        /// クラス破棄処理
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// マネージド、アンマネージドリソースの破棄
        /// </summary>
        /// <param name="disposing">破棄フラグ</param>
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄する
                    // IDisposable を継承するものは、マネージドオブジェクト
                    if (handle?.IsInvalid == true)
                    {
                        handle.Dispose();
                    }
                }

                // アンマネージドリソースを解放し、ファイナライザをオーバーライドする
                disposedValue = true;
            }
        }

        /// <summary>
        /// 画像を読み込み、画像サイズを取得する
        /// </summary>
        /// <param name="imagePath">画像ファイルパス</param>
        /// <param name="imageReadSettingPtr">画像読み込み設定</param>
        /// <param name="imageSize">画像サイズ</param>
        /// <returns>成功: True, 失敗: False</returns>
        public bool LoadImageAndGetImageSize(string imagePath, in IntPtr imageReadSettingPtr, out int imageSize)
        {
            if (handle.IsInvalid)
            {
                throw new ObjectDisposedException("ハンドルが破棄されています");
            }

            return ImageReadLibrary.LoadImageAndGetImageSize(handle, imagePath, imageReadSettingPtr, out imageSize);
        }

        /// <summary>
        /// 画像データを取得する
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>成功: True, 失敗: False</returns>
        public bool GetImageData(ref ImageData imageData)
        {
            if (handle.IsInvalid)
            {
                throw new ObjectDisposedException("ハンドルが破棄されています");
            }

            return ImageReadLibrary.GetImageData(handle, ref imageData);
        }

        /// <summary>
        /// サムネイルデータを取得する
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>成功: True, 失敗: False</returns>
        public bool GetThumbnailImageData(ref ImageData imageData)
        {
            if (handle.IsInvalid)
            {
                throw new ObjectDisposedException("ハンドルが破棄されています");
            }

            return ImageReadLibrary.GetThumbnailImageData(handle, ref imageData);
        }
    }
}
