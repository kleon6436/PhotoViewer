using System.Collections.Generic;
using System.IO;

namespace Kchary.PhotoViewer.Model
{
    public static class ExifParser
    {
        /// <summary>
        /// Set exif data in MediaInfo.
        /// </summary>
        public static List<ExifInfo> GetExifDataFromFile(string filePath)
        {
            var _ = new FileInfo(filePath);
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            Shell32.FolderItem folderItem = objFolder.ParseName(Path.GetFileName(filePath));

            var exifInfos = new List<ExifInfo>
            {
                // Get file name.
                GetFileName(filePath),
                // Get shooting date and time.
                GetMediaDate(objFolder, folderItem),
                // Get camera model information.
                GetCameraModel(objFolder, folderItem),
                // Get camera manufacturer information.
                GetCameraManufacturer(objFolder, folderItem),
                // Get bit depth.
                GetBitDepth(objFolder, folderItem),
                // Get ISO data.
                GetISO(objFolder, folderItem),
                // Get focal length.
                GetFocusLength(objFolder, folderItem),
                // Get metering mode.
                GetMeteringMode(objFolder, folderItem),
            };

            // Get image width and height.
            exifInfos.AddRange(GetImageWidthAndHeight(objFolder, folderItem));
            // Get image resolution.
            exifInfos.AddRange(GetImageResolutionWidthAndHeight(objFolder, folderItem));
            // Get shutter speed and aperture value.
            exifInfos.AddRange(GetFnumberAndShutterSpeed(objFolder, folderItem));
            // Get exposure program and white balance.
            exifInfos.AddRange(GetExposeModeAndWhiteBlance(objFolder, folderItem));

            return exifInfos;
        }

        /// <summary>
        /// Get file name.
        /// </summary>
        /// <param name="filePath">filePath</param>
        private static ExifInfo GetFileName(string filePath)
        {
            string propertyValue = Path.GetFileName(filePath);
            string propertyText = "ファイル名";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get shooting date and time.
        /// </summary>
        private static ExifInfo GetMediaDate(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 3);
            string propertyText = "撮影日時";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get camera model information.
        /// </summary>
        private static ExifInfo GetCameraModel(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 30);
            string propertyText = "カメラモデル";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get camera manufacturer information.
        /// </summary>
        private static ExifInfo GetCameraManufacturer(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 32);
            string propertyText = "カメラ製造元";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get image width and height.
        /// </summary>
        private static List<ExifInfo> GetImageWidthAndHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            List<ExifInfo> imageWidthAndHeightInfo = new List<ExifInfo>();

            // 幅
            string propertyValue = objFolder.GetDetailsOf(folderItem, 176);
            string propertyText = "幅";
            imageWidthAndHeightInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 高さ
            propertyValue = objFolder.GetDetailsOf(folderItem, 178);
            propertyText = "高さ";
            imageWidthAndHeightInfo.Add(new ExifInfo(propertyText, propertyValue));

            return imageWidthAndHeightInfo;
        }

        /// <summary>
        /// Get image resolution.
        /// </summary>
        private static List<ExifInfo> GetImageResolutionWidthAndHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            List<ExifInfo> imageResolutionInfo = new List<ExifInfo>();

            // 水平解像度
            string propertyValue = objFolder.GetDetailsOf(folderItem, 175);
            string propertyText = "水平解像度";
            imageResolutionInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 垂直解像度
            propertyValue = objFolder.GetDetailsOf(folderItem, 177);
            propertyText = "垂直解像度";
            imageResolutionInfo.Add(new ExifInfo(propertyText, propertyValue));

            return imageResolutionInfo;
        }

        /// <summary>
        /// Get bit depth.
        /// </summary>
        private static ExifInfo GetBitDepth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 174);
            string propertyText = "ビット深度";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get shutter speed and aperture value.
        /// </summary>
        private static List<ExifInfo> GetFnumberAndShutterSpeed(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            List<ExifInfo> shutterSpeedAndApertureInfo = new List<ExifInfo>();

            // シャッター速度
            string propertyValue = objFolder.GetDetailsOf(folderItem, 259);
            string propertyText = "シャッター速度";
            shutterSpeedAndApertureInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 絞り値
            propertyValue = objFolder.GetDetailsOf(folderItem, 260);
            propertyText = "絞り値";
            shutterSpeedAndApertureInfo.Add(new ExifInfo(propertyText, propertyValue));

            return shutterSpeedAndApertureInfo;
        }

        /// <summary>
        /// Get ISO data.
        /// </summary>
        private static ExifInfo GetISO(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 264);
            string propertyText = "ISO";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get focal length.
        /// </summary>
        private static ExifInfo GetFocusLength(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 262);
            string propertyText = "焦点距離";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// Get exposure program and white balance.
        /// </summary>
        private static List<ExifInfo> GetExposeModeAndWhiteBlance(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            List<ExifInfo> exposeModeAndWhiteBlanceInfo = new List<ExifInfo>();

            // 露出プログラム
            string propertyValue = objFolder.GetDetailsOf(folderItem, 258);
            string propertyText = "露出プログラム";
            exposeModeAndWhiteBlanceInfo.Add(new ExifInfo(propertyText, propertyValue));

            // ホワイトバランス
            propertyValue = objFolder.GetDetailsOf(folderItem, 275);
            propertyText = "ホワイトバランス";
            exposeModeAndWhiteBlanceInfo.Add(new ExifInfo(propertyText, propertyValue));

            return exposeModeAndWhiteBlanceInfo;
        }

        /// <summary>
        /// Get metering mode.
        /// </summary>
        private static ExifInfo GetMeteringMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 269);
            string propertyText = "測光モード";
            return new ExifInfo(propertyText, propertyValue);
        }
    }
}