using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// 登録アプリ情報クラス
    /// </summary>
    public sealed class ExtraAppSetting : BindableBase
    {
        private string appName;
        private string appPath;

        /// <summary>
        /// アプリケーション名
        /// </summary>
        public string AppName
        {
            get => appName;
            set => SetProperty(ref appName, value);
        }

        /// <summary>
        /// アプリケーションの絶対パス
        /// </summary>
        public string AppPath
        {
            get => appPath;
            set => SetProperty(ref appPath, value);
        }
    }
}