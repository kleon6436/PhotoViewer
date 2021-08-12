using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// コンテキストメニュークラス
    /// </summary>
    public sealed class ContextMenuInfo
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// アイコン
        /// </summary>
        public BitmapSource ContextIcon { get; set; }
    }
}