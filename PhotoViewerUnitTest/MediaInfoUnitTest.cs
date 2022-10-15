using Kchary.PhotoViewer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoViewerUnitTest
{
    [TestClass]
    public class MediaInfoUnitTest
    {
        [TestMethod]
        public void CreateThumbnailImage()
        {
            const string FilePath = @"..\..\..\..\TestData\Mountain.jpg";
            const string FileName = "Mountain.jpg";

            var mediaInfo = new MediaInfo(FilePath);

            // Check thumbnail image.
            Assert.IsTrue(mediaInfo.ThumbnailImage != null);

            // Check filePath and fileName.
            Assert.AreEqual(FilePath, mediaInfo.FilePath);
            Assert.AreEqual(FileName, mediaInfo.FileName);
        }
    }
}
