using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FastEnumUtility;
using Kchary.PhotoViewer.Data;
using Kchary.PhotoViewer.Views;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// アプリケーション設定ファイル用のXML管理クラス
    /// </summary>
    public sealed class AppConfigXml
    {
        private const string PreviousFolderElemName = "previous_folder";
        private const string PreviousPathElemName = "previous_path";
        private const string LinkAppElemName = "linkage_app";
        private const string LinkAppDataName = "linkage_app_data";
        private const string LinkAppNameElemName = "name";
        private const string LinkAppPathElemName = "path";
        private const string PlacementElemName = "window_placement";
        private const string PlacementTopElemName = "top";
        private const string PlacementLeftElemName = "left";
        private const string PlacementRightElemName = "right";
        private const string PlacementBottomElemName = "buttom";
        private const string PlacementMaxPosXElemName = "maxPosX";
        private const string PlacementMaxPosYElemName = "maxPosY";
        private const string PlacementMinPosXElemName = "minPosX";
        private const string PlacementMinPosYElemName = "minPosY";
        private const string PlacementFlagElemName = "windowFlag";
        private const string PlacementSwElemName = "sw";

        /// <summary>
        /// アプリケーション設定情報をXMLファイルからインポートする
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        /// <param name="configData">アプリケーション設定データ</param>
        public static void Import(string filePath, AppConfigData configData)
        {
            var doc = XDocument.Load(filePath);

            ParsePreviousPathXml(doc, configData);
            ParseLinkageAppXml(doc, configData);
            ParseWindowPlacementXml(doc, configData);
        }

        /// <summary>
        /// アプリケーション設定情報を既定XMLファイルに出力する
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        /// <param name="configData">アプリケーション設定データ</param>
        public static void Export(string filePath, AppConfigData configData)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var data = new XElement("data");

            data.Add(CreatePreviousPathXml(configData));
            data.Add(CreateLinkageAppXml(configData));
            data.Add(CreateWindowPlacementXml(configData));
            doc.Add(data);

            doc.Save(filePath);
        }

        /// <summary>
        /// 前回のフォルダパスのXMLデータを作成する
        /// </summary>
        /// <param name="configData">アプリケーション設定データ</param>
        private static XElement CreatePreviousPathXml(AppConfigData configData)
        {
            var dataElement = new XElement(PreviousFolderElemName);
            var previousPath = new XElement(PreviousPathElemName, configData.PreviousFolderPath == null ? new XText("") : new XText(configData.PreviousFolderPath));

            dataElement.Add(previousPath);
            return dataElement;
        }

        /// <summary>
        /// 前回のフォルダパスをXMLから読み込む
        /// </summary>
        /// <param name="doc">XMLデータ</param>
        /// <param name="configData">アプリケーション設定データ</param>
        private static void ParsePreviousPathXml(XDocument doc, AppConfigData configData)
        {
            var dataElement = doc.Root?.Element(PreviousFolderElemName);
            var previousPath = dataElement?.Element(PreviousPathElemName);

            if (previousPath == null)
            {
                return;
            }
            configData.PreviousFolderPath = previousPath.Value;
        }

        /// <summary>
        /// 登録アプリリストのXMLデータを作成する
        /// </summary>
        /// <param name="configData">アプリケーション設定データ</param>
        private static XElement CreateLinkageAppXml(AppConfigData configData)
        {
            var linkageElement = new XElement(LinkAppElemName);

            var linkageAppList = configData.LinkageAppList;
            foreach (var linkageApp in linkageAppList)
            {
                var dataElement = new XElement(LinkAppDataName);
                var appNameElement = new XElement(LinkAppNameElemName, linkageApp == null ? new XText("") : new XText(linkageApp.AppName));
                var appPathElement = new XElement(LinkAppPathElemName, linkageApp == null ? new XText("") : new XText(linkageApp.AppPath));

                dataElement.Add(appNameElement);
                dataElement.Add(appPathElement);

                linkageElement.Add(dataElement);
            }

            return linkageElement;
        }

        /// <summary>
        /// 登録アプリリストをXMLデータから取得する
        /// </summary>
        /// <param name="doc">XMLデータ</param>
        /// <param name="configData">アプリケーション設定データ</param>
        private static void ParseLinkageAppXml(XDocument doc, AppConfigData configData)
        {
            var linkageElement = doc.Root?.Element(LinkAppElemName);
            var dataElements = linkageElement?.Elements(LinkAppDataName);

            if (dataElements == null)
            {
                return;
            }

            foreach (var dataElement in dataElements)
            {
                var appNameElement = dataElement.Element(LinkAppNameElemName);
                var appPathElement = dataElement.Element(LinkAppPathElemName);

                if (appPathElement != null && appNameElement != null &&
                    (string.IsNullOrEmpty(appNameElement.Value) || string.IsNullOrEmpty(appPathElement.Value)))
                {
                    continue;
                }

                var linkageApp = new ExtraAppSetting { AppName = appNameElement?.Value, AppPath = appPathElement?.Value };
                configData.LinkageAppList.Add(linkageApp);
            }
        }

        /// <summary>
        /// ウィンドウサイズ、位置などの情報のXMLデータを作成する
        /// </summary>
        /// <param name="configData">アプリケーション設定データ</param>
        private static XElement CreateWindowPlacementXml(AppConfigData configData)
        {
            var dataElement = new XElement(PlacementElemName);
            var windowPlaceTopElement = new XElement(PlacementTopElemName, new XText(configData.PlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement(PlacementLeftElemName, new XText(configData.PlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement(PlacementRightElemName, new XText(configData.PlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement(PlacementBottomElemName, new XText(configData.PlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement(PlacementMaxPosXElemName, new XText(configData.PlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement(PlacementMaxPosYElemName, new XText(configData.PlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement(PlacementMinPosXElemName, new XText(configData.PlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement(PlacementMinPosYElemName, new XText(configData.PlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement(PlacementFlagElemName, new XText(configData.PlaceData.flags.ToString()));
            var windowSwElement = new XElement(PlacementSwElemName, new XText(configData.PlaceData.showCmd.ToString()));

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
        /// ウィンドウサイズ、位置などの情報をXMLデータから取得する
        /// </summary>
        /// <param name="doc">XMLデータ</param>
        /// <param name="configData">アプリケーション設定データ</param>
        private static void ParseWindowPlacementXml(XDocument doc, AppConfigData configData)
        {
            var dataElement = doc.Root?.Element(PlacementElemName);
            var windowPlaceTopElement = dataElement?.Element(PlacementTopElemName);
            var windowPlaceLeftElement = dataElement?.Element(PlacementLeftElemName);
            var windowPlaceRightElement = dataElement?.Element(PlacementRightElemName);
            var windowPlaceBottomElement = dataElement?.Element(PlacementBottomElemName);
            var windowMaxPositionX = dataElement?.Element(PlacementMaxPosXElemName);
            var windowMaxPositionY = dataElement?.Element(PlacementMaxPosYElemName);
            var windowMinPositionX = dataElement?.Element(PlacementMinPosXElemName);
            var windowMinPositionY = dataElement?.Element(PlacementMinPosYElemName);
            var windowFlag = dataElement?.Element(PlacementFlagElemName);
            var windowSwElement = dataElement?.Element(PlacementSwElemName);

            configData.PlaceData.normalPosition.Top = Convert.ToInt32(windowPlaceTopElement?.Value);
            configData.PlaceData.normalPosition.Left = Convert.ToInt32(windowPlaceLeftElement?.Value);
            configData.PlaceData.normalPosition.Right = Convert.ToInt32(windowPlaceRightElement?.Value);
            configData.PlaceData.normalPosition.Bottom = Convert.ToInt32(windowPlaceBottomElement?.Value);
            configData.PlaceData.maxPosition.X = Convert.ToInt32(windowMaxPositionX?.Value);
            configData.PlaceData.maxPosition.Y = Convert.ToInt32(windowMaxPositionY?.Value);
            configData.PlaceData.minPosition.X = Convert.ToInt32(windowMinPositionX?.Value);
            configData.PlaceData.minPosition.Y = Convert.ToInt32(windowMinPositionY?.Value);
            configData.PlaceData.length = Marshal.SizeOf(typeof(MainWindow.NativeMethods.Placement));
            configData.PlaceData.flags = Convert.ToInt32(windowFlag?.Value);
            configData.PlaceData.showCmd = windowSwElement == null ? MainWindow.NativeMethods.Sw.ShowNormal : FastEnum.Parse<MainWindow.NativeMethods.Sw>(windowSwElement.Value);
        }
    }
}