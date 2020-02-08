using System;
using System.IO;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace PhotoViewer.Model
{
    public class MediaInfo : BindableBase
    {
        public enum MediaType
        {
            PICTURE,
            MOVIE,
        }

        #region Media Parameters
        /// <summary>
        /// メディアタイプ
        /// </summary>
        public MediaType ContentMediaType
        {
            get { return CheckMediaType(FilePath); }
        }

        private BitmapSource thumbnailImage;
        /// <summary>
        /// サムネイルイメージ
        /// </summary>
        public BitmapSource ThumbnailImage
        {
            get { return thumbnailImage; }
            set { SetProperty(ref thumbnailImage, value); }
        }

        private string fileName;
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath { get; set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MediaInfo()
        {

        }

        /// <summary>
        /// コピーコンストラクタ(各コンテンツのコンストラクタで呼ばれる)
        /// </summary>
        /// <param name="_mediaFileInfo">メディアファイルの情報</param>
        public MediaInfo(MediaInfo mediaFileInfo)
        {
            this.FilePath = mediaFileInfo.FilePath;
            this.FileName = Path.GetFileName(this.FilePath);
            this.ThumbnailImage = mediaFileInfo.ThumbnailImage;
        }

        /// <summary>
        /// メディアファイルのタイプを取得する
        /// </summary>
        /// <param name="_filePath">確認するファイルパス</param>
        /// <returns>ファイルのタイプ</returns>
        private MediaType CheckMediaType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (MediaChecker.CheckPictureExtensions(extension))
            {
                return MediaType.PICTURE;
            }
            else
            {
                throw new FileFormatException();
            }
        }

        /// <summary>
        /// サムネイル画像の生成
        /// </summary>
        /// <returns>サムネイル成功時: True、失敗時: False</returns>
        public void CreateThumbnailImage()
        {
            if (this.FilePath == null || !File.Exists(this.FilePath))
            {
                throw new FileLoadException();
            }

            switch (this.ContentMediaType)
            {
                case MediaType.PICTURE:
                    this.ThumbnailImage = ImageControl.CreatePictureThumbnailImage(this.FilePath);
                    return;

                case MediaType.MOVIE:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PictureMediaInfo : MediaInfo
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaInfo() : base(null)
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaInfo(MediaInfo _mediaFileInfo) : base(_mediaFileInfo)
        {

        }
    }
}
