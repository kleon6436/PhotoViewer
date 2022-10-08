using System;
using System.Runtime.InteropServices;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// C++側で生成されるアンマネージドのハンドルのラッパークラス
    /// </summary>
    /// <remarks>
    /// https://learn.microsoft.com/ja-jp/dotnet/standard/garbage-collection/implementing-dispose#implement-the-dispose-pattern-with-safe-handles
    /// unmanaged handle は SafeHandle でラップすることが推奨されている
    /// </remarks>
    public sealed class ImageReaderHandle : SafeHandle
    {
        // handle の値が 0 の場合、無効として扱う
        public override bool IsInvalid => IntPtr.Zero == handle;

        /// <summary>
        /// ImageReaderハンドルを生成する
        /// </summary>
        /// <returns>ImageReaderのハンドル</returns>
        public static ImageReaderHandle Create()
        {
            return new ImageReaderHandle(ImageReadLibrary.CreateInstance());
        }

        /// <summary>
        /// ImageReaderハンドルを破棄する
        /// </summary>
        /// <returns>True: 成功、False：失敗</returns>
        protected override bool ReleaseHandle()
        {
            ImageReadLibrary.DeleteInstance(handle);
            return true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handle">ハンドル</param>
        private ImageReaderHandle(IntPtr handle) : base(handle, true)
        {
            SetHandle(handle);
        }
    }
}
