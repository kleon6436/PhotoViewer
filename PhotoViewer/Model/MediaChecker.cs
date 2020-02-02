using System.Collections.Generic;
using System.Linq;

namespace PhotoViewer.Model
{
    public static class MediaChecker
    {
        private static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif"};

        /// <summary>
        /// アプリがサポートする拡張子リストを取得
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
    }
}
