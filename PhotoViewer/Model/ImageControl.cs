using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public static class ImageControl
    {
        /// <summary>
        /// Get an image from the file path (image that reflects only rotation information without changing width and height).
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource DecodePicture(string filePath)
        {
            using var sourceStream = new WrappingStream(new FileStream(filePath, FileMode.Open));

            // Get image data.
            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);
            var metaData = bitmapFrame.Metadata as BitmapMetadata;
            var bitmapSource = bitmapFrame.Clone();

            var decodeBitmapSource = new WriteableBitmap(ImageControl.RotateImage(metaData, bitmapSource));
            decodeBitmapSource.Freeze();

            return decodeBitmapSource;
        }

        /// <summary>
        /// Generate an image to be magnified.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            using var ms = new WrappingStream(new FileStream(filePath, FileMode.Open));

            // Reset the stream position and first get metadata.
            ms.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(ms);
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // Reset stream position and decode image.
            ms.Seek(0, SeekOrigin.Begin);

            var maxViewWidth = 2200;
            var maxViewHeight = 1650;

            // Check the rotation information and in the case of vertical position images, swap the maximum vertical and horizontal sizes.
            var rotation = GetRotation(metaData);
            if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
            {
                var tmp = maxViewWidth;
                maxViewWidth = maxViewHeight;
                maxViewHeight = tmp;
            }

            var bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            bmpImage.DecodePixelWidth = maxViewWidth;
            bmpImage.StreamSource = ms;
            bmpImage.EndInit();

            // If the image is large, reduce the image.
            BitmapSource viewImage = RotateImage(metaData, (BitmapSource)bmpImage);
            if (bmpImage.PixelWidth > maxViewWidth || bmpImage.PixelHeight > maxViewHeight)
            {
                viewImage = ResizeImage(bmpImage, maxViewWidth, maxViewHeight);
            }

            // Export image and make it unchangeable.
            viewImage = new WriteableBitmap(viewImage);
            viewImage.Freeze();

            return viewImage;
        }

        /// <summary>
        /// Creating thumbnail images of still images.
        /// </summary>
        /// <param name="filePath">File`ath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            using var ms = new WrappingStream(new FileStream(filePath, FileMode.Open));

            // Reset the stream position and get the metadata.
            ms.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(ms);
            var metaData = bitmapFrame.Metadata as BitmapMetadata;
            var thumbnailImage = bitmapFrame.Thumbnail;

            var maxScaledWidth = 100;
            var maxScaledHeight = 75;

            // Check the rotation information, and in the case of vertical position images, swap the maximum vertical and horizontal sizes.
            var rotation = GetRotation(metaData);
            if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
            {
                var tmp = maxScaledWidth;
                maxScaledWidth = maxScaledHeight;
                maxScaledHeight = tmp;
            }

            if (thumbnailImage == null)
            {
                // Reset stream position and decode image.
                ms.Seek(0, SeekOrigin.Begin);

                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.DecodePixelWidth = maxScaledWidth;
                bmpImage.StreamSource = ms;
                bmpImage.EndInit();
            }
            else
            {
                // If the thumbnail image is large, reduce the image.
                if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
                {
                    thumbnailImage = ResizeImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
                }
            }

            // Rotate for thumbnail images.
            thumbnailImage = RotateImage(metaData, thumbnailImage);

            // Export image and make it unchangeable.
            thumbnailImage = new WriteableBitmap(thumbnailImage);
            thumbnailImage.Freeze();

            return thumbnailImage;
        }

        /// <summary>
        /// Create thumbnail image for still image edit screen.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureEditViewThumbnail(string filePath)
        {
            using var ms = new WrappingStream(new FileStream(filePath, FileMode.Open));

            // Reset the stream position and get the metadata.
            ms.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(ms);
            var metaData = bitmapFrame.Metadata as BitmapMetadata;
            var thumbnailImage = bitmapFrame.Thumbnail;

            var maxScaledWidth = 400;
            var maxScaledHeight = 300;

            // Check the rotation information, and in the case of vertical position images, swap the maximum vertical and horizontal sizes.
            var rotation = GetRotation(metaData);
            if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
            {
                var tmp = maxScaledWidth;
                maxScaledWidth = maxScaledHeight;
                maxScaledHeight = tmp;
            }

            if (thumbnailImage == null)
            {
                // Reset stream position and decode image.
                ms.Seek(0, SeekOrigin.Begin);

                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.DecodePixelWidth = maxScaledWidth;
                bmpImage.StreamSource = ms;
                bmpImage.EndInit();
            }
            else
            {
                // If the thumbnail image is large, reduce the image.
                if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
                {
                    thumbnailImage = ResizeImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
                }
            }

            // Rotate for thumbnail images.
            thumbnailImage = RotateImage(metaData, thumbnailImage);

            // Export image and make it unchangeable.
            thumbnailImage = new WriteableBitmap(thumbnailImage);
            thumbnailImage.Freeze();

            return thumbnailImage;
        }

        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <param name="image">BitmapSource</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource RotateImage(BitmapMetadata metaData, BitmapSource image)
        {
            var rotation = GetRotation(metaData);

            return rotation switch
            {
                1 => image,
                3 => TransformBitmap(image, new RotateTransform(180)),
                6 => TransformBitmap(image, new RotateTransform(90)),
                8 => TransformBitmap(image, new RotateTransform(270)),
                2 => TransformBitmap(image, new ScaleTransform(-1, 1, 0, 0)),
                4 => TransformBitmap(image, new ScaleTransform(1, -1, 0, 0)),
                5 => TransformBitmap(TransformBitmap(image, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0)),
                7 => TransformBitmap(TransformBitmap(image, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0)),
                _ => image,
            };
        }

        /// <summary>
        /// Resize image to specified size.
        /// </summary>
        /// <param name="image">BitmapSource.</param>
        /// <param name="maxScaledWidth">Width</param>
        /// <param name="maxScaledHeight">Height</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource ResizeImage(BitmapSource image, int maxScaledWidth, int maxScaledHeight)
        {
            // Generate a scaled image.
            double scaleX = (double)maxScaledWidth / image.PixelWidth;
            double scaleY = (double)maxScaledHeight / image.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);

            // Generate WritableBitmap from generated TransformedBitmap.
            return new TransformedBitmap(image, new ScaleTransform(scale, scale));
        }

        /// <summary>
        /// Get image rotation information from image metadata.
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <returns>rotation value</returns>
        private static uint GetRotation(BitmapMetadata metaData)
        {
            string _query = "/app1/ifd/exif:{uint=274}";
            if (!metaData.ContainsQuery(_query))
            {
                return 0;
            }

            return Convert.ToUInt32(metaData.GetQuery(_query));
        }

        /// <summary>
        /// Rotate the image.
        /// </summary>
        private static BitmapSource TransformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            return result;
        }
    }
}