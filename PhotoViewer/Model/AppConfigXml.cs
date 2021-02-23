using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Kchary.PhotoViewer.Views;

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
            var dataElement = xdoc.Root?.Element(PreviousFolderElemName);
            var previousPath = dataElement?.Element(PreviousPathElemName);

            if (previousPath == null)
            {
                return;
            }
            configData.PreviousFolderPath = previousPath.Value;
        }

        /// <summary>
        /// Generate XML for linked application.
        /// </summary>
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
        /// Parse the XML of the linked application.
        /// </summary>
        private static void ParseLinkageAppXml(XDocument xdoc, AppConfigData configData)
        {
            var linkageElement = xdoc.Root?.Element(LinkAppElemName);
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

                var linkageApp = new ExtraAppSetting {AppName = appNameElement?.Value, AppPath = appPathElement?.Value};
                configData.LinkageAppList.Add(linkageApp);
            }
        }

        /// <summary>
        /// Generate XML of Window size and position.
        /// </summary>
        private static XElement CreateWindowPlacementXml(AppConfigData configData)
        {
            var dataElement = new XElement(WindowPlacementElemName);
            var windowPlaceTopElement = new XElement(WindowPlacementTopElemName, new XText(configData.PlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement(WindowPlacementLeftElemName, new XText(configData.PlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement(WindowPlacementRightElemName, new XText(configData.PlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement(WindowsPlacementButtomElemName, new XText(configData.PlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement(WindowPlacementMaxPosXElemName, new XText(configData.PlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement(WindowPlacementMaxPosYElemName, new XText(configData.PlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement(WindowPlacementMinPosXElemName, new XText(configData.PlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement(WindowPlacementMinPosYElemName, new XText(configData.PlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement(WindowPlacementFlagElemName, new XText(configData.PlaceData.flags.ToString()));
            var windowSwElement = new XElement(WindowPlacementSwElemName, new XText(configData.PlaceData.showCmd.ToString()));

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
            var dataElement = xdoc.Root?.Element(WindowPlacementElemName);
            var windowPlaceTopElement = dataElement?.Element(WindowPlacementTopElemName);
            var windowPlaceLeftElement = dataElement?.Element(WindowPlacementLeftElemName);
            var windowPlaceRightElement = dataElement?.Element(WindowPlacementRightElemName);
            var windowPlaceButtomElement = dataElement?.Element(WindowsPlacementButtomElemName);
            var windowMaxPositionX = dataElement?.Element(WindowPlacementMaxPosXElemName);
            var windowMaxPositionY = dataElement?.Element(WindowPlacementMaxPosYElemName);
            var windowMinPositionX = dataElement?.Element(WindowPlacementMinPosXElemName);
            var windowMinPositionY = dataElement?.Element(WindowPlacementMinPosYElemName);
            var windowFlag = dataElement?.Element(WindowPlacementFlagElemName);
            var windowSwElement = dataElement?.Element(WindowPlacementSwElemName);

            configData.PlaceData.normalPosition.Top = Convert.ToInt32(windowPlaceTopElement?.Value);
            configData.PlaceData.normalPosition.Left = Convert.ToInt32(windowPlaceLeftElement?.Value);
            configData.PlaceData.normalPosition.Right = Convert.ToInt32(windowPlaceRightElement?.Value);
            configData.PlaceData.normalPosition.Bottom = Convert.ToInt32(windowPlaceButtomElement?.Value);
            configData.PlaceData.maxPosition.X = Convert.ToInt32(windowMaxPositionX?.Value);
            configData.PlaceData.maxPosition.Y = Convert.ToInt32(windowMaxPositionY?.Value);
            configData.PlaceData.minPosition.X = Convert.ToInt32(windowMinPositionX?.Value);
            configData.PlaceData.minPosition.Y = Convert.ToInt32(windowMinPositionY?.Value);
            configData.PlaceData.length = Marshal.SizeOf(typeof(MainWindow.NativeMethods.WindowPlacement));
            configData.PlaceData.flags = Convert.ToInt32(windowFlag?.Value);
            if (windowSwElement == null)
            {
                configData.PlaceData.showCmd = MainWindow.NativeMethods.Sw.ShowNormal;
            }
            else
            {
                configData.PlaceData.showCmd = (MainWindow.NativeMethods.Sw)Enum.Parse(typeof(MainWindow.NativeMethods.Sw), windowSwElement.Value);
            }
        }
    }
}