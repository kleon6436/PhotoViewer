using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// 登録アプリ情報クラス
    /// </summary>
    public sealed class ExtraAppSetting
    {
        /// <summary>
        /// アプリケーション名
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// アプリケーションの絶対パス
        /// </summary>
        public string AppPath { get; set; }
    }
}