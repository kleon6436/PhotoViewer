using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private string filePath;
        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }

        private BitmapImage thumbnailImage;
        /// <summary>
        /// サムネイルイメージ
        /// </summary>
        public BitmapImage ThumbnailImage
        {
            get { return thumbnailImage; }
            set { SetProperty(ref thumbnailImage, value); }
        }
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
        public bool CreateThumbnailImage()
        {
            if (this.FilePath == null || !File.Exists(this.FilePath))
            {
                return false;
            }

            switch (this.ContentMediaType)
            {
                case MediaType.PICTURE:
                    //this.ThumbnailImage =
                    return true;

                case MediaType.MOVIE:
                default:
                    return false;
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
