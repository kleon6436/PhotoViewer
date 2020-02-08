using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;
using System.IO;

namespace PhotoViewer.Model
{
    public static class ExifParser
    {
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
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetMediaDate(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.DateModified;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            string propertyText = "撮影日時";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// カメラモデルの情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetCameraModel(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Photo.CameraModel;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value;
            }

            string propertyText = "カメラモデル";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// カメラ製造元の情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetCameraManufacturer(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Photo.CameraManufacturer;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "";
            }
            else
            {
                propertyValue = property.Value;
            }

            string propertyText = "カメラ製造元";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 画像の幅と高さの情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static List<ExifInfo> GetImageWidthAndHeight(ShellObject shell)
        {
            string propertyValue = "Null";
            List<ExifInfo> imageWidthAndHeightInfo = new List<ExifInfo>();

            // 幅
            var property = shell.Properties.System.Image.HorizontalSize;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            string propertyText = "幅";
            imageWidthAndHeightInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 高さ
            property = shell.Properties.System.Image.VerticalSize;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            propertyText = "高さ";
            imageWidthAndHeightInfo.Add(new ExifInfo(propertyText, propertyValue));

            return imageWidthAndHeightInfo;
        }

        /// <summary>
        /// 画像の解像度の情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static List<ExifInfo> GetImageResolutionWidthAndHeight(ShellObject shell)
        {
            string propertyValue = "Null";
            List<ExifInfo> imageResolutionInfo = new List<ExifInfo>();

            // 水平解像度
            var property = shell.Properties.System.Image.HorizontalResolution;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            string propertyText = "水平解像度";
            imageResolutionInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 垂直解像度
            property = shell.Properties.System.Image.VerticalResolution;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            propertyText = "垂直解像度";
            imageResolutionInfo.Add(new ExifInfo(propertyText, propertyValue));

            return imageResolutionInfo;
        }

        /// <summary>
        /// 画像のビットの深さ情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetBitDepth(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Image.BitDepth;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            string propertyText = "ビット深度";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// シャッタ―速度と絞り値の情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static List<ExifInfo> GetFnumberAndShutterSpeed(ShellObject shell)
        {
            string propertyValue = "Null";
            List<ExifInfo> shutterSpeedAndApertureInfo = new List<ExifInfo>();

            // シャッター速度
            var denominatorProperty = shell.Properties.System.Photo.ExposureTimeDenominator;
            var numeratorProperty = shell.Properties.System.Photo.ExposureTimeNumerator;
            if (denominatorProperty?.ValueAsObject == null || numeratorProperty?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                int _denominator = (int)denominatorProperty.Value.Value;
                int _numerator = (int)numeratorProperty.Value.Value;

                // 最大公約数を求める
                int commonFactor = _denominator % _numerator;
                if (commonFactor == 0)
                {
                    commonFactor = _numerator;
                }

                // 約分したシャッタースピードを表示する
                propertyValue = (numeratorProperty.Value.Value / commonFactor).ToString() + "/" + (denominatorProperty.Value.Value / commonFactor).ToString();
            }

            string propertyText = "シャッター速度";
            shutterSpeedAndApertureInfo.Add(new ExifInfo(propertyText, propertyValue));

            // 絞り値
            var property = shell.Properties.System.Photo.FNumber;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            propertyText = "絞り値";
            shutterSpeedAndApertureInfo.Add(new ExifInfo(propertyText, propertyValue));

            return shutterSpeedAndApertureInfo;
        }

        /// <summary>
        /// ISO情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetISO(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Photo.ISOSpeed;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value.ToString();
            }

            string propertyText = "ISO";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 焦点距離の情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetFocusLength(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Photo.FocalLength;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = ((uint)property.Value).ToString();
            }

            string propertyText = "焦点距離";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// 露出プログラムとホワイトバランスの情報を取得する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static List<ExifInfo> GetExposeModeAndWhiteBlance(ShellObject shell)
        {
            string propertyValue = "Null";
            List<ExifInfo> exposeModeAndWhiteBlanceInfo = new List<ExifInfo>();

            // 露出プログラム
            var property = shell.Properties.System.Photo.ExposureProgramText;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value;
            }

            string propertyText = "露出プログラム";
            exposeModeAndWhiteBlanceInfo.Add(new ExifInfo(propertyText, propertyValue));

            // ホワイトバランス
            property = shell.Properties.System.Photo.WhiteBalanceText;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value;
            }

            propertyText = "ホワイトバランス";
            exposeModeAndWhiteBlanceInfo.Add(new ExifInfo(propertyText, propertyValue));

            return exposeModeAndWhiteBlanceInfo;
        }

        /// <summary>
        /// 測光モードの情報をピクチャコンテンツの情報に設定する
        /// </summary>
        /// <param name="shell">ShellObject</param>
        private static ExifInfo GetMeteringMode(ShellObject shell)
        {
            string propertyValue = "Null";

            var property = shell.Properties.System.Photo.MeteringModeText;
            if (property?.ValueAsObject == null)
            {
                propertyValue = "Null";
            }
            else
            {
                propertyValue = property.Value;
            }

            string propertyText = "測光モード";
            return new ExifInfo(propertyText, propertyValue);
        }

        /// <summary>
        /// ExifデータをMediaInfoにセットするメソッド
        /// </summary>
        public static List<ExifInfo> GetExifDataFromFile(string filePath)
        {
            //  ファイルからプロパティを取得
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                List<ExifInfo> exifInfos = new List<ExifInfo>();

                // ファイル名を取得
                exifInfos.Add(GetFileName(filePath));
                // 撮影日時を取得
                exifInfos.Add(GetMediaDate(shell));
                // カメラモデルの情報を取得
                exifInfos.Add(GetCameraModel(shell));
                // カメラ製造元の情報を取得
                exifInfos.Add(GetCameraManufacturer(shell));
                // 画像の幅と高さを取得
                exifInfos.AddRange(GetImageWidthAndHeight(shell));
                // 画像の解像度を取得
                exifInfos.AddRange(GetImageResolutionWidthAndHeight(shell));
                // ビットの深さを取得
                exifInfos.Add(GetBitDepth(shell));
                // シャッター速度と絞り値を取得
                exifInfos.AddRange(GetFnumberAndShutterSpeed(shell));
                // ISOを取得
                exifInfos.Add(GetISO(shell));
                // 焦点距離を取得
                exifInfos.Add(GetFocusLength(shell));
                // 測光モードを取得
                exifInfos.Add(GetMeteringMode(shell));
                // 露出プログラムとホワイトバランスを取得
                exifInfos.AddRange(GetExposeModeAndWhiteBlance(shell));

                return exifInfos;
            }
        }
    }
}
