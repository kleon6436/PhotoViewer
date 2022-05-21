using System;
using System.IO;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// アプリケーション設定情報の管理クラス
    /// </summary>
    public sealed class AppConfigManager
    {
        /// <summary>
        /// アプリケーション情報インスタンス
        /// </summary>
        public AppConfigData ConfigData { get; private init; }

        /// <summary>
        /// アプリケーション設定ファイルの絶対パス
        /// </summary>
        private static readonly string AppConfigFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KcharyPhotoViewer\\Setting.conf";

        /// <summary>
        /// アプリケーション設定クラスのシングルトン
        /// </summary>
        private static readonly AppConfigManager SingleInstance = new() { ConfigData = new AppConfigData() };

        /// <summary>
        /// アプリケーション設定インスタンスを取得する
        /// </summary>
        public static AppConfigManager GetInstance()
        {
            return SingleInstance;
        }

        /// <summary>
        /// アプリケーション設定情報をXMLファイルからインポートする
        /// </summary>
        public void Import()
        {
            try
            {
                AppConfigXml.Import(AppConfigFilePath, ConfigData);
            }
            catch (Exception ex)
            {
                // 設定値読み込み時の例外はログに出力するのみ
                App.LogException(ex);
            }
        }

        /// <summary>
        /// アプリケーション設定情報を既定XMLファイルに出力する
        /// </summary>
        public void Export()
        {
            // 既定ディレクトリが存在しない場合は、ディレクトリも作成
            var appConfigDirectory = Path.GetDirectoryName(AppConfigFilePath);
            if (!Directory.Exists(appConfigDirectory))
            {
                Directory.CreateDirectory(appConfigDirectory ?? throw new InvalidOperationException());
            }

            try
            {
                AppConfigXml.Export(AppConfigFilePath, ConfigData);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
        }

        /// <summary>
        /// 連携アプリをリストに追加する
        /// </summary>
        /// <param name="linkageApp">登録する連携アプリ</param>
        public void AddLinkageApp(ExtraAppSetting linkageApp)
        {
            ConfigData.LinkageAppList.Add(linkageApp);
        }

        /// <summary>
        /// 連携アプリをリストから削除する
        /// </summary>
        /// <param name="linkageApp">削除する連携アプリ</param>
        public void RemoveLinkageApp(ExtraAppSetting linkageApp)
        {
            ConfigData.LinkageAppList.Remove(linkageApp);
        }
    }
}