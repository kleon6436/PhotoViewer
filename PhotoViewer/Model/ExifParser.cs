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
            Shell32.Folder objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            Shell32.FolderItem folderItem = objFolder.ParseName(Path.GetFileName(filePath));

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
            yield return GetISO(objFolder, folderItem);
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
            yield return GetFnumber(objFolder, folderItem);
            // Get shutter speed value.
            yield return GetShutterSpeed(objFolder, folderItem);
            // Get exposure program.
            yield return GetExposeMode(objFolder, folderItem);
            // Get white balance.
            yield return GetWhiteBlance(objFolder, folderItem);
        }

        /// <summary>
        /// Get file name.
        /// </summary>
        /// <param name="filePath">filePath</param>
        private static ExifInfo GetFileName(string filePath)
        {
            var propertyValue = Path.GetFileName(filePath);
            var propertyText = "File name";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get shooting date and time.
        /// </summary>
        private static ExifInfo GetMediaDate(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 3);
            var propertyText = "Date";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get camera model information.
        /// </summary>
        private static ExifInfo GetCameraModel(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 30);
            var propertyText = "Camera model";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get camera manufacturer information.
        /// </summary>
        private static ExifInfo GetCameraManufacturer(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 32);
            var propertyText = "Camera manufacturer";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get image width.
        /// </summary>
        private static ExifInfo GetImageWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Width
            var propertyValue = objFolder.GetDetailsOf(folderItem, 176);
            var propertyText = "Width";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get image height.
        /// </summary>
        private static ExifInfo GetImageHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Height
            var propertyValue = objFolder.GetDetailsOf(folderItem, 178);
            var propertyText = "Height";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get image width resolution.
        /// </summary>
        private static ExifInfo GetImageResolutionWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Horizon resolution
            var propertyValue = objFolder.GetDetailsOf(folderItem, 175);
            var propertyText = "Horizon resolution";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get image height resolution.
        /// </summary>
        private static ExifInfo GetImageResolutionHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Vertical resolution
            var propertyValue = objFolder.GetDetailsOf(folderItem, 177);
            var propertyText = "Vertical resolution";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get bit depth.
        /// </summary>
        private static ExifInfo GetBitDepth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 174);
            var propertyText = "Bit depth";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get shutter speed and aperture value.
        /// </summary>
        private static ExifInfo GetShutterSpeed(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Shutter speed
            var propertyValue = objFolder.GetDetailsOf(folderItem, 259);
            var propertyText = "Shutter speed";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get aperture value.
        /// </summary>
        private static ExifInfo GetFnumber(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // F number
            var propertyValue = objFolder.GetDetailsOf(folderItem, 260);
            var propertyText = "F number";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get ISO data.
        /// </summary>
        private static ExifInfo GetISO(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 264);
            var propertyText = "ISO";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get focal length.
        /// </summary>
        private static ExifInfo GetFocusLength(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 262);
            var propertyText = "Focal length";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get exposure program and white balance.
        /// </summary>
        private static ExifInfo GetExposeMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // Exposure program
            var propertyValue = objFolder.GetDetailsOf(folderItem, 258);
            var propertyText = "Exposure program";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get  white balance.
        /// </summary>
        private static ExifInfo GetWhiteBlance(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            // White balance
            var propertyValue = objFolder.GetDetailsOf(folderItem, 275);
            var propertyText = "White balance";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get metering mode.
        /// </summary>
        private static ExifInfo GetMeteringMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 269);
            var propertyText = "Metering mode";
            return new ExifInfo(propertyText, propertyValue);
        }
    }
}