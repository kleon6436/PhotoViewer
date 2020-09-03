using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ExtraAppSetting : BindableBase
    {
        private string appName;

        public string AppName
        {
            get { return appName; }
            set { SetProperty(ref appName, value); }
        }

        private string appPath;

        public string AppPath
        {
            get { return appPath; }
            set { SetProperty(ref appPath, value); }
        }
    }
}