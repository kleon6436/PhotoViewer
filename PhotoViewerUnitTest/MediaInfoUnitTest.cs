using System.IO;
using Kchary.PhotoViewer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoViewerUnitTest
{
    [TestClass]
    public class MediaInfoUnitTest
    {
        [TestMethod]
        public void CreateThumbnailImage()
        {
            const string FilePath = @".\..\..\..\..\TestData\Mountain.jpg";
            const string FileName = "Mountain.jpg";

            var mediaInfo = new MediaInfo()
            {
                FilePath = FilePath
            };
            mediaInfo.FileName = Path.GetFileName(mediaInfo.FilePath);

            // Create thumbnail image test.
            Assert.IsTrue(mediaInfo.CreateThumbnailImage());

            // Check filePath and fileName.
            Assert.AreEqual(FilePath, mediaInfo.FilePath);
            Assert.AreEqual(FileName, mediaInfo.FileName);
        }
    }
}
