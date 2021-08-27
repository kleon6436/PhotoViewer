namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// 編集画面に表示するリサイズカテゴリ情報クラス
    /// </summary>
    public sealed class ResizeImageCategory
    {
        /// <summary>
        /// リサイズカテゴリ
        /// </summary>
        public enum ResizeCategory
        {
            None,
            Print,
            Blog,
            Twitter,
        }

        /// <summary>
        /// リサイズカテゴリ名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// リサイズカテゴリ
        /// </summary>
        public ResizeCategory Category { get; init; }

        /// <summary>
        /// リサイズする長辺の長さ
        /// </summary>
        public int ResizeLongSideValue { get; init; }
    }
}