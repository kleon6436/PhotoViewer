using FastEnumUtility;
using Kchary.PhotoViewer.Helpers;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.Tiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kchary.PhotoViewer.Models
{
    // https://developer.adobe.com/xmp/docs/XMPNamespaces/exif/
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
        BitDepth,
        ShutterSpeed,
        FNumber,
        Iso,
        FocalLength,
        ExposureProgram,
        WhiteBalance,
        MeteringMode
    }

    /// <summary>
    /// 露光プログラムのタイプ定義
    /// </summary>
    public enum ExposureProgramType
    {
        Unknown = 0,
        Manual = 1,
        Normal = 2,
        AperturePriority = 3,
        ShutterPriority = 4,
        CreativeProgram = 5,
        ActionProgram = 6,
        Portrait = 7,
        LandScape = 8
    }

    /// <summary>
    /// 測光モードのタイプ定義
    /// </summary>
    public enum MeteringModeType
    {
        Unknown = 0,
        Average = 1,
        CenterWeightedAverage = 2,
        Spot = 3,
        MultiSpot = 4,
        Pattern = 5,
        Partial = 6,
        Other = 255
    }

    /// <summary>
    /// ホワイトバランスのタイプ定義
    /// </summary>
    public enum WhiteBalanceType
    {
        Unknown = 0,
        Daylight = 1,
        Fluorescent = 2,
        Tungsten = 3,
        Flash = 4,
        FineWeather = 9,
        CloudyWeather = 10,
        Shade = 11,
        DaylightFluorescent = 12,
        DayWhiteFluorescent = 13,
        CoolWhiteFluorescent = 14,
        WhiteFluorescent = 15,
        StandardLightA = 17,
        StandardLightB = 18,
        StandardLightC = 19,
        D55 = 20,
        D65 = 21,
        D75 = 22,
        D50 = 23,
        IsoStudioTungsten = 24,
        Other = 255
    }

    /// <summary>
    /// Exif情報を写真情報から読み込むクラス
    /// </summary>
    public sealed class ExifLoader
    {
        #region ExifInfoに表示するパラメータ名

        private const string FileNameProperty = "File name";
        private const string DateProperty = "Date";
        private const string CameraModelProperty = "Camera model";
        private const string CameraManufacturerProperty = "Camera manufacturer";
        private const string ImageWidthProperty = "Width";
        private const string ImageHeightProperty = "Height";
        private const string HorizonResolutionProperty = "Horizon resolution";
        private const string VerticalResolutionProperty = "Vertical resolution";
        private const string BitDepthProperty = "Bit depth";
        private const string ShutterSpeedProperty = "Shutter speed";
        private const string FNumberProperty = "F number";
        private const string IsoProperty = "ISO";
        private const string FocalLengthProperty = "Focal length";
        private const string ExposureProgramProperty = "Exposure program";
        private const string WhiteBalanceProperty = "White balance";
        private const string MeteringModeProperty = "Metering mode";

        #endregion

        /// <summary>
        /// 写真情報
        /// </summary>
        public PhotoInfo PhotoInfo { private get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExifLoader()
        {
        }

        /// <summary>
        /// Exif情報のデフォルトリストを作成する
        /// </summary>
        /// <returns>Exif情報のデフォルトリスト</returns>
        public static ExifInfo[] CreateExifDefaultList()
        {
            var exifInfos = new ExifInfo[FastEnum.GetNames<PropertyType>().Length];

            var count = 0;
            foreach (var propertyType in FastEnum.GetValues<PropertyType>())
            {
                exifInfos[count] = new ExifInfo(GetPropertyName(propertyType), "", propertyType);
                count++;
            }

            return exifInfos;
        }

        /// <summary>
        /// 写真情報からExif情報のリストを作成する
        /// </summary>
        /// <param name="stopRequest">停止要求フラグ</param>
        /// <returns>Exif情報のリスト</returns>
        public ExifInfo[] CreateExifInfoList(bool stopRequest)
        {
            ExifInfo[] exifInfos = CreateExifDefaultList();

            var filePath = PhotoInfo.FilePath;
            var fileExtensionType = PhotoInfo.FileExtensionType;
            var directories = GetMetadataDirectories(fileExtensionType).ToArray();
            var subIfdDirectories = directories.OfType<ExifSubIfdDirectory>().ToArray();

            Parallel.ForEach(exifInfos, exifInfo =>
            {
                if (stopRequest)
                {
                    throw new OperationCanceledException("Cancelled");
                }

                switch (exifInfo.ExifPropertyType)
                {
                    case PropertyType.FileName:
                        exifInfo.ExifParameterValue = FileUtil.GetFileName(filePath, false);
                        break;

                    case PropertyType.Date:
                        exifInfo.ExifParameterValue = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagDateTime);
                        break;

                    case PropertyType.CameraModel:
                        exifInfo.ExifParameterValue = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagModel);
                        break;

                    case PropertyType.CameraManufacturer:
                        exifInfo.ExifParameterValue = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagMake);
                        break;

                    case PropertyType.ImageWidth:
                        exifInfo.ExifParameterValue = PhotoInfo.IsRawImage
                            ? $"{GetExifDataFromMetadata(subIfdDirectories, ExifDirectoryBase.TagImageWidth)} pixel"
                            : $"{GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExifImageWidth)} pixel";
                        break;

                    case PropertyType.ImageHeight:
                        exifInfo.ExifParameterValue = PhotoInfo.IsRawImage
                            ? $"{GetExifDataFromMetadata(subIfdDirectories, ExifDirectoryBase.TagImageHeight)} pixel"
                            : $"{GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExifImageHeight)} pixel";
                        break;

                    case PropertyType.HorizonResolution:
                        var horizonResolution = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagXResolution);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(horizonResolution) ? $"{horizonResolution} dpi" : horizonResolution;
                        break;

                    case PropertyType.VerticalResolution:
                        var verticalResolution = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagYResolution);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(verticalResolution) ? $"{verticalResolution} dpi" : verticalResolution;
                        break;

                    case PropertyType.BitDepth:
                        var bitDepth = fileExtensionType switch
                        {
                            FileExtensionType.Jpeg => GetExifDataFromMetadata(directories, JpegDirectory.TagDataPrecision),
                            FileExtensionType.Bmp => GetExifDataFromMetadata(directories, BmpHeaderDirectory.TagBitsPerPixel),
                            FileExtensionType.Png => GetExifDataFromMetadata(directories, PngDirectory.TagBitsPerSample),
                            FileExtensionType.Gif => GetExifDataFromMetadata(directories, GifHeaderDirectory.TagBitsPerPixel),
                            FileExtensionType.Tiff or FileExtensionType.Dng or FileExtensionType.Nef => GetExifDataFromMetadata(directories, ExifDirectoryBase.TagBitsPerSample).AsSpan(0, 1),
                            FileExtensionType.Unknown => throw new ArgumentOutOfRangeException(exifInfo.ExifPropertyType.ToString(), nameof(exifInfo.ExifPropertyType)),
                            _ => throw new ArgumentOutOfRangeException(exifInfo.ExifPropertyType.ToString(), nameof(exifInfo.ExifPropertyType)),
                        };
                        exifInfo.ExifParameterValue = $"{bitDepth.ToString()} bits";
                        break;

                    case PropertyType.ShutterSpeed:
                        var shutterSpeed = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExposureTime);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(shutterSpeed) ? $"{shutterSpeed} sec" : shutterSpeed;
                        break;

                    case PropertyType.FNumber:
                        var fNumber = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagFNumber);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(fNumber) ? $"F/{fNumber}" : "";
                        break;

                    case PropertyType.Iso:
                        exifInfo.ExifParameterValue = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagIsoEquivalent);
                        break;

                    case PropertyType.FocalLength:
                        var focalLength = GetExifDataFromMetadata(directories, ExifDirectoryBase.Tag35MMFilmEquivFocalLength);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(focalLength) ? $"{focalLength} mm" : focalLength;
                        break;

                    case PropertyType.ExposureProgram:
                        var exposureProgram = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExposureProgram);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(exposureProgram) ? FastEnum.Parse<ExposureProgramType>(exposureProgram).ToString() : exposureProgram;
                        break;

                    case PropertyType.WhiteBalance:
                        var whiteBalance = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagWhiteBalance);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(whiteBalance) ? FastEnum.Parse<WhiteBalanceType>(whiteBalance).ToString() : whiteBalance;
                        break;

                    case PropertyType.MeteringMode:
                        var meteringMode = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagMeteringMode);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(meteringMode) ? FastEnum.Parse<MeteringModeType>(meteringMode).ToString() : meteringMode;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(exifInfo.ExifPropertyType.ToString(), nameof(exifInfo.ExifPropertyType));

                }
            });

            return exifInfos;
        }

        /// <summary>
        /// メタデータの構造データを取得する
        /// </summary>
        /// <param name="fileExtensionType">ファイルの拡張子</param>
        /// <returns>メタデータの構造データ</returns>
        private IEnumerable<Directory> GetMetadataDirectories(FileExtensionType fileExtensionType)
        {
            return fileExtensionType switch
            {
                FileExtensionType.Jpeg => JpegMetadataReader.ReadMetadata(PhotoInfo.FilePath),
                FileExtensionType.Bmp => BmpMetadataReader.ReadMetadata(PhotoInfo.FilePath),
                FileExtensionType.Png => PngMetadataReader.ReadMetadata(PhotoInfo.FilePath),
                FileExtensionType.Gif => GifMetadataReader.ReadMetadata(PhotoInfo.FilePath),
                FileExtensionType.Tiff or FileExtensionType.Dng or FileExtensionType.Nef => TiffMetadataReader.ReadMetadata(PhotoInfo.FilePath),
                FileExtensionType.Unknown or _ => throw new ArgumentOutOfRangeException(nameof(fileExtensionType)),
            };
        }

        /// <summary>
        /// Exif情報をメタデータから取得する
        /// </summary>
        /// <typeparam name="T">MetadataExtractor.Directory</typeparam>
        /// <param name="metadataDirectories">メタデータの構造データ</param>
        /// <param name="tag">取得したいExifのタグ番号</param>
        private static string GetExifDataFromMetadata<T>(IEnumerable<T> metadataDirectories, int tag)
            where T : Directory
        {
            foreach (var metadataDirectory in metadataDirectories)
            {
                if (metadataDirectory is not Directory exifDirectory)
                {
                    continue;
                }

                // 該当データが見つかった場合は、すぐにその値を返す
                var metadata = exifDirectory.GetString(tag);
                if (!string.IsNullOrEmpty(metadata))
                {
                    return metadata;
                }
            }

            // 見つからなかった場合、空文字とする
            return "";
        }

        /// <summary>
        /// 表示用のExifプロパティ名を取得する
        /// </summary>
        /// <param name="propertyType">プロパティタイプ</param>
        /// <returns>Exifプロパティ名</returns>
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
                PropertyType.BitDepth => BitDepthProperty,
                PropertyType.ShutterSpeed => ShutterSpeedProperty,
                PropertyType.FNumber => FNumberProperty,
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
