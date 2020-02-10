using System;
using System.IO;
using System.Runtime.InteropServices;
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
        // ウィンドウの表示状態
        public WINDOWPLACEMENT WindowPlaceData { get; set; }

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
            var xdata = new XElement("data");

            var linkageAppXml = CreateLinkageAppXml();
            xdata.Add(linkageAppXml);
            var windowPlacementXml = CreateWindowPlacementXml();
            xdata.Add(windowPlacementXml);

            xdoc.Add(xdata);
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
                ParseWindowPlacementXml(xdoc);
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
        /// <returns>XElement</returns>
        private XElement CreateLinkageAppXml()
        {
            var dataElement = new XElement("linkage_app_data");
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
        private void ParseLinkageAppXml(XDocument xdoc)
        {
            var dataElement = xdoc.Root.Element("linkage_app_data");
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
        /// Windowのサイズ、位置のXMLを生成する
        /// </summary>
        /// <returns>XElement</returns>
        private XElement CreateWindowPlacementXml()
        {
            var dataElement = new XElement("window_placement");
            var windowPlaceTopElement = new XElement("top", new XText(WindowPlaceData.normalPosition.Top.ToString()));
            var windowPlaceLeftElement = new XElement("left", new XText(WindowPlaceData.normalPosition.Left.ToString()));
            var windowPlaceRightElement = new XElement("right", new XText(WindowPlaceData.normalPosition.Right.ToString()));
            var windowPlaceButtonElement = new XElement("buttom", new XText(WindowPlaceData.normalPosition.Bottom.ToString()));
            var windowMaxPositionX = new XElement("maxPosX", new XText(WindowPlaceData.maxPosition.X.ToString()));
            var windowMaxPositionY = new XElement("maxPosY", new XText(WindowPlaceData.maxPosition.Y.ToString()));
            var windowMinPositionX = new XElement("minPosX", new XText(WindowPlaceData.minPosition.X.ToString()));
            var windowMinPositionY = new XElement("minPosY", new XText(WindowPlaceData.minPosition.Y.ToString()));
            var windowFlag = new XElement("windowFlag", new XText(WindowPlaceData.flags.ToString()));
            var windowSwElement = new XElement("sw", new XText(WindowPlaceData.showCmd.ToString()));

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
        private void ParseWindowPlacementXml(XDocument xdoc)
        {
            var dataElement = xdoc.Root.Element("window_placement");
            var windowPlaceTopElement = dataElement.Element("top");
            var windowPlaceLeftElement = dataElement.Element("left");
            var windowPlaceRightElement = dataElement.Element("right");
            var windowPlaceButtomElement = dataElement.Element("buttom");
            var windowMaxPositionX = dataElement.Element("maxPosX");
            var windowMaxPositionY = dataElement.Element("maxPosY");
            var windowMinPositionX = dataElement.Element("minPosX");
            var windowMinPositionY = dataElement.Element("minPosY");
            var windowFlag = dataElement.Element("windowFlag");
            var windowSwElement = dataElement.Element("sw");

            WINDOWPLACEMENT windowPlaceData;
            windowPlaceData.normalPosition.Top = Convert.ToInt32(windowPlaceTopElement.Value);
            windowPlaceData.normalPosition.Left = Convert.ToInt32(windowPlaceLeftElement.Value);
            windowPlaceData.normalPosition.Right = Convert.ToInt32(windowPlaceRightElement.Value);
            windowPlaceData.normalPosition.Bottom = Convert.ToInt32(windowPlaceButtomElement.Value);
            windowPlaceData.maxPosition.X = Convert.ToInt32(windowMaxPositionX.Value);
            windowPlaceData.maxPosition.Y = Convert.ToInt32(windowMaxPositionY.Value);
            windowPlaceData.minPosition.X = Convert.ToInt32(windowMinPositionX.Value);
            windowPlaceData.minPosition.Y = Convert.ToInt32(windowMinPositionY.Value);
            windowPlaceData.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            windowPlaceData.flags = Convert.ToInt32(windowFlag.Value);

            var sw = SW.SHOWNORMAL;
            try
            {
                sw = (SW)Enum.Parse(typeof(SW), windowSwElement.Value);
            }
            catch (Exception ex)
            {
                // ログだけ取得しておく
                App.LogException(ex);
            }
            windowPlaceData.showCmd = (sw == SW.SHOWMINIMIZED) ? SW.SHOWNORMAL : sw;

            WindowPlaceData = windowPlaceData;
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

    public class WindowPlacement
    {
#pragma warning disable CA1401 // P/Invokes should not be visible
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
#pragma warning restore CA1401 // P/Invokes should not be visible
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public SW showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    public enum SW
    {
        HIDE = 0,
        SHOWNORMAL = 1,
        SHOWMINIMIZED = 2,
        SHOWMAXIMIZED = 3,
        SHOWNOACTIVATE = 4,
        SHOW = 5,
        MINIMIZE = 6,
        SHOWMINNOACTIVE = 7,
        SHOWNA = 8,
        RESTORE = 9,
        SHOWDEFAULT = 10,
    }
}
