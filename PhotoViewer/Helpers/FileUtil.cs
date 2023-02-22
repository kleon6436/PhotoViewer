using System.Collections.Generic;
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

        /// <summary>
        /// 指定されたパスがディレクトリであるか確認する
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>True: ディレクトリ、False: ディレクトリでない</returns>
        public static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// ファイルパスからファイル拡張子を取得する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイル拡張子</returns>
        public static string GetFileExtensions(string filePath)
        {
            return Path.GetExtension(filePath).ToLower();
        }

        /// <summary>
        /// ファイル名を取得する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="withoutExtension">拡張子の有無(True: 拡張子なし, False: 拡張子あり)</param>
        /// <returns>ファイル名</returns>
        public static string GetFileName(string filePath, bool withoutExtension)
        {
            return withoutExtension ? Path.GetFileNameWithoutExtension(filePath) : Path.GetFileName(filePath);
        }

        /// <summary>
        /// すべての親ディレクトリのパスリストを取得する
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <param name="parentPathList">親ディレクトリのパスリスト</param>
        public static void GetAllParentPathList(string folderPath, ICollection<string> parentPathList)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            parentPathList.Add(directoryInfo.FullName);
            GetParentPathList(folderPath, parentPathList);
        }

        /// <summary>
        /// 親ディレクトリのパスリストを取得する
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <param name="parentPathList">親ディレクトリのパスリスト</param>
        public static void GetParentPathList(string folderPath, ICollection<string> parentPathList)
        {
            while (true)
            {
                var parentDirectory = Directory.GetParent(folderPath);
                if (parentDirectory == null)
                {
                    return;
                }

                parentPathList.Add(parentDirectory.FullName);
                folderPath = parentDirectory.FullName;
            }
        }
    }
}
