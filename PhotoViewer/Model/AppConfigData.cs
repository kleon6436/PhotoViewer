using System.Collections.Generic;
using Kchary.PhotoViewer.Views;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// アプリケーションデータクラス
    /// </summary>
    public sealed record AppConfigData
    {
        /// <summary>
        /// 前回のフォルダパス
        /// </summary>
        public string PreviousFolderPath { get; set; }

        /// <summary>
        /// 登録アプリリスト
        /// </summary>
        /// <remarks>
        /// 登録アプリの一覧表示に使用される
        /// </remarks>
        public List<ExtraAppSetting> LinkageAppList { get; set; } = new();

        /// <summary>
        /// Windowの設定情報
        /// </summary>
        /// <remarks>
        /// 位置、表示状態などが保持されている
        /// </remarks>
        public MainWindow.NativeMethods.Placement PlaceData;
    }
}