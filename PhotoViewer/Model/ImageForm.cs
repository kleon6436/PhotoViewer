namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// 編集画面に表示する画像保存形式クラス
    /// </summary>
    public sealed class ImageForm
    {
        /// <summary>
        /// 画像保存形式
        /// </summary>
        public enum ImageForms
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
        }

        /// <summary>
        /// 画像保存形式名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 画像保存形式
        /// </summary>
        public ImageForms Form { get; init; }
    }
}