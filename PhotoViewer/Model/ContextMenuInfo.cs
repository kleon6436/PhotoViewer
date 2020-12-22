using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ContextMenuInfo : BindableBase
    {
        private string displayName;

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        private BitmapSource contextIcon;

        /// <summary>
        /// Context icon
        /// </summary>
        public BitmapSource ContextIcon
        {
            get => contextIcon;
            set => SetProperty(ref contextIcon, value);
        }
    }
}