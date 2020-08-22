using System.Collections.Generic;

namespace PhotoViewer.Model
{
    /// <summary>
    /// アプリケーション設定情報のデータクラス
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// 前回終了時点の表示フォルダ
        /// </summary>
        public string PreviousFolderPath { get; set; }

        /// <summary>
        /// 登録されている連携アプリ
        /// </summary>
        public List<ExtraAppSetting> LinkageAppList { get; set; }

        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public MainWindow.WINDOWPLACEMENT WindowPlaceData;

        public ConfigData()
        {
            PreviousFolderPath = null;
            LinkageAppList = new List<ExtraAppSetting>();
        }
    }
}