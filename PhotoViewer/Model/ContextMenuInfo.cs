using Prism.Mvvm;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ContextMenuInfo : BindableBase
    {
        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { SetProperty(ref displayName, value); }
        }

        private BitmapSource contextIcon;

        public BitmapSource ContextIcon
        {
            get { return contextIcon; }
            set { SetProperty(ref contextIcon, value); }
        }
    }
}