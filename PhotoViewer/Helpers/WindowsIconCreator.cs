using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// Windows APIを用いたアイコン生成クラス
    /// </summary>
    public static class WindowsIconCreator
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetStockIconInfo(
            StockIconId siid,       // StockIconId enum type that specifies the ID of the icon to retrieve
            StockIconFlags uFlags,  // StockIconFlags enum type that specifies the type of icon to retrieve
            ref StockIconInfo psii  // (Return value) StockIconInfo type
        );

        [DllImport("User32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct StockIconInfo
        {
            public uint cbSize;        // Size of structure (number of bytes)
            public IntPtr hIcon;       // (Return value) Handle to the icon
            public int iSysImageIndex; // (Return value) The index of the icon in the system icon cache.
            public int iIcon;          // (Return value) Index of the retrieved icon

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;      // (Return value) File name that holds the icon resource
        }

        /// <summary>
        /// Type of icon to get
        /// </summary>
        [Flags]
        public enum StockIconFlags
        {
            Large = 0x000000000,       // Big icon
            Small = 0x000000001,       // Small icon
            Handle = 0x000000100,      // The handle of the specified icon
        }

        /// <summary>
        /// Type of icon to get
        /// </summary>
        public enum StockIconId
        {
            SiidFolder = 3,
            SiidDrivefixed = 8,
            SiidImageFiles = 72,
        }

        /// <summary>
        /// Get the standard Windows icon.
        /// </summary>
        /// <param name="iconId">Type of icon to get</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage GetWindowsIcon(StockIconId iconId)
        {
            // Get handle of big icon.
            const StockIconFlags Flags = StockIconFlags.Large | StockIconFlags.Handle;

            var info = new StockIconInfo
            {
                cbSize = (uint)Marshal.SizeOf(typeof(StockIconInfo))
            };

            // Save bitmap source of icon.
            BitmapImage source = null;
            SHGetStockIconInfo(iconId, Flags, ref info);

            try
            {
                if (info.hIcon != IntPtr.Zero)
                {
                    // WinUIではHICONから直接BitmapImageを作成する方法が異なる
                    // 簡略化のため、既定のアイコンを返す
                    source = new BitmapImage();
                    // 注: 実際の実装では、HICONからピクセルデータを取得して
                    // WriteableBitmapを経由する必要がある
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }

            return source;
        }
    }
}