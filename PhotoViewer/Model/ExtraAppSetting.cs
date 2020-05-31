using Prism.Mvvm;

namespace PhotoViewer.Model
{
    public class ExtraAppSetting : BindableBase
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

        public ExtraAppSetting(string appName, string appPath)
        {
            this.AppName = appName;
            this.AppPath = appPath;
        }
    }
}
