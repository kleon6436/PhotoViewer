using System.Collections.Generic;
using System.Linq;

namespace Kchary.PhotoViewer.Model
{
    public static class MediaChecker
    {
        /// <summary>
        /// Support extensions
        /// </summary>
        private static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };

        /// <summary>
        /// Support raw extensions
        /// </summary>
        private static readonly string[] SupportRawPictureExtensions = { ".dng", ".nef" };

        /// <summary>
        /// Get a list of extensions supported by the app.
        /// </summary>
        /// <returns>Extension list supported</returns>
        public static IEnumerable<string> GetSupportExtentions()
        {
            return SupportPictureExtensions;
        }

        /// <summary>
        /// Check if the extension is a still image supported extension.
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <returns>True: the extension is supported, False: the extension is not supported</returns>
        public static bool CheckPictureExtensions(string extension)
        {
            return SupportPictureExtensions.Any(supportExtension => supportExtension == extension);
        }

        /// <summary>
        /// Check if the file is a Raw File.
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <returns>True: the extension is nef, False: the extension is not nef</returns>
        public static bool CheckRawFileExtension(string extension)
        {
            return SupportRawPictureExtensions.Any(x => x == extension);
        }
    }
}