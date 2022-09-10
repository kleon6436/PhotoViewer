using FastEnumUtility;
using Kchary.PhotoViewer.Views;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// アプリケーションデータクラス
    /// </summary>
    public sealed class AppConfigData
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

        #region XMLに記述する要素名

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

        #endregion

        /// <summary>
        /// アプリケーション設定情報をXMLファイルからインポートする
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        public void Import(string filePath)
        {
            var doc = XDocument.Load(filePath);

            ParsePreviousPathXml(doc);
            ParseLinkageAppXml(doc);
            ParseWindowPlacementXml(doc);
        }

        /// <summary>
        /// アプリケーション設定情報を既定XMLファイルに出力する
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        public void Export(string filePath)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var data = new XElement("data");

            data.Add(CreatePreviousPathXml());
            data.Add(CreateLinkageAppXml());
            data.Add(CreateWindowPlacementXml());
            doc.Add(data);

            doc.Save(filePath);
        }

        /// <summary>
        /// 前回のフォルダパスのXMLデータを作成する
        /// </summary>
        /// <returns>
        /// XMLの要素
        /// </returns>
        private XElement CreatePreviousPathXml()
        {
            var dataElement = new XElement(PreviousFolderElemName);
            var previousPath = new XElement(PreviousPathElemName, PreviousFolderPath == null ? new XText("") : new XText(PreviousFolderPath));

            dataElement.Add(previousPath);
            return dataElement;
        }

        /// <summary>
        /// 前回のフォルダパスをXMLから読み込む
        /// </summary>
        /// <param name="doc">XMLデータ</param>
        private void ParsePreviousPathXml(XDocument doc)
        {
            var dataElement = doc.Root?.Element(PreviousFolderElemName);
            var previousPath = dataElement?.Element(PreviousPathElemName);

            if (previousPath == null)
            {
                return;
            }
            PreviousFolderPath = previousPath.Value;
        }

        /// <summary>
        /// 登録アプリリストのXMLデータを作成する
        /// </summary>
        /// <returns>
        /// XMLの要素
        /// </returns>
        private XElement CreateLinkageAppXml()
        {
            var linkageElement = new XElement(LinkAppElemName);

            foreach (var linkageApp in LinkageAppList)
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
        private void ParseLinkageAppXml(XDocument doc)
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
                LinkageAppList.Add(linkageApp);
            }
        }

        /// <summary>
        /// ウィンドウサイズ、位置などの情報のXMLデータを作成する
        /// </summary>
        /// <returns>
        /// XMLの要素
        /// </returns>
        private XElement CreateWindowPlacementXml()
        {
            var dataElement = new XElement(PlacementElemName);
            var windowPlaceTopElement = new XElement(PlacementTopElemName, new XText(PlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement(PlacementLeftElemName, new XText(PlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement(PlacementRightElemName, new XText(PlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement(PlacementBottomElemName, new XText(PlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement(PlacementMaxPosXElemName, new XText(PlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement(PlacementMaxPosYElemName, new XText(PlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement(PlacementMinPosXElemName, new XText(PlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement(PlacementMinPosYElemName, new XText(PlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement(PlacementFlagElemName, new XText(PlaceData.flags.ToString()));
            var windowSwElement = new XElement(PlacementSwElemName, new XText(PlaceData.showCmd.ToString()));

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
        private void ParseWindowPlacementXml(XDocument doc)
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

            PlaceData.normalPosition.Top = Convert.ToInt32(windowPlaceTopElement?.Value);
            PlaceData.normalPosition.Left = Convert.ToInt32(windowPlaceLeftElement?.Value);
            PlaceData.normalPosition.Right = Convert.ToInt32(windowPlaceRightElement?.Value);
            PlaceData.normalPosition.Bottom = Convert.ToInt32(windowPlaceBottomElement?.Value);
            PlaceData.maxPosition.X = Convert.ToInt32(windowMaxPositionX?.Value);
            PlaceData.maxPosition.Y = Convert.ToInt32(windowMaxPositionY?.Value);
            PlaceData.minPosition.X = Convert.ToInt32(windowMinPositionX?.Value);
            PlaceData.minPosition.Y = Convert.ToInt32(windowMinPositionY?.Value);
            PlaceData.length = Marshal.SizeOf(typeof(MainWindow.NativeMethods.Placement));
            PlaceData.flags = Convert.ToInt32(windowFlag?.Value);
            PlaceData.showCmd = windowSwElement == null ? MainWindow.NativeMethods.Sw.ShowNormal : FastEnum.Parse<MainWindow.NativeMethods.Sw>(windowSwElement.Value);
        }
    }
}