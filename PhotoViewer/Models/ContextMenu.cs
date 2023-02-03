using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// コンテキストメニュークラス
    /// </summary>
    public sealed record ContextMenu
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// アイコン
        /// </summary>
        public BitmapSource ContextIcon { get; init; }
    }
}