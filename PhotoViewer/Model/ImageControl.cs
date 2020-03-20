using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public static class ImageControl
    {
        /// <summary>
        /// 拡大表示する画像を生成する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>BitmapSource(生成した画像)</returns>
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            using (var ms = new WrappingStream(new FileStream(filePath, FileMode.Open)))
            {
                // ストリーム位置をリセットし、まずはメタデータの取得
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapFrame = BitmapFrame.Create(ms);
                var metaData = bitmapFrame.Metadata as BitmapMetadata;

                // ストリーム位置をリセットし、画像をデコード
                ms.Seek(0, SeekOrigin.Begin);

                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.DecodePixelWidth = 880;
                bmpImage.StreamSource = ms;
                bmpImage.EndInit();

                BitmapSource viewImage = (BitmapSource)bmpImage;

                int maxViewWidth = 880;
                int maxViewHeight = 660;

                // 回転情報を確認し、縦位置画像の場合は、縦横の最大サイズを入れ替えておく
                uint rotation = GetRotation(metaData);
                if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
                {
                    int tmp = maxViewWidth;
                    maxViewWidth = maxViewHeight;
                    maxViewHeight = tmp;
                }

                // リサイズ後に回転する
                if (viewImage.PixelWidth > maxViewWidth || viewImage.PixelHeight > maxViewHeight)
                {
                    viewImage = ResizeImage(viewImage, maxViewWidth, maxViewHeight);
                }
                viewImage = RotateImage(metaData, viewImage);

                // 画像を書き出し、変更不可にする
                viewImage = new WriteableBitmap(viewImage);
                viewImage.Freeze();

                return viewImage;
            }
        }

        /// <summary>
        /// 静止画のサムネイル画像を作成する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>BitmapSource(生成したサムネイル画像)</returns>
        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            using (var ms = new WrappingStream(new FileStream(filePath, FileMode.Open)))
            {
                // ストリーム位置をリセットし、まずはメタデータを取得
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapFrame = BitmapFrame.Create(ms);
                var metaData = bitmapFrame.Metadata as BitmapMetadata;
                var thumbnailImage = bitmapFrame.Thumbnail;

                int maxScaledWidth = 100;
                int maxScaledHeight = 75;

                // 回転情報を確認し、縦位置画像の場合は、縦横の最大サイズを入れ替えておく
                uint rotation = GetRotation(metaData);
                if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
                {
                    int tmp = maxScaledWidth;
                    maxScaledWidth = maxScaledHeight;
                    maxScaledHeight = tmp;
                }

                if (thumbnailImage == null)
                {
                    // ストリーム位置をリセットし、画像をデコード
                    ms.Seek(0, SeekOrigin.Begin);

                    var bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImage.DecodePixelWidth = 100;
                    bmpImage.StreamSource = ms;
                    bmpImage.EndInit();

                    // 画像の縮小処理
                    thumbnailImage = ResizeImage(bmpImage, maxScaledWidth, maxScaledHeight);
                }
                else
                {
                    // サムネイル画像が大きい場合は、画像の縮小処理
                    if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
                    {
                        thumbnailImage = ResizeImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
                    }
                }

                // サムネイル画像向けに回転
                thumbnailImage = RotateImage(metaData, thumbnailImage);

                // 画像を書き出し、変更不可にする
                thumbnailImage = new WriteableBitmap(thumbnailImage);
                thumbnailImage.Freeze();

                return thumbnailImage;
            }
        }

        /// <summary>
        /// 静止画編集画面用のサムネイル画像を作成する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>BitmapSource(生成したサムネイル画像)</returns>
        public static BitmapSource CreatePictureEditViewThumbnail(string filePath)
        {
            using (var ms = new WrappingStream(new FileStream(filePath, FileMode.Open)))
            {
                // ストリーム位置をリセットし、まずはメタデータを取得
                ms.Seek(0, SeekOrigin.Begin);
                var bitmapFrame = BitmapFrame.Create(ms);
                var metaData = bitmapFrame.Metadata as BitmapMetadata;
                var thumbnailImage = bitmapFrame.Thumbnail;

                int maxScaledWidth = 400;
                int maxScaledHeight = 300;

                // 回転情報を確認し、縦位置画像の場合は、縦横の最大サイズを入れ替えておく
                uint rotation = GetRotation(metaData);
                if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
                {
                    int tmp = maxScaledWidth;
                    maxScaledWidth = maxScaledHeight;
                    maxScaledHeight = tmp;
                }

                if (thumbnailImage == null)
                {
                    // ストリーム位置をリセットし、画像をデコード
                    ms.Seek(0, SeekOrigin.Begin);

                    var bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImage.StreamSource = ms;
                    bmpImage.EndInit();

                    // 画像の縮小処理
                    thumbnailImage = ResizeImage(bmpImage, maxScaledWidth, maxScaledHeight);
                }
                else
                {
                    // サムネイル画像が大きい場合は、画像の縮小処理
                    if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
                    {
                        thumbnailImage = ResizeImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
                    }
                }

                // サムネイル画像向けに回転
                thumbnailImage = RotateImage(metaData, thumbnailImage);

                // 画像を書き出し、変更不可にする
                thumbnailImage = new WriteableBitmap(thumbnailImage);
                thumbnailImage.Freeze();

                return thumbnailImage;
            }
        }

        /// <summary>
        /// 画像を指定サイズにリサイズする
        /// </summary>
        /// <param name="image">BitmapSource(画像データ)</param>
        /// <param name="maxScaledWidth">指定幅</param>
        /// <param name="maxScaledHeight">指定高さ</param>
        /// <returns>BitmapSource(リサイズ後の画像)</returns>
        private static BitmapSource ResizeImage(BitmapSource image, int maxScaledWidth, int maxScaledHeight)
        {
            // 拡大/縮小したイメージを生成する
            double scaleX = (double)maxScaledWidth / image.PixelWidth;
            double scaleY = (double)maxScaledHeight / image.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);

            // 生成したTransformedBitmapから再度WritableBitmapを生成する
            return new TransformedBitmap(image, new ScaleTransform(scale, scale));
        }

        /// <summary>
        /// 画像を回転する
        /// </summary>
        /// <param name="metaData">メタデータ</param>
        /// <param name="image">BitmapSource(画像データ)</param>
        /// <returns>BitmapSource(回転後の画像)</returns>
        private static BitmapSource RotateImage(BitmapMetadata metaData, BitmapSource image)
        {
            uint rotation = GetRotation(metaData);

            switch (rotation)
            {
                case 1:
                    return image;

                case 3:
                    return TransformBitmap(image, new RotateTransform(180));

                case 6:
                    return TransformBitmap(image, new RotateTransform(90));

                case 8:
                    return TransformBitmap(image, new RotateTransform(270));
                case 2:
                    return TransformBitmap(image, new ScaleTransform(-1, 1, 0, 0));

                case 4:
                    return TransformBitmap(image, new ScaleTransform(1, -1, 0, 0));

                case 5:
                    return TransformBitmap(TransformBitmap(image, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0));

                case 7:
                    return TransformBitmap(TransformBitmap(image, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0));

                default:
                    return image;
            }
        }

        /// <summary>
        /// 画像のメタデータから画像の回転情報を取得する
        /// </summary>
        /// <param name="metaData">メタデータ</param>
        /// <returns>rotation値</returns>
        private static uint GetRotation(BitmapMetadata metaData)
        {
            string _query = "/app1/ifd/exif:{uint=274}";
            if (!metaData.ContainsQuery(_query))
            {
                return 0;   // エラーとして返す
            }

            return Convert.ToUInt32(metaData.GetQuery(_query));
        }

        /// <summary>
        /// 画像を回転するメソッド
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
