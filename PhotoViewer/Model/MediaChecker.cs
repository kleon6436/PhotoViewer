using System.Collections.Generic;
using System.Linq;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// メディアファイルの確認クラス
    /// </summary>
    public static class MediaChecker
    {
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
    }
}