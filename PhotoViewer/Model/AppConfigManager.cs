using System;
using System.Collections.Generic;
using System.IO;

namespace PhotoViewer.Model
{
    /// <summary>
    /// アプリケーション設定情報の管理クラス
    /// </summary>
    public sealed class AppConfigManager
    {
        /// <summary>
        /// アプリケーション設定情報のデータ
        /// </summary>
        public AppConfigData ConfigData { get; set; }

        private readonly string appConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\KcharyPhotoViewer\Setting.conf";
        private static readonly AppConfigManager singleInstance = new AppConfigManager();

        /// <summary>
        /// アプリケーション設定情報クラスのインスタンス取得
        /// </summary>
        public static AppConfigManager GetInstance()
        {
            return singleInstance;
        }

        public void Import()
        {
            try
            {
                var configXmlObject = new AppConfigXml(appConfigFilePath);
                configXmlObject.Import(ConfigData);
            }
            catch (Exception ex)
            {
                // 読み込み時の例外は無視する
                App.LogException(ex);
            }
        }

        public void Export()
        {
            // フォルダが存在しない場合は作成
            string appConfigdirectory = Path.GetDirectoryName(appConfigFilePath);
            if (!Directory.Exists(appConfigdirectory))
            {
                Directory.CreateDirectory(appConfigdirectory);
            }

            try
            {
                var configXmlObject = new AppConfigXml(appConfigFilePath);
                configXmlObject.Export(ConfigData);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                throw;
            }
        }

        public void SetLinkageApp(List<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist;
        }

        public void RemoveLinkageApp(List<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist;
        }

        private AppConfigManager()
        {
            ConfigData = new AppConfigData();
        }
    }
}