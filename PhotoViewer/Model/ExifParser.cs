using System;
using System.Collections.Generic;
using System.IO;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// Exif情報のパースクラス
    /// </summary>
    public static class ExifParser
    {
        /// <summary>
        /// ファイルからExif情報を取得する
        /// </summary>
        /// <param name="filePath">画像のファイルパス</param>
        [STAThread]
        public static IEnumerable<ExifInfo> GetExifDataFromFile(string filePath)
        {
            var _ = new FileInfo(filePath);
            var shell = new Shell32.Shell();
            var objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            var folderItem = objFolder.ParseName(Path.GetFileName(filePath));

            yield return GetFileName(filePath);
            yield return GetMediaDate(objFolder, folderItem);
            yield return GetCameraModel(objFolder, folderItem);
            yield return GetCameraManufacturer(objFolder, folderItem);
            yield return GetBitDepth(objFolder, folderItem);
            yield return GetIso(objFolder, folderItem);
            yield return GetFocusLength(objFolder, folderItem);
            yield return GetMeteringMode(objFolder, folderItem);
            yield return GetImageWidth(objFolder, folderItem);
            yield return GetImageHeight(objFolder, folderItem);
            yield return GetImageResolutionWidth(objFolder, folderItem);
            yield return GetImageResolutionHeight(objFolder, folderItem);
            yield return GetFNumber(objFolder, folderItem);
            yield return GetShutterSpeed(objFolder, folderItem);
            yield return GetExposeMode(objFolder, folderItem);
            yield return GetWhiteBalance(objFolder, folderItem);
        }

        /// <summary>
        /// ファイル名を取得する
        /// </summary>
        /// <param name="filePath">画像のファイルパス</param>
        private static ExifInfo GetFileName(string filePath)
        {
            var propertyValue = Path.GetFileName(filePath);
            const string PropertyText = "File name";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// ファイル更新日時を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetMediaDate(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 3);
            const string PropertyText = "Date";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// カメラ情報を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetCameraModel(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 30);
            const string PropertyText = "Camera model";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// カメラの製造メーカーを取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetCameraManufacturer(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 32);
            const string PropertyText = "Camera manufacturer";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// カメラ情報を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetImageWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 176);
            const string PropertyText = "Width";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 画像の幅と高さを取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetImageHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 178);
            const string PropertyText = "Height";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 画像の解像度(幅側)を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetImageResolutionWidth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 175);
            const string PropertyText = "Horizon resolution";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 画像の解像度(高さ側)を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetImageResolutionHeight(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 177);
            const string PropertyText = "Vertical resolution";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// ビット深度を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetBitDepth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 174);
            const string PropertyText = "Bit depth";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// シャッター速度を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetShutterSpeed(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 259);
            const string PropertyText = "Shutter speed";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// F値を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetFNumber(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 260);
            const string PropertyText = "F number";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// ISO値を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetIso(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 264);
            const string PropertyText = "ISO";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 焦点距離を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetFocusLength(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 262);
            const string PropertyText = "Focal length";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 露光プログラムを取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetExposeMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 258);
            const string PropertyText = "Exposure program";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// ホワイトバランスの情報を取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetWhiteBalance(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 275);
            const string PropertyText = "White balance";
            return new ExifInfo(PropertyText, propertyValue);
        }

        /// <summary>
        /// 露光モードを取得する
        /// </summary>
        /// <param name="objFolder">フォルダ情報</param>
        /// <param name="folderItem">ファイル情報</param>
        private static ExifInfo GetMeteringMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            var propertyValue = objFolder.GetDetailsOf(folderItem, 269);
            const string PropertyText = "Metering mode";
            return new ExifInfo(PropertyText, propertyValue);
        }
    }
}