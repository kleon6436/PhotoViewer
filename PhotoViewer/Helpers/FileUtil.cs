using System.IO;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// ファイル、フォルダIOのユーティリティクラス
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// フォルダパスが有効であるか確認する
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <returns>True: 有効、False: 無効</returns>
        public static bool CheckFolderPath(string folderPath)
        {
            return !string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath);
        }

        /// <summary>
        /// ファイルパスが有効であるか確認する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>True: 有効、False: 無効</returns>
        public static bool CheckFilePath(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }
    }
}
