using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Media.Imaging;

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
        /// Media type
        /// </summary>
        public MediaType ContentMediaType
        {
            get { return CheckMediaType(FilePath); }
        }

        private BitmapSource thumbnailImage;

        /// <summary>
        /// Image of thumbnail
        /// </summary>
        public BitmapSource ThumbnailImage
        {
            get { return thumbnailImage; }
            set { SetProperty(ref thumbnailImage, value); }
        }

        private string fileName;

        /// <summary>
        /// File name
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        /// <summary>
        /// File path
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        public MediaInfo()
        {
        }

        public MediaInfo(MediaInfo mediaFileInfo)
        {
            FilePath = mediaFileInfo.FilePath;
            FileName = Path.GetFileName(FilePath);
            ThumbnailImage = mediaFileInfo.ThumbnailImage;
        }

        /// <summary>
        /// Create thumbnail image.
        /// </summary>
        /// <returns>True: Success、False: Failure</returns>
        public void CreateThumbnailImage()
        {
            if (FilePath == null || !File.Exists(FilePath))
            {
                throw new FileLoadException();
            }

            switch (ContentMediaType)
            {
                case MediaType.PICTURE:
                    ThumbnailImage = ImageControl.CreatePictureThumbnailImage(FilePath);
                    return;

                case MediaType.MOVIE:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get the type of media file.
        /// </summary>
        /// <param name="_filePath">File path to check</param>
        /// <returns>File type</returns>
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
    }
}