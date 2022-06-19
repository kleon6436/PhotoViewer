using Kchary.PhotoViewer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace PhotoViewerUnitTest
{
    [TestClass]
    public class MediaCheckerUnitTest
    {
        [TestMethod]
        public void GetSupportExtensions()
        {
            string[] truthSupportExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };
            var supportExtentions = MediaChecker.GetSupportExtentions();

            // Check support extensions
            Assert.IsTrue(supportExtentions.SequenceEqual(truthSupportExtensions));
        }

        [TestMethod]
        public void CheckPictureExtensions()
        {
            string[] truthSupportExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };
            foreach (var extension in truthSupportExtensions)
            {
                Assert.IsTrue(MediaChecker.CheckPictureExtensions(extension));
            }
        }

        [TestMethod]
        public void CheckRawFileExtension()
        {
            string[] truthSupportRawExtensions = { ".dng", ".nef" };
            foreach (var extension in truthSupportRawExtensions)
            {
                Assert.IsTrue(MediaChecker.CheckRawFileExtension(extension));
            }
        }
    }
}
