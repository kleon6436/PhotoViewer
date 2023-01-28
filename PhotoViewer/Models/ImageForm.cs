namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 編集画面に表示する画像保存形式クラス
    /// </summary>
    public sealed record ImageForm
    {
        /// <summary>
        /// 画像保存形式名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 画像保存形式
        /// </summary>
        public FileExtensionType Form { get; init; }
    }
}