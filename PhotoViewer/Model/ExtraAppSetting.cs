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
        public string AppName { get; init; }

        /// <summary>
        /// アプリケーションの絶対パス
        /// </summary>
        public string AppPath { get; init; }
    }
}