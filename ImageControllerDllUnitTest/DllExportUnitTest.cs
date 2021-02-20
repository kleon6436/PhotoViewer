using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace ImageControllerDllUnitTest
{
    [TestClass]
    public class DllExportUnitTest
    {
        /// <summary>
        /// Native image control method class.
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetRawImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, out ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetRawThumbnailImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, [In] int longSideLength, out ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetNormalImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, out ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            internal static extern int GetNormalThumbnailImageData([MarshalAs(UnmanagedType.LPWStr), In] string path, [In] int longSideLength, out ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeBuffer(IntPtr buffer);

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

        [TestMethod]
        public void GetRawImageData()
        {
            const string ImagePath = @"..\..\..\..\TestData\Penguins.NEF";

            if (NativeMethods.GetRawImageData(ImagePath, out var imageData) != 0)
            {
                Assert.Fail("File format is wrong.");
            }

            var imgData = new byte[imageData.size];
            Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
            NativeMethods.FreeBuffer(imageData.buffer);

            Assert.AreEqual(3292, imageData.width);
            Assert.AreEqual(4940, imageData.height);
            Assert.AreEqual((uint)48787440, imageData.size);
            Assert.AreEqual(9876, imageData.stride);
        }

        [TestMethod]
        public void GetRawThumbnailImageData()
        {
            const string ImagePath = @"..\..\..\..\TestData\Penguins.NEF";
            const int LongSideLength = 2200;

            if (NativeMethods.GetRawThumbnailImageData(ImagePath, LongSideLength, out var imageData) != 0)
            {
                Assert.Fail("File format is wrong.");
            }

            var imgData = new byte[imageData.size];
            Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
            NativeMethods.FreeBuffer(imageData.buffer);

            Assert.AreEqual(1464, imageData.width);
            Assert.AreEqual(2200, imageData.height);
            Assert.AreEqual((uint)9662400, imageData.size);
            Assert.AreEqual(4392, imageData.stride);
        }

        [TestMethod]
        public void GetNormalImageData()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";

            if (NativeMethods.GetNormalImageData(ImagePath, out var imageData) != 0)
            {
                Assert.Fail("File format is wrong.");
            }

            var imgData = new byte[imageData.size];
            Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
            NativeMethods.FreeBuffer(imageData.buffer);

            Assert.AreEqual(4928, imageData.width);
            Assert.AreEqual(3264, imageData.height);
            Assert.AreEqual((uint)48254976, imageData.size);
            Assert.AreEqual(14784, imageData.stride);
        }

        [TestMethod]
        public void GetNormalThumbnailImageData()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";
            const int LongSideLength = 2200;

            if (NativeMethods.GetNormalThumbnailImageData(ImagePath, LongSideLength, out var imageData) != 0)
            {
                Assert.Fail("File format is wrong.");
            }

            var imgData = new byte[imageData.size];
            Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
            NativeMethods.FreeBuffer(imageData.buffer);

            Assert.AreEqual(2200, imageData.width);
            Assert.AreEqual(1457, imageData.height);
            Assert.AreEqual((uint)9616200, imageData.size);
            Assert.AreEqual(6600, imageData.stride);
        }
    }
}
