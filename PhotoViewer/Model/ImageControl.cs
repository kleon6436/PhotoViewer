using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public static class ImageControl
    {
        /// <summary>
        /// Generate an image to be magnified.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            const int maxViewWidth = 2200;
            const int maxViewHeight = 1650;

            return DecodePicture(bitmapFrame, maxViewWidth, maxViewHeight);
        }

        /// <summary>
        /// Creating thumbnail images of still images.
        /// </summary>
        /// <param name="filePath">File`ath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Reset the stream position and get the metadata.
            fs.Seek(0, SeekOrigin.Begin);

            var bitmapFrame = BitmapFrame.Create(fs);
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
                thumbnailImage = bitmapFrame.Clone();
            }

            // If the thumbnail image is large, reduce the image.
            if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
            {
                thumbnailImage = ResizeImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
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
        public static BitmapSource CreatePictureEditViewThumbnail(string filePath, out int defaultPictureWidth, out int defaultPictureHeight, out uint rotation)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            defaultPictureWidth = bitmapFrame.PixelWidth;
            defaultPictureHeight = bitmapFrame.PixelHeight;
            rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

            const int maxScaledWidth = 240;
            const int maxScaledHeight = 180;

            return DecodePicture(bitmapFrame, maxScaledWidth, maxScaledHeight);
        }

        /// <summary>
        /// Create save image.
        /// </summary>
        /// <param name="filePath"><FilePath/param>
        /// <param name="scale">scale</param>
        /// <returns>BitmapSOurce</returns>
        public static BitmapSource CreateSavePicture(string filePath, double scale)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            // Check the rotation information.
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // Decode picture.
            var saveImage = bitmapFrame.Clone();

            // Resize image.
            saveImage = new TransformedBitmap(saveImage, new ScaleTransform(scale, scale));

            // Rotate image.
            saveImage = RotateImage(metaData, saveImage);

            var decodeImage = new WriteableBitmap(saveImage);
            decodeImage.Freeze();

            return decodeImage;
        }

        /// <summary>
        /// Decode image from filePath.(If you set max width and max height, Image is resized.)
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <param name="maxWidth">Max picture width</param>
        /// <param name="maxHeight">Max picture height</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource DecodePicture(BitmapFrame bitmapFrame, int maxWidth, int maxHeight)
        {
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // Check the rotation information, and in the case of vertical position images, swap the maximum vertical and horizontal sizes.
            var rotation = GetRotation(metaData);
            if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
            {
                var tmp = maxWidth;
                maxWidth = maxHeight;
                maxHeight = tmp;
            }

            // Decode picture.
            var viewImage = bitmapFrame.Clone();

            //// If the image is large, reduce the image.
            if (viewImage.PixelWidth > maxWidth || viewImage.PixelHeight > maxHeight)
            {
                viewImage = ResizeImage(viewImage, maxWidth, maxHeight);
            }

            // Rotate image.
            viewImage = RotateImage(metaData, viewImage);

            var decodeImage = new WriteableBitmap(viewImage);
            decodeImage.Freeze();

            return decodeImage;
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