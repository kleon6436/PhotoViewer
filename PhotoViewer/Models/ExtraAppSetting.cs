namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 登録アプリ情報クラス
    /// </summary>
    public sealed record ExtraAppSetting
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