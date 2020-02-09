using System;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace PhotoViewer.Model
{
    public class ContextMenuInfo : BindableBase
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

        public ContextMenuInfo(string displayName, BitmapSource contextIcon)
        {
            this.DisplayName = displayName;
            this.ContextIcon = contextIcon;
        }
    }
}
