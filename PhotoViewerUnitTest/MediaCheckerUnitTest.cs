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
            var supportExtentions = AppConfigManager.GetSupportExtentions();

            // Check support extensions
            Assert.IsTrue(supportExtentions.SequenceEqual(truthSupportExtensions));
        }
    }
}
