using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoViewerUnitTest
{
    [TestClass]
    public class ImageReadLibraryWrapperTest
    {
        [TestMethod]
        public void GetThumbnailImageDataTest()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";
            const int longSideLength = 4928;

            // 画像を読み込む
            var isThumbnailMode = true;
            ImageReaderSettingsWrapper imageReadSettings = new()
            {
                IsRawImage = false,
                IsThumbnailMode = isThumbnailMode,
                ResizeLongSideLength = longSideLength,
            };

            ImageDataWrapper imageData = new();
            ImageReaderWrapper imageReader = new();
            if (!imageReader.GetImageData(ImagePath, imageReadSettings, imageData))
            {
                Assert.Fail("Failed to get image");
            }

            Assert.AreEqual(4928, imageData.Width);
            Assert.AreEqual(3264, imageData.Height);
            Assert.AreEqual(14784, imageData.Stride);
        }

        [TestMethod]
        public void GetNormalImageDataTest()
        {
            const string ImagePath = @"..\..\..\..\TestData\Mountain.jpg";
            const int longSideLength = 4928;

            // 画像を読み込む
            var isThumbnailMode = false;
            ImageReaderSettingsWrapper imageReadSettings = new()
            {
                IsRawImage = false,
                IsThumbnailMode = isThumbnailMode,
                ResizeLongSideLength = longSideLength,
            };

            ImageDataWrapper imageData = new();
            ImageReaderWrapper imageReader = new();
            if (!imageReader.GetImageData(ImagePath, imageReadSettings, imageData))
            {
                Assert.Fail("Failed to get image");
            }

            Assert.AreEqual(4928, imageData.Width);
            Assert.AreEqual(3264, imageData.Height);
            Assert.AreEqual(14784, imageData.Stride);
        }
    }
}
