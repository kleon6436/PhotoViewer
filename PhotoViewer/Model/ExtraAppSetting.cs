using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ExtraAppSetting : BindableBase
    {
        private string appName;

        /// <summary>
        /// Application name
        /// </summary>
        public string AppName
        {
            get => appName;
            set => SetProperty(ref appName, value);
        }

        private string appPath;

        /// <summary>
        /// Application path
        /// </summary>
        public string AppPath
        {
            get => appPath;
            set => SetProperty(ref appPath, value);
        }
    }
}