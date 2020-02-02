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

                const int maxScaledWidth = 100;
                const int maxScaledHeight = 75;
                if (thumbnailImage == null)
                {
                    // ストリーム位置をリセットし、画像をデコード
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapDecoder decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                    // 画像の縮小処理
                    thumbnailImage = ResizeThumbnailImage(decoder.Frames[0], maxScaledWidth, maxScaledHeight);
                }
                else
                {
                    // サムネイル画像が大きい場合は、画像の縮小処理
                    if (thumbnailImage.PixelWidth > maxScaledWidth || thumbnailImage.PixelHeight > maxScaledHeight)
                    {
                        thumbnailImage = ResizeThumbnailImage(thumbnailImage, maxScaledWidth, maxScaledHeight);
                    }
                }

                // サムネイル画像向けに回転
                thumbnailImage = RotateThumbnailImage(metaData, thumbnailImage);
                thumbnailImage = new WriteableBitmap(thumbnailImage);
                thumbnailImage.Freeze();

                return thumbnailImage;
            }
        }

        private static BitmapSource ResizeThumbnailImage(BitmapSource thumbnailImage, int maxScaledWidth, int maxScaledHeight)
        {
            // 拡大/縮小したイメージを生成する
            double scaleX = (double)maxScaledWidth / thumbnailImage.PixelWidth;
            double scaleY = (double)maxScaledHeight / thumbnailImage.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);

            // 生成したTransformedBitmapから再度WritableBitmapを生成する
            return new TransformedBitmap(thumbnailImage, new ScaleTransform(scale, scale));
        }

        private static BitmapSource RotateThumbnailImage(BitmapMetadata metaData, BitmapSource thumbnailSource)
        {
            uint rotation = GetRotation(metaData);

            switch (rotation)
            {
                case 1:
                    return thumbnailSource;

                case 3:
                    return TransformBitmap(thumbnailSource, new RotateTransform(180));

                case 6:
                    return TransformBitmap(thumbnailSource, new RotateTransform(90));

                case 8:
                    return TransformBitmap(thumbnailSource, new RotateTransform(270));
                case 2:
                    return TransformBitmap(thumbnailSource, new ScaleTransform(-1, 1, 0, 0));

                case 4:
                    return TransformBitmap(thumbnailSource, new ScaleTransform(1, -1, 0, 0));

                case 5:
                    return TransformBitmap(TransformBitmap(thumbnailSource, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0));

                case 7:
                    return TransformBitmap(TransformBitmap(thumbnailSource, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0));

                default:
                    return thumbnailSource;
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
