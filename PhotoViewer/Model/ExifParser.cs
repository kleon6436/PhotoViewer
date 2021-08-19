using System;
using System.IO;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// プロパティ名のタイプ
    /// </summary>
    public enum PropertyType
    {
        FileName,
        Date,
        CameraModel,
        CameraManufacturer,
        ImageWidth,
        ImageHeight,
        HorizonResolution,
        VerticalResolution,
        Bitdepth,
        ShutterSpeed,
        Fnumber,
        Iso,
        FocalLength,
        ExposureProgram,
        WhiteBalance,
        MeteringMode
    }

    /// <summary>
    /// Exif情報のパースクラス
    /// </summary>
    public static class ExifParser
    {
        private const string FileNameProperty = "File name";
        private const string DateProperty = "Date";
        private const string CameraModelProperty = "Camera model";
        private const string CameraManufacturerProperty = "Camera manufacturer";
        private const string ImageWidthProperty = "Width";
        private const string ImageHeightProperty = "Height";
        private const string HorizonResolutionProperty = "Horizon resolution";
        private const string VerticalResolutionProperty = "Vertical resolution";
        private const string BitdepthProperty = "Bit depth";
        private const string ShutterSpeedProperty = "Shutter speed";
        private const string FnumberProperty = "F number";
        private const string IsoProperty = "ISO";
        private const string FocalLengthProperty = "Focal length";
        private const string ExposureProgramProperty = "Exposure program";
        private const string WhiteBalanceProperty = "White balance";
        private const string MeteringModeProperty = "Metering mode";

        /// <summary>
        /// Exif情報表示の値なしのリストを作成する
        /// </summary>
        /// <returns></returns>
        public static void CreateExifDefaultList(ExifInfo[] exifDataList)
        {
            var count = 0;
            foreach (PropertyType propertyType in Enum.GetValues<PropertyType>())
            {
                exifDataList[count] = new ExifInfo(GetPropertyName(propertyType), "", propertyType);
                count++;
            }
        }

        /// <summary>
        /// ファイルからExif情報を取得してリストに設定する
        /// </summary>
        /// <param name="filePath">画像のファイルパス</param>
        /// <param name="exifDataList">読み込んだExif情報を設定するリスト</param>
        [STAThread]
        public static void SetExifDataFromFile(string filePath, ExifInfo[] exifDataList)
        {
            var shell = new Shell32.Shell();
            var objFolder = shell.NameSpace(Path.GetDirectoryName(filePath));
            var folderItem = objFolder.ParseName(Path.GetFileName(filePath));

            foreach (var exifInfo in exifDataList)
            {
                switch (exifInfo.ExifPropertyType)
                {
                    case PropertyType.FileName:
                        exifInfo.ExifParameterValue = Path.GetFileName(filePath);
                        break;

                    case PropertyType.Date:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 3);
                        break;

                    case PropertyType.CameraModel:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 30);
                        break;

                    case PropertyType.CameraManufacturer:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 32);
                        break;

                    case PropertyType.ImageWidth:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 176);
                        break;

                    case PropertyType.ImageHeight:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 178);
                        break;

                    case PropertyType.HorizonResolution:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 175);
                        break;

                    case PropertyType.VerticalResolution:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 177);
                        break;

                    case PropertyType.Bitdepth:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 174);
                        break;

                    case PropertyType.ShutterSpeed:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 259);
                        break;

                    case PropertyType.Fnumber:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 260);
                        break;

                    case PropertyType.Iso:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 264);
                        break;

                    case PropertyType.FocalLength:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 262);
                        break;

                    case PropertyType.ExposureProgram:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 258);
                        break;

                    case PropertyType.WhiteBalance:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 275);
                        break;

                    case PropertyType.MeteringMode:
                        exifInfo.ExifParameterValue = objFolder.GetDetailsOf(folderItem, 269);
                        break;

                    default:
                        break;
                }
            }
        }

        private static string GetPropertyName(PropertyType propertyType)
        {
            return propertyType switch
            {
                PropertyType.FileName => FileNameProperty,
                PropertyType.Date => DateProperty,
                PropertyType.CameraModel => CameraModelProperty,
                PropertyType.CameraManufacturer => CameraManufacturerProperty,
                PropertyType.ImageWidth => ImageWidthProperty,
                PropertyType.ImageHeight => ImageHeightProperty,
                PropertyType.HorizonResolution => HorizonResolutionProperty,
                PropertyType.VerticalResolution => VerticalResolutionProperty,
                PropertyType.Bitdepth => BitdepthProperty,
                PropertyType.ShutterSpeed => ShutterSpeedProperty,
                PropertyType.Fnumber => FnumberProperty,
                PropertyType.Iso => IsoProperty,
                PropertyType.FocalLength => FocalLengthProperty,
                PropertyType.ExposureProgram => ExposureProgramProperty,
                PropertyType.WhiteBalance => WhiteBalanceProperty,
                PropertyType.MeteringMode => MeteringModeProperty,
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType)),
            };
        }
    }
}