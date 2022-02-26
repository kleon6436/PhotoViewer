using System.Collections.Generic;
using System.Linq;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// サポート対象のファイル拡張子のタイプ定義
    /// </summary>
    public enum FileExtensionType
    {
        Jpeg,
        Bmp,
        Png,
        Tiff,
        Gif,
        Dng,
        Nef,
        Unknown
    }

    /// <summary>
    /// メディアファイルの確認クラス
    /// </summary>
    public static class MediaChecker
    {
        private static readonly Dictionary<string, FileExtensionType> SupportExtensionMap = new()
        {
            { ".jpg", FileExtensionType.Jpeg },
            { ".bmp", FileExtensionType.Bmp },
            { ".png", FileExtensionType.Png },
            { ".tiff", FileExtensionType.Tiff },
            { ".tif", FileExtensionType.Tiff },
            { ".gif", FileExtensionType.Gif },
            { ".dng", FileExtensionType.Dng },
            { ".nef", FileExtensionType.Nef }
        };

        /// <summary>
        /// サポートする画像の拡張子名
        /// </summary>
        private static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };

        /// <summary>
        /// サポートするRaw画像の拡張子名
        /// </summary>
        private static readonly string[] SupportRawPictureExtensions = { ".dng", ".nef" };

        /// <summary>
        /// サポートする画像の拡張子名リストを取得する
        /// </summary>
        /// <returns>サポートする画像の拡張子名リスト</returns>
        public static IEnumerable<string> GetSupportExtentions()
        {
            return SupportPictureExtensions;
        }

        /// <summary>
        /// サポートする画像の拡張子であるか確認する
        /// </summary>
        /// <param name="extension">確認する拡張子名</param>
        /// <returns>True: サポートする拡張子である, False: サポートしない拡張子である</returns>
        public static bool CheckPictureExtensions(string extension)
        {
            return SupportPictureExtensions.Any(supportExtension => supportExtension == extension);
        }

        /// <summary>
        /// サポートするRaw画像の拡張子であるか確認する
        /// </summary>
        /// <param name="extension">確認する拡張子名</param>
        /// <returns>True: サポートする拡張子である, False: サポートしない拡張子である</returns>
        public static bool CheckRawFileExtension(string extension)
        {
            return SupportRawPictureExtensions.Any(x => x == extension);
        }

        /// <summary>
        /// ファイルパスからサポート対象のファイル拡張子のタイプ定義を返す
        /// </summary>
        /// <param name="extension">タイプ定義を取得したい拡張子名</param>
        /// <returns>サポート対象のファイル拡張子のタイプ定義</returns>
        public static FileExtensionType GetFileExtensionType(string extension)
        {
            if (!CheckPictureExtensions(extension))
            {
                return FileExtensionType.Unknown;
            }

            return !SupportExtensionMap.TryGetValue(extension, out var extensionType) ? FileExtensionType.Unknown : extensionType;
        }
    }
}