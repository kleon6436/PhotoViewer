using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// メディア情報クラス
    /// </summary>
    public sealed class MediaInfo : BindableBase
    {
        /// <summary>
        /// メディアのタイプ
        /// </summary>
        public enum MediaType
        {
            Picture,
            Movie,
        }

        #region Media Parameters

        /// <summary>
        /// メディアタイプ
        /// </summary>
        public MediaType ContentMediaType => CheckMediaType(FilePath);

        private BitmapSource thumbnailImage;

        /// <summary>
        /// サムネイル画像
        /// </summary>
        public BitmapSource ThumbnailImage
        {
            get => thumbnailImage;
            set => SetProperty(ref thumbnailImage, value);
        }

        private string fileName;

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName
        {
            get => fileName;
            set => SetProperty(ref fileName, value);
        }

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

                ThumbnailImage = ContentMediaType switch
                {
                    MediaType.Picture => ImageController.CreatePictureThumbnailImage(FilePath),
                    MediaType.Movie => throw new ArgumentOutOfRangeException(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                return true;
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// ファイルのタイプを確認する
        /// </summary>
        /// <param name="filePath">確認するファイルパス</param>
        /// <returns>メディアタイプ</returns>
        private static MediaType CheckMediaType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            if (MediaChecker.CheckPictureExtensions(extension))
            {
                return MediaType.Picture;
            }

            throw new FileFormatException();
        }
    }
}