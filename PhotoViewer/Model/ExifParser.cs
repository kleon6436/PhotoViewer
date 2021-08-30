using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.Tiff;
using MetadataExtractor.Formats.Gif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    /// 露光プログラムのタイプ定義
    /// </summary>
    public enum ExposureProgramType
    {
        Unknown,
        Manual,
        Normal,
        AperturePriority,
        ShutterPriority,
        Slow,
        HighSpeed,
        Portrait,
        LandScape
    }

    /// <summary>
    /// 測光モードのタイプ定義
    /// </summary>
    public enum MeteringModeType
    {
        Unknown,
        Average,
        CenterWeightedAverage,
        Spot,
        MultiSpot,
        MultiSegment,
        Partial,
        Other = 255
    }

    /// <summary>
    /// ホワイトバランスのタイプ定義
    /// </summary>
    public enum WhiteBlanceType
    {
        Unknown,
        Daylight,
        Fluorescent,
        Tungsten,
        Flash = 10,
        StandardLightA = 18,
        StandardLightB = 19,
        StandardLightC = 20,
        D55 = 21,
        D64 = 22,
        D75 = 255
    }

    /// <summary>
    /// Exif情報のパースクラス
    /// </summary>
    /// <remarks>
    /// Exif規格表のID値を参照する
    /// https://www.cipa.jp/std/documents/j/DC-008-2012_J.pdf
    /// https://github.com/drewnoakes/metadata-extractor-dotnet
    /// </remarks>
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
        public static void SetExifDataFromFile(string filePath, ExifInfo[] exifDataList)
        {
            var extension = Path.GetExtension(filePath);
            var fileExtensionType = MediaChecker.GetFileExtensionType(extension.ToLower());
            var directories = GetMetadataDirectories(filePath, fileExtensionType);
            var subIfdDirectories = directories.OfType<ExifSubIfdDirectory>();

            foreach (var exifInfo in exifDataList)
            {
                switch (exifInfo.ExifPropertyType)
                {
                    case PropertyType.FileName:
                        exifInfo.ExifParameterValue = Path.GetFileName(filePath);
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
                        exifInfo.ExifParameterValue = MediaChecker.CheckRawFileExtension(extension.ToLower())
                            ? $"{GetExifDataFromMetadata(subIfdDirectories, ExifDirectoryBase.TagImageWidth)} pixel"
                            : $"{GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExifImageWidth)} pixel";
                        break;

                    case PropertyType.ImageHeight:
                        exifInfo.ExifParameterValue = MediaChecker.CheckRawFileExtension(extension.ToLower())
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

                    case PropertyType.Bitdepth:
                        var bitdepth = fileExtensionType switch
                        {
                            FileExtensionType.Jpeg => GetExifDataFromMetadata(directories, JpegDirectory.TagDataPrecision),
                            FileExtensionType.Bmp => GetExifDataFromMetadata(directories, BmpHeaderDirectory.TagBitsPerPixel),
                            FileExtensionType.Png => GetExifDataFromMetadata(directories, PngDirectory.TagBitsPerSample),
                            FileExtensionType.Gif => GetExifDataFromMetadata(directories, GifHeaderDirectory.TagBitsPerPixel),
                            FileExtensionType.Tiff or FileExtensionType.Dng or FileExtensionType.Nef => GetExifDataFromMetadata(directories, ExifDirectoryBase.TagBitsPerSample).AsSpan(0, 1),
                            FileExtensionType.Unknown => throw new NotImplementedException(),
                            _ => throw new ArgumentOutOfRangeException(),
                        };
                        exifInfo.ExifParameterValue = bitdepth.ToString();
                        break;

                    case PropertyType.ShutterSpeed:
                        var shutterSpeed = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagExposureTime);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(shutterSpeed) ? $"{shutterSpeed} sec" : shutterSpeed;
                        break;

                    case PropertyType.Fnumber:
                        exifInfo.ExifParameterValue = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagFNumber);
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
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(exposureProgram) ? Enum.Parse<ExposureProgramType>(exposureProgram).ToString() : exposureProgram;
                        break;

                    case PropertyType.WhiteBalance:
                        var whiteBalance = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagWhiteBalance);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(whiteBalance) ? Enum.Parse<WhiteBlanceType>(whiteBalance).ToString() : whiteBalance;
                        break;

                    case PropertyType.MeteringMode:
                        var metaringMode = GetExifDataFromMetadata(directories, ExifDirectoryBase.TagMeteringMode);
                        exifInfo.ExifParameterValue = !string.IsNullOrEmpty(metaringMode) ? Enum.Parse<MeteringModeType>(metaringMode).ToString() : metaringMode;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// メタデータの構造データを取得する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="fileExtensionType">ファイルの拡張子</param>
        /// <returns>メタデータの構造データ</returns>
        private static IEnumerable<MetadataExtractor.Directory> GetMetadataDirectories(string filePath, FileExtensionType fileExtensionType)
        {
            var directories = fileExtensionType switch
            {
                FileExtensionType.Jpeg => JpegMetadataReader.ReadMetadata(filePath),
                FileExtensionType.Bmp => BmpMetadataReader.ReadMetadata(filePath),
                FileExtensionType.Png => PngMetadataReader.ReadMetadata(filePath),
                FileExtensionType.Gif => GifMetadataReader.ReadMetadata(filePath),
                FileExtensionType.Tiff or FileExtensionType.Dng or FileExtensionType.Nef => TiffMetadataReader.ReadMetadata(filePath),
                FileExtensionType.Unknown => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(),
            };

            return directories;
        }

        /// <summary>
        /// Exif情報をメタデータから取得する
        /// </summary>
        /// <typeparam name="T">MetadataExtractor.Directory</typeparam>
        /// <param name="metadataDirectories">メタデータの構造データ</param>
        /// <param name="tag">取得したいExifのタグ番号</param>
        /// <returns></returns>
        private static string GetExifDataFromMetadata<T>(IEnumerable<T> metadataDirectories, int tag)
            where T : MetadataExtractor.Directory
        {
            foreach (var metadataDirectory in metadataDirectories)
            {
                if (metadataDirectory is MetadataExtractor.Directory exifDirectory)
                {
                    // 該当データが見つかった場合は、すぐにその値を返す
                    var metadata = exifDirectory.GetString(tag);
                    if (!string.IsNullOrEmpty(metadata))
                    {
                        return metadata;
                    }
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