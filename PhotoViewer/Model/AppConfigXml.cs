using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// XML management class of application setting information.
    /// </summary>
    public sealed class AppConfigXml
    {
        private const string PreviousFolderElemName = "previous_folder";
        private const string PreviousPathElemName = "previous_path";
        private const string LinkAppElemName = "linkage_app";
        private const string LinkAppDataName = "linkage_app_data";
        private const string LinkAppNameElemName = "name";
        private const string LinkAppPathElemName = "path";
        private const string WindowPlacementElemName = "window_placement";
        private const string WindowPlacementTopElemName = "top";
        private const string WindowPlacementLeftElemName = "left";
        private const string WindowPlacementRightElemName = "right";
        private const string WindowsPlacementButtomElemName = "buttom";
        private const string WindowPlacementMaxPosXElemName = "maxPosX";
        private const string WindowPlacementMaxPosYElemName = "maxPosY";
        private const string WindowPlacementMinPosXElemName = "minPosX";
        private const string WindowPlacementMinPosYElemName = "minPosY";
        private const string WindowPlacementFlagElemName = "windowFlag";
        private const string WindowPlacementSwElemName = "sw";

        // File path of application setting information.
        private readonly string appConfigFilePath;

        public AppConfigXml(string filePath)
        {
            appConfigFilePath = filePath;
        }

        public void Import(AppConfigData configData)
        {
            var xdoc = XDocument.Load(appConfigFilePath);

            ParsePreviousPathXml(xdoc, configData);
            ParseLinkageAppXml(xdoc, configData);
            ParseWindowPlacementXml(xdoc, configData);
        }

        public void Export(AppConfigData configData)
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
        /// Generate xml of previous folder path.
        /// </summary>
        private static XElement CreatePreviousPathXml(AppConfigData configData)
        {
            var dataElement = new XElement(PreviousFolderElemName);
            var previousPath = new XElement(PreviousPathElemName, configData.PreviousFolderPath == null ? new XText("") : new XText(configData.PreviousFolderPath));

            dataElement.Add(previousPath);
            return dataElement;
        }

        /// <summary>
        /// Parse xml of previous path.
        /// </summary>
        private static void ParsePreviousPathXml(XDocument xdoc, AppConfigData configData)
        {
            XElement dataElement = xdoc.Root.Element(PreviousFolderElemName);
            XElement previousPath = dataElement.Element(PreviousPathElemName);

            configData.PreviousFolderPath = previousPath.Value;
        }

        /// <summary>
        /// Generate XML for linked application.
        /// </summary>
        private static XElement CreateLinkageAppXml(AppConfigData configData)
        {
            var linkageElement = new XElement(LinkAppElemName);

            List<ExtraAppSetting> linkageAppList = configData.LinkageAppList;
            foreach (ExtraAppSetting linkageApp in linkageAppList)
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
        /// Parse the XML of the linked application.
        /// </summary>
        private static void ParseLinkageAppXml(XDocument xdoc, AppConfigData configData)
        {
            XElement linkageElement = xdoc.Root.Element(LinkAppElemName);
            IEnumerable<XElement> dataElements = linkageElement.Elements(LinkAppDataName);

            foreach (XElement dataElement in dataElements)
            {
                XElement appNameElement = dataElement.Element(LinkAppNameElemName);
                XElement appPathElement = dataElement.Element(LinkAppPathElemName);

                if (!string.IsNullOrEmpty(appNameElement.Value) && !string.IsNullOrEmpty(appPathElement.Value))
                {
                    var linkageApp = new ExtraAppSetting { AppName = appNameElement.Value, AppPath = appPathElement.Value };
                    configData.LinkageAppList.Add(linkageApp);
                }
            }
        }

        /// <summary>
        /// Generate XML of Window size and position.
        /// </summary>
        private static XElement CreateWindowPlacementXml(AppConfigData configData)
        {
            var dataElement = new XElement(WindowPlacementElemName);
            var windowPlaceTopElement = new XElement(WindowPlacementTopElemName, new XText(configData.WindowPlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement(WindowPlacementLeftElemName, new XText(configData.WindowPlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement(WindowPlacementRightElemName, new XText(configData.WindowPlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement(WindowsPlacementButtomElemName, new XText(configData.WindowPlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement(WindowPlacementMaxPosXElemName, new XText(configData.WindowPlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement(WindowPlacementMaxPosYElemName, new XText(configData.WindowPlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement(WindowPlacementMinPosXElemName, new XText(configData.WindowPlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement(WindowPlacementMinPosYElemName, new XText(configData.WindowPlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement(WindowPlacementFlagElemName, new XText(configData.WindowPlaceData.flags.ToString()));
            var windowSwElement = new XElement(WindowPlacementSwElemName, new XText(configData.WindowPlaceData.showCmd.ToString()));

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
        /// Parse xml of window size and position.
        /// </summary>
        private static void ParseWindowPlacementXml(XDocument xdoc, AppConfigData configData)
        {
            XElement dataElement = xdoc.Root.Element(WindowPlacementElemName);
            XElement windowPlaceTopElement = dataElement.Element(WindowPlacementTopElemName);
            XElement windowPlaceLeftElement = dataElement.Element(WindowPlacementLeftElemName);
            XElement windowPlaceRightElement = dataElement.Element(WindowPlacementRightElemName);
            XElement windowPlaceButtomElement = dataElement.Element(WindowsPlacementButtomElemName);
            XElement windowMaxPositionX = dataElement.Element(WindowPlacementMaxPosXElemName);
            XElement windowMaxPositionY = dataElement.Element(WindowPlacementMaxPosYElemName);
            XElement windowMinPositionX = dataElement.Element(WindowPlacementMinPosXElemName);
            XElement windowMinPositionY = dataElement.Element(WindowPlacementMinPosYElemName);
            XElement windowFlag = dataElement.Element(WindowPlacementFlagElemName);
            XElement windowSwElement = dataElement.Element(WindowPlacementSwElemName);

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