using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public sealed class MediaInfo : BindableBase
    {
        /// <summary>
        /// Media type
        /// </summary>
        public enum MediaType
        {
            PICTURE,
            MOVIE,
        }

        #region Media Parameters

        /// <summary>
        /// Media type
        /// </summary>
        public MediaType ContentMediaType => CheckMediaType(FilePath);

        private BitmapSource thumbnailImage;

        /// <summary>
        /// Image of thumbnail
        /// </summary>
        public BitmapSource ThumbnailImage
        {
            get => thumbnailImage;
            set => SetProperty(ref thumbnailImage, value);
        }

        private string fileName;

        /// <summary>
        /// File name
        /// </summary>
        public string FileName
        {
            get => fileName;
            set => SetProperty(ref fileName, value);
        }

        /// <summary>
        /// File path
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        /// <summary>
        /// Create thumbnail image.
        /// </summary>
        /// <returns>True: Success、False: Failure</returns>
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
                    MediaType.PICTURE => ImageController.CreatePictureThumbnailImage(FilePath),
                    MediaType.MOVIE => throw new ArgumentOutOfRangeException(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the type of media file.
        /// </summary>
        /// <param name="_filePath">File path to check</param>
        /// <returns>File type</returns>
        private static MediaType CheckMediaType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

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