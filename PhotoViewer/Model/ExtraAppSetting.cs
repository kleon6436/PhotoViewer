using Prism.Mvvm;

namespace PhotoViewer.Model
{
    public class ExtraAppSetting : BindableBase
    {
        public int AppId { get; private set; }

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

        public ExtraAppSetting(int appId, string appName, string appPath)
        {
            this.AppId = appId;
            this.AppName = appName;
            this.AppPath = appPath;
        }
    }
}
