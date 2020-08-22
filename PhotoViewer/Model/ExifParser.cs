using System.Collections.Generic;
using System.IO;

namespace PhotoViewer.Model
{
    public static class ExifParser
    {
        /// <summary>
        /// ExifデータをMediaInfoにセットするメソッド
        /// </summary>
        public static List<ExifInfo> GetExifDataFromFile(string filePath)
        {
            var _ = new FileInfo(filePath);
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            Shell32.FolderItem folderItem = objFolder.ParseName(Path.GetFileName(filePath));

            var exifInfos = new List<ExifInfo>
            {
                // ファイル名を取得
                GetFileName(filePath),
                // 撮影日時を取得
                GetMediaDate(objFolder, folderItem),
                // カメラモデルの情報を取得
                GetCameraModel(objFolder, folderItem),
                // カメラ製造元の情報を取得
                GetCameraManufacturer(objFolder, folderItem),
                // ビットの深さを取得
                GetBitDepth(objFolder, folderItem),
                // ISOを取得
                GetISO(objFolder, folderItem),
                // 焦点距離を取得
                GetFocusLength(objFolder, folderItem),
                // 測光モードを取得
                GetMeteringMode(objFolder, folderItem),
            };

            // 画像の幅と高さを取得
            exifInfos.AddRange(GetImageWidthAndHeight(objFolder, folderItem));
            // 画像の解像度を取得
            exifInfos.AddRange(GetImageResolutionWidthAndHeight(objFolder, folderItem));
            // シャッター速度と絞り値を取得
            exifInfos.AddRange(GetFnumberAndShutterSpeed(objFolder, folderItem));
            // 露出プログラムとホワイトバランスを取得
            exifInfos.AddRange(GetExposeModeAndWhiteBlance(objFolder, folderItem));

            return exifInfos;
        }

        /// <summary>
        /// ファイル名を取得する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        private static ExifInfo GetFileName(string filePath)
        {
            string propertyValue = Path.GetFileName(filePath);
            string propertyText = "ファイル名";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 撮影日時の情報を取得する
        /// </summary>
        private static ExifInfo GetMediaDate(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 3);
            string propertyText = "撮影日時";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// カメラモデルの情報を取得する
        /// </summary>
        private static ExifInfo GetCameraModel(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 30);
            string propertyText = "カメラモデル";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// カメラ製造元の情報を取得する
        /// </summary>
        private static ExifInfo GetCameraManufacturer(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 32);
            string propertyText = "カメラ製造元";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 画像の幅と高さの情報を取得する
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
        /// 画像の解像度の情報を取得する
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
        /// 画像のビットの深さ情報を取得する
        /// </summary>
        private static ExifInfo GetBitDepth(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 174);
            string propertyText = "ビット深度";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// シャッタ―速度と絞り値の情報を取得する
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
        /// ISO情報を取得する
        /// </summary>
        private static ExifInfo GetISO(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 264);
            string propertyText = "ISO";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 焦点距離の情報を取得する
        /// </summary>
        private static ExifInfo GetFocusLength(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 262);
            string propertyText = "焦点距離";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 露出プログラムとホワイトバランスの情報を取得する
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
        /// 測光モードの情報をピクチャコンテンツの情報に設定する
        /// </summary>
        private static ExifInfo GetMeteringMode(Shell32.Folder objFolder, Shell32.FolderItem folderItem)
        {
            string propertyValue = objFolder.GetDetailsOf(folderItem, 269);
            string propertyText = "測光モード";
            return new ExifInfo(propertyText, propertyValue);
        }
    }
}