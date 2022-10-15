using Kchary.PhotoViewer.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace PhotoViewerUnitTest
{
    [TestClass]
    public class ImageReadLibraryWrapperTest
    {
        [TestMethod]
        public void LoadImageAndGetImageSizeTest()
        {
            const string ImagePath = @"..\..\..\..\TestData\Penguins.NEF";
            const int longSideLength = 2200;

            ImageReadLibraryWrapper imageReadLibraryWrapper = new();
            var imageReadSettingPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ImageReadSettings)));

            try
            {
                // 画像を読み込む
                var isThumbnailMode = true;
                ImageReadSettings imageReadSettings = new()
                {
                    isRawImage = Convert.ToInt32(true),
                    isThumbnailMode = Convert.ToInt32(isThumbnailMode),
                    resizeLongSideLength = longSideLength,
                };
                Marshal.StructureToPtr(imageReadSettings, imageReadSettingPtr, false);
                imageReadLibraryWrapper.LoadImageAndGetImageSize(ImagePath, imageReadSettingPtr, out int imageSize);

                Assert.AreEqual(9662400, imageSize);
            }
            finally
            {
                Marshal.FreeCoTaskMem(imageReadSettingPtr);
                imageReadLibraryWrapper.Dispose();
            }
        }

        [TestMethod]
        public void GetThumbnailImageDataTest()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";
            const int longSideLength = 4928;

            ImageReadLibraryWrapper imageReadLibraryWrapper = new();
            ImageData imageData = new();
            var imageReadSettingPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ImageReadSettings)));

            try
            {
                // 画像を読み込む
                var isThumbnailMode = true;
                ImageReadSettings imageReadSettings = new()
                {
                    isRawImage = Convert.ToInt32(false),
                    isThumbnailMode = Convert.ToInt32(isThumbnailMode),
                    resizeLongSideLength = longSideLength,
                };
                Marshal.StructureToPtr(imageReadSettings, imageReadSettingPtr, false);
                imageReadLibraryWrapper.LoadImageAndGetImageSize(ImagePath, imageReadSettingPtr, out int imageSize);

                Assert.AreEqual(48254976, imageSize);

                // 必要なバッファを準備
                imageData.buffer = Marshal.AllocCoTaskMem(imageSize);

                // 画像データの取得
                imageReadLibraryWrapper.GetThumbnailImageData(ref imageData);

                Assert.AreEqual(4928, imageData.width);
                Assert.AreEqual(3264, imageData.height);
                Assert.AreEqual(48254976, imageSize);
                Assert.AreEqual(14784, imageData.stride);
            }
            finally
            {
                Marshal.FreeCoTaskMem(imageData.buffer);
                Marshal.FreeCoTaskMem(imageReadSettingPtr);
                imageReadLibraryWrapper.Dispose();
            }
        }

        [TestMethod]
        public void GetNormalImageDataTest()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";
            const int longSideLength = 4928;

            ImageReadLibraryWrapper imageReadLibraryWrapper = new();
            ImageData imageData = new();
            var imageReadSettingPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ImageReadSettings)));

            try
            {
                // 画像を読み込む
                var isThumbnailMode = false;
                ImageReadSettings imageReadSettings = new()
                {
                    isRawImage = Convert.ToInt32(false),
                    isThumbnailMode = Convert.ToInt32(isThumbnailMode),
                    resizeLongSideLength = longSideLength,
                };
                Marshal.StructureToPtr(imageReadSettings, imageReadSettingPtr, false);
                imageReadLibraryWrapper.LoadImageAndGetImageSize(ImagePath, imageReadSettingPtr, out int imageSize);

                Assert.AreEqual(48254976, imageSize);

                // 必要なバッファを準備
                imageData.buffer = Marshal.AllocCoTaskMem(imageSize);

                // 画像データの取得
                imageReadLibraryWrapper.GetImageData(ref imageData);

                Assert.AreEqual(4928, imageData.width);
                Assert.AreEqual(3264, imageData.height);
                Assert.AreEqual(48254976, imageSize);
                Assert.AreEqual(14784, imageData.stride);
            }
            finally
            {
                Marshal.FreeCoTaskMem(imageData.buffer);
                Marshal.FreeCoTaskMem(imageReadSettingPtr);
                imageReadLibraryWrapper.Dispose();
            }
        }
    }
}
