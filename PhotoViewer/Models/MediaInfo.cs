using Kchary.PhotoViewer.Helper;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// メディア情報クラス
    /// </summary>
    public sealed record MediaInfo
    {
        #region Media Parameters

        /// <summary>
        /// サムネイル画像
        /// </summary>
        public BitmapSource ThumbnailImage { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        /// <summary>
        /// サムネイル画像を作成する
        /// </summary>
        /// <returns>True: 成功、False: 失敗</returns>
        public bool CreateThumbnailImage()
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                {
                    return false;
                }

                ThumbnailImage = ImageController.CreatePictureThumbnailImage(FilePath);
                return true;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                return false;
            }
        }
    }
}