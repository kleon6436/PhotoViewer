using System.Linq;
using System.Collections.Generic;

namespace PhotoViewer.Model
{
    public static class MediaChecker
    {
        private static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };

        /// <summary>
        /// アプリがサポートする拡張子リストを取得する
        /// </summary>
        /// <returns>サポートする拡張子リスト</returns>
        public static List<string> GetSupportExtentions()
        {
            return SupportPictureExtensions.ToList();
        }

        /// <summary>
        /// 静止画のサポートする拡張子であるか確認する
        /// </summary>
        /// <param name="_extension">確認する拡張子</param>
        /// <returns>サポートする拡張子の場合はTrue, ない場合はFalseを返す</returns>
        public static bool CheckPictureExtensions(string extension)
        {
            return SupportPictureExtensions.Any(supportExtension => supportExtension == extension);
        }

        /// <summary>
        /// ファイルがNikonRawFile(NEFファイル)であるか確認する
        /// </summary>
        /// <param name="extension">確認するファイルの拡張子</param>
        /// <returns>NEFファイルの場合: True、それ以外のファイルの場合: False</returns>
        public static bool CheckNikonRawFileExtension(string extension)
        {
            return (extension == ".nef") ? true : false;
        }
    }
}
