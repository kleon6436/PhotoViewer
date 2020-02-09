using System;
using System.IO;
using System.Xml.Linq;

namespace PhotoViewer.Model
{
    public sealed class AppConfigManager
    {
        private readonly string AppConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\KcharyPhotographViewer\Setting.conf";

        // シングルトンクラス
        private static AppConfigManager singleInstance = new AppConfigManager();
        // 登録されている連携アプリ
        public ExtraAppSetting LinkageApp { get; private set; }

        public void SetLinkageApp(ExtraAppSetting linkageApp)
        {
            LinkageApp = linkageApp;
        }

        public void RemoveLinkageApp()
        {
            LinkageApp = null;
        }
        
        public void Export()
        {
            // フォルダが存在しない場合は作成
            string appConfigdirectory = Path.GetDirectoryName(AppConfigFilePath);
            if (!Directory.Exists(appConfigdirectory))
            {
                Directory.CreateDirectory(appConfigdirectory);
            }

            // XML文章の作成
            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null));

            var linkageAppXml = CreateLinkageAppXml();
            xdoc.Add(linkageAppXml);

            xdoc.Save(AppConfigFilePath);
        }

        public void Import()
        {
            if (!File.Exists(AppConfigFilePath))
            {
                return;
            }

            try
            {
                var xdoc = XDocument.Load(AppConfigFilePath);

                ParseLinkageAppXml(xdoc);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                throw new IOException();
            }
        }

        public void ImportLinkageAppXml()
        {
            if (!File.Exists(AppConfigFilePath))
            {
                return;
            }

            try
            {
                var xdoc = XDocument.Load(AppConfigFilePath);
                ParseLinkageAppXml(xdoc);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                throw new IOException();
            }
        }

        /// <summary>
        /// 連携アプリのXMLを生成する
        /// </summary>
        private XElement CreateLinkageAppXml()
        {
            var dataElement = new XElement("linkageAppData");
            var appIdElement = new XElement("id", LinkageApp == null ? new XText("") : new XText(LinkageApp.AppId.ToString()));
            var appNameElement = new XElement("name", LinkageApp == null ? new XText("") : new XText(LinkageApp.AppName));
            var appPathElement = new XElement("path", LinkageApp == null ? new XText("") : new XText(LinkageApp.AppPath));

            dataElement.Add(appIdElement);
            dataElement.Add(appNameElement);
            dataElement.Add(appPathElement);

            return dataElement;
        }

        /// <summary>
        /// 連携アプリのXMLを解析する
        /// </summary>
        /// <param name="xdoc">XDocument</param>
        private void ParseLinkageAppXml(XDocument xdoc)
        {
            var dataElement = xdoc.Element("linkageAppData");
            XElement appIdElement = dataElement.Element("id");
            XElement appNameElement = dataElement.Element("name");
            XElement appPathElement = dataElement.Element("path");

            if (string.IsNullOrEmpty(appIdElement.Value) || string.IsNullOrEmpty(appNameElement.Value) || string.IsNullOrEmpty(appPathElement.Value))
            {
                LinkageApp = null;
            }
            else
            {
                var linkageApp = new ExtraAppSetting(Convert.ToInt32(appIdElement.Value), appNameElement.Value, appPathElement.Value);
                LinkageApp = linkageApp;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private AppConfigManager()
        {
            LinkageApp = null;
        }

        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <returns><AppConfigManager</returns>
        public static AppConfigManager GetInstance()
        {
            return singleInstance;
        }
    }
}
