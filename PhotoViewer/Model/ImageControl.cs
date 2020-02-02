using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public static class ImageControl
    {
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            using (var ms = new MemoryStream())
            {
                // 読み込みファイルをメモリにコピーする
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    fs.CopyTo(ms);
                }

                // ストリーム位置をリセットし、まずはメタデータの取得
                ms.Seek(0, SeekOrigin.Begin);
                var metaData = BitmapFrame.Create(ms).Metadata as BitmapMetadata;

                // ストリーム位置をリセットし、画像をでコード
                ms.Seek(0, SeekOrigin.Begin);
                BitmapDecoder decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

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

                BitmapSource viewImage = decoder.Frames[0];

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

        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            using (var ms = new MemoryStream())
            {
                // 読み込みファイルをメモリにコピーする
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    fs.CopyTo(ms);
                }

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
                    BitmapDecoder decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                    // 画像の縮小処理
                    thumbnailImage = ResizeImage(decoder.Frames[0], maxScaledWidth, maxScaledHeight);
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

        private static BitmapSource ResizeImage(BitmapSource image, int maxScaledWidth, int maxScaledHeight)
        {
            // 拡大/縮小したイメージを生成する
            double scaleX = (double)maxScaledWidth / image.PixelWidth;
            double scaleY = (double)maxScaledHeight / image.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);

            // 生成したTransformedBitmapから再度WritableBitmapを生成する
            return new TransformedBitmap(image, new ScaleTransform(scale, scale));
        }

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
