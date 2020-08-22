using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace PhotoViewer.Model
{
    /// <summary>
    /// アプリケーション設定情報のXML管理クラス
    /// </summary>
    public class ConfigXml
    {
        // XMLの要素名
        private const string PREVIOUS_FOLDER_ELEM_NAME = "previous_folder";
        private const string PREVIOUS_PATH_ELEM_NAME = "previous_path";
        private const string LINK_APP_ELEM_NAME = "linkage_app";
        private const string LINK_APP_DATA_NAME = "linkage_app_data";
        private const string LINK_APP_NAME_ELEM_NAME = "name";
        private const string LINK_APP_PATH_ELEM_NAME = "path";
        private const string WINDOW_PLACEMENT_ELEM_NAME = "window_placement";
        private const string WINDOW_PLACEMENT_TOP_ELEM_NAME = "top";
        private const string WINDOW_PLACEMENT_LEFT_ELEM_NAME = "left";
        private const string WINDOW_PLACEMENT_RIGHT_ELEM_NAME = "right";
        private const string WINDOW_PLACEMENT_BUTTOM_ELEM_NAME = "buttom";
        private const string WINDOW_PLACEMENT_MAXPOSX_ELEM_NAME = "maxPosX";
        private const string WINDOW_PLACEMENT_MAXPOSY_ELEM_NAME = "maxPosY";
        private const string WINDOW_PLACEMENT_MINPOSX_ELEM_NAME = "minPosX";
        private const string WINDOW_PLACEMENT_MINPOSY_ELEM_NAME = "minPosY";
        private const string WINDOW_PLACEMENT_FLAG_ELEM_NAME = "windowFlag";
        private const string WINDOW_PLACEMENT_SW_ELEM_NAME = "sw";

        // アプリケーション設定情報のファイルパス
        private readonly string appConfigFilePath;

        public ConfigXml(string filePath)
        {
            appConfigFilePath = filePath;
        }

        public void Import(ConfigData configData)
        {
            var xdoc = XDocument.Load(appConfigFilePath);

            ParsePreviousPathXml(xdoc, configData);
            ParseLinkageAppXml(xdoc, configData);
            ParseWindowPlacementXml(xdoc, configData);
        }

        public void Export(ConfigData configData)
        {
            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var xdata = new XElement("data");

            xdata.Add(CreatePreviousPathXml(configData));
            xdata.Add(CreateLinkageAppXml(configData));
            xdata.Add(CreateWindowPlacementXml(configData));
            xdoc.Add(xdata);

            xdoc.Save(appConfigFilePath);
        }

        /// <summary>
        /// PreviousフォルダパスのXMLを生成する
        /// </summary>
        private XElement CreatePreviousPathXml(ConfigData configData)
        {
            var dataElement = new XElement(PREVIOUS_FOLDER_ELEM_NAME);
            var previousPath = new XElement(PREVIOUS_PATH_ELEM_NAME, configData.PreviousFolderPath == null ? new XText("") : new XText(configData.PreviousFolderPath));

            dataElement.Add(previousPath);
            return dataElement;
        }

        /// <summary>
        /// PreviousフォルダパスのXMLを解析する
        /// </summary>
        private void ParsePreviousPathXml(XDocument xdoc, ConfigData configData)
        {
            var dataElement = xdoc.Root.Element(PREVIOUS_FOLDER_ELEM_NAME);
            XElement previousPath = dataElement.Element(PREVIOUS_PATH_ELEM_NAME);

            configData.PreviousFolderPath = previousPath.Value;
        }

        /// <summary>
        /// 連携アプリのXMLを生成する
        /// </summary>
        private XElement CreateLinkageAppXml(ConfigData configData)
        {
            var linkageElement = new XElement(LINK_APP_ELEM_NAME);

            var linkageAppList = configData.LinkageAppList;
            foreach (var linkageApp in linkageAppList)
            {
                var dataElement = new XElement(LINK_APP_DATA_NAME);
                var appNameElement = new XElement(LINK_APP_NAME_ELEM_NAME, linkageApp == null ? new XText("") : new XText(linkageApp.AppName));
                var appPathElement = new XElement(LINK_APP_PATH_ELEM_NAME, linkageApp == null ? new XText("") : new XText(linkageApp.AppPath));

                dataElement.Add(appNameElement);
                dataElement.Add(appPathElement);

                linkageElement.Add(dataElement);
            }

            return linkageElement;
        }

        /// <summary>
        /// 連携アプリのXMLを解析する
        /// </summary>
        private void ParseLinkageAppXml(XDocument xdoc, ConfigData configData)
        {
            var linkageElement = xdoc.Root.Element(LINK_APP_ELEM_NAME);
            var dataElements = linkageElement.Elements(LINK_APP_DATA_NAME);

            foreach (var dataElement in dataElements)
            {
                XElement appNameElement = dataElement.Element(LINK_APP_NAME_ELEM_NAME);
                XElement appPathElement = dataElement.Element(LINK_APP_PATH_ELEM_NAME);

                if (!string.IsNullOrEmpty(appNameElement.Value) && !string.IsNullOrEmpty(appPathElement.Value))
                {
                    var linkageApp = new ExtraAppSetting(appNameElement.Value, appPathElement.Value);
                    configData.LinkageAppList.Add(linkageApp);
                }
            }
        }

        /// <summary>
        /// Windowのサイズ、位置のXMLを生成する
        /// </summary>
        private XElement CreateWindowPlacementXml(ConfigData configData)
        {
            var dataElement = new XElement(WINDOW_PLACEMENT_ELEM_NAME);
            var windowPlaceTopElement = new XElement(WINDOW_PLACEMENT_TOP_ELEM_NAME, new XText(configData.WindowPlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement(WINDOW_PLACEMENT_LEFT_ELEM_NAME, new XText(configData.WindowPlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement(WINDOW_PLACEMENT_RIGHT_ELEM_NAME, new XText(configData.WindowPlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement(WINDOW_PLACEMENT_BUTTOM_ELEM_NAME, new XText(configData.WindowPlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement(WINDOW_PLACEMENT_MAXPOSX_ELEM_NAME, new XText(configData.WindowPlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement(WINDOW_PLACEMENT_MAXPOSY_ELEM_NAME, new XText(configData.WindowPlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement(WINDOW_PLACEMENT_MINPOSX_ELEM_NAME, new XText(configData.WindowPlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement(WINDOW_PLACEMENT_MINPOSY_ELEM_NAME, new XText(configData.WindowPlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement(WINDOW_PLACEMENT_FLAG_ELEM_NAME, new XText(configData.WindowPlaceData.flags.ToString()));
            var windowSwElement = new XElement(WINDOW_PLACEMENT_SW_ELEM_NAME, new XText(configData.WindowPlaceData.showCmd.ToString()));

            dataElement.Add(windowPlaceTopElement);
            dataElement.Add(windowPlaceLeftElement);
            dataElement.Add(windowPlaceRightElement);
            dataElement.Add(windowPlaceButtonElement);
            dataElement.Add(windowMaxPositionX);
            dataElement.Add(windowMaxPositionY);
            dataElement.Add(windowMinPositionX);
            dataElement.Add(windowMinPositionY);
            dataElement.Add(windowFlag);
            dataElement.Add(windowSwElement);

            return dataElement;
        }

        /// <summary>
        /// Windowのサイズ、位置のXMLを解析する
        /// </summary>
        private void ParseWindowPlacementXml(XDocument xdoc, ConfigData configData)
        {
            var dataElement = xdoc.Root.Element(WINDOW_PLACEMENT_ELEM_NAME);
            var windowPlaceTopElement = dataElement.Element(WINDOW_PLACEMENT_TOP_ELEM_NAME);
            var windowPlaceLeftElement = dataElement.Element(WINDOW_PLACEMENT_LEFT_ELEM_NAME);
            var windowPlaceRightElement = dataElement.Element(WINDOW_PLACEMENT_RIGHT_ELEM_NAME);
            var windowPlaceButtomElement = dataElement.Element(WINDOW_PLACEMENT_BUTTOM_ELEM_NAME);
            var windowMaxPositionX = dataElement.Element(WINDOW_PLACEMENT_MAXPOSX_ELEM_NAME);
            var windowMaxPositionY = dataElement.Element(WINDOW_PLACEMENT_MAXPOSY_ELEM_NAME);
            var windowMinPositionX = dataElement.Element(WINDOW_PLACEMENT_MINPOSX_ELEM_NAME);
            var windowMinPositionY = dataElement.Element(WINDOW_PLACEMENT_MINPOSY_ELEM_NAME);
            var windowFlag = dataElement.Element(WINDOW_PLACEMENT_FLAG_ELEM_NAME);
            var windowSwElement = dataElement.Element(WINDOW_PLACEMENT_SW_ELEM_NAME);

            configData.WindowPlaceData.normalPosition.Top = Convert.ToInt32(windowPlaceTopElement.Value);
            configData.WindowPlaceData.normalPosition.Left = Convert.ToInt32(windowPlaceLeftElement.Value);
            configData.WindowPlaceData.normalPosition.Right = Convert.ToInt32(windowPlaceRightElement.Value);
            configData.WindowPlaceData.normalPosition.Bottom = Convert.ToInt32(windowPlaceButtomElement.Value);
            configData.WindowPlaceData.maxPosition.X = Convert.ToInt32(windowMaxPositionX.Value);
            configData.WindowPlaceData.maxPosition.Y = Convert.ToInt32(windowMaxPositionY.Value);
            configData.WindowPlaceData.minPosition.X = Convert.ToInt32(windowMinPositionX.Value);
            configData.WindowPlaceData.minPosition.Y = Convert.ToInt32(windowMinPositionY.Value);
            configData.WindowPlaceData.length = Marshal.SizeOf(typeof(MainWindow.WINDOWPLACEMENT));
            configData.WindowPlaceData.flags = Convert.ToInt32(windowFlag.Value);
            configData.WindowPlaceData.showCmd = (MainWindow.SW)Enum.Parse(typeof(MainWindow.SW), windowSwElement.Value);
        }
    }
}