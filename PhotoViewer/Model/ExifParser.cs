using System;
using System.Collections.Generic;
using System.IO;

namespace Kchary.PhotoViewer.Model
{
    public static class ExifParser
    {
        /// <summary>
        /// Set exif data in MediaInfo.
        /// </summary>
        [STAThread]
        public static IEnumerable<ExifInfo> GetExifDataFromFile(string filePath)
        {
            var _ = new FileInfo(filePath);
            var shell = new Shell32.Shell();
            var objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            var folderItem = objFolder.ParseName(Path.GetFileName(filePath));

            // Get file name.
            yield return GetFileName(filePath);
            // Get shooting date and time.
            yield return GetMediaDate(objFolder, folderItem);
            // Get camera model information.
            yield return GetCameraModel(objFolder, folderItem);
            // Get camera manufacturer information.
            yield return GetCameraManufacturer(objFolder, folderItem);
            // Get bit depth.
            yield return GetBitDepth(objFolder, folderItem);
            // Get ISO data.
            yield return GetIso(objFolder, folderItem);
            // Get focal length.
            yield return GetFocusLength(objFolder, folderItem);
            // Get metering mode.
            yield return GetMeteringMode(objFolder, folderItem);
            // Get image width.
            yield return GetImageWidth(objFolder, folderItem);
            // Get image height.
            yield return GetImageHeight(objFolder, folderItem);
            // Get image width resolution.
            yield return GetImageResolutionWidth(objFolder, folderItem);
            // Get image height resolution.
            yield return GetImageResolutionHeight(objFolder, folderItem);
            // Get aperture value.
            yield return GetFNumber(objFolder, folderItem);
            // Get shutter speed value.
            yield return GetShutterSpeed(objFolder, folderItem);
            // Get exposure program.
            yield return GetExposeMode(objFolder, folderItem);
            // Get white balance.
            yield return GetWhiteBalance(objFolder, folderItem);
        }

        /// <summary>
        /// Get file name.
        /// </summary>
        /// <param name="filePath">filePath</param>
        private static ExifInfo GetFileName(string filePath)
        {
            var propertyValue = Path.GetFileName(filePath);
            const string PropertyText = "File name";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get shooting date and time.
        /// </summary>
        private static ExifInfo GetMediaDate(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 3);
            const string PropertyText = "Date";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get camera model information.
        /// </summary>
        private static ExifInfo GetCameraModel(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 30);
            const string PropertyText = "Camera model";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get camera manufacturer information.
        /// </summary>
        private static ExifInfo GetCameraManufacturer(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 32);
            const string PropertyText = "Camera manufacturer";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get image width.
        /// </summary>
        private static ExifInfo GetImageWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Width
            var propertyValue = objFolder.GetDetailsOf(folderItem, 176);
            const string PropertyText = "Width";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get image height.
        /// </summary>
        private static ExifInfo GetImageHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Height
            var propertyValue = objFolder.GetDetailsOf(folderItem, 178);
            const string PropertyText = "Height";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get image width resolution.
        /// </summary>
        private static ExifInfo GetImageResolutionWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Horizon resolution
            var propertyValue = objFolder.GetDetailsOf(folderItem, 175);
            const string PropertyText = "Horizon resolution";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get image height resolution.
        /// </summary>
        private static ExifInfo GetImageResolutionHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Vertical resolution
            var propertyValue = objFolder.GetDetailsOf(folderItem, 177);
            const string PropertyText = "Vertical resolution";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get bit depth.
        /// </summary>
        private static ExifInfo GetBitDepth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 174);
            const string PropertyText = "Bit depth";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get shutter speed and aperture value.
        /// </summary>
        private static ExifInfo GetShutterSpeed(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Shutter speed
            var propertyValue = objFolder.GetDetailsOf(folderItem, 259);
            const string PropertyText = "Shutter speed";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get aperture value.
        /// </summary>
        private static ExifInfo GetFNumber(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // F number
            var propertyValue = objFolder.GetDetailsOf(folderItem, 260);
            const string PropertyText = "F number";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get ISO data.
        /// </summary>
        private static ExifInfo GetIso(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 264);
            const string PropertyText = "ISO";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get focal length.
        /// </summary>
        private static ExifInfo GetFocusLength(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 262);
            const string PropertyText = "Focal length";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get exposure program and white balance.
        /// </summary>
        private static ExifInfo GetExposeMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Exposure program
            var propertyValue = objFolder.GetDetailsOf(folderItem, 258);
            const string PropertyText = "Exposure program";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get  white balance.
        /// </summary>
        private static ExifInfo GetWhiteBalance(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // White balance
            var propertyValue = objFolder.GetDetailsOf(folderItem, 275);
            const string PropertyText = "White balance";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// Get metering mode.
        /// </summary>
        private static ExifInfo GetMeteringMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 269);
            const string PropertyText = "Metering mode";
            return new ExifInfo(PropertyText, propertyValue);
        }
    }
}