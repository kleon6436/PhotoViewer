using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// コンテキストメニュークラス
    /// </summary>
    public sealed class ContextMenuInfo : BindableBase
    {
        private string displayName;
        private BitmapSource contextIcon;

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        /// <summary>
        /// アイコン
        /// </summary>
        public BitmapSource ContextIcon
        {
            get => contextIcon;
            set => SetProperty(ref contextIcon, value);
        }
    }
}