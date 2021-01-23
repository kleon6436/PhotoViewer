using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageControllerWPFTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static class NativeMethods
        {
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetRawImageData(string path, out IntPtr buffer, out uint size, out int stride, out int width, out int height);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetRawThumbnailImageData(string path, int longSideLength, out IntPtr buffer, out uint size, out int stride, out int width, out int height);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetNormalImageData(string path, out IntPtr buffer, out uint size, out int stride, out int width, out int height);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetNormalThumbnailImageData(string path, int longSideLength, out IntPtr buffer, out uint size, out int stride, out int width, out int height);
        }

        public MainWindow()
        {
            InitializeComponent();
            SetImage();
        }

        private void SetImage()
        {
            const string rawImagePath = @"F:\20201101-02_大井川鉄道\Raw\NK8_2503.NEF";

            if (NativeMethods.GetRawThumbnailImageData(rawImagePath, 200, out IntPtr rawbuffer, out uint rawSize, out int rawStride, out int rawWidth, out int rawHeight) != 0)
            {
                return;
            }

            var imgRawData = new byte[rawSize];
            Marshal.Copy(rawbuffer, imgRawData, 0, (int)rawSize);

            //const string normalImagePath = @"F:\20201101-02_大井川鉄道\jpeg\NK8_2503.JPG";

            //if (GetNormalImageData(normalImagePath, out IntPtr normalBuffer, out uint normalSize, out int normalStride, out int normalWidth, out int normalHeight) != 0)
            //{
            //    return;
            //}

            //var imgNormalData = new byte[normalSize];
            //Marshal.Copy(normalBuffer, imgNormalData, 0, (int)normalSize);

            int dpi = 96;
            var bitmap = new WriteableBitmap(rawWidth, rawHeight, dpi, dpi, PixelFormats.Bgr24, null);
            bitmap.WritePixels(new Int32Rect(0, 0, rawWidth, rawHeight), imgRawData, rawStride, 0, 0);

            this.testImage.Source = bitmap;
        }
    }
}
