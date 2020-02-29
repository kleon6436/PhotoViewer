using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace PhotoViewer.Model
{
    /// <summary>
    /// アプリケーション設定情報のデータクラス
    /// </summary>
    public class ConfigData
    {
        public string PreviousFolderPath;           // 前回終了時点の表示フォルダ
        public ExtraAppSetting LinkageApp;          // 登録されている連携アプリ
        public WINDOWPLACEMENT WindowPlaceData;     // ウィンドウの表示状態

        public ConfigData()
        {
            PreviousFolderPath = null;
            LinkageApp = null;
        }
    }

    /// <summary>
    /// アプリケーション設定情報の管理クラス
    /// </summary>
    public sealed class AppConfigManager
    {
        private readonly string AppConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\KcharyPhotographViewer\Setting.conf";
        private static AppConfigManager singleInstance = new AppConfigManager();

        // アプリケーション設定情報のデータ
        public ConfigData configData;

        private AppConfigManager()
        {
            configData = new ConfigData();
        }

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
                var configXmlObject = new ConfigXml(AppConfigFilePath);
                configXmlObject.Import(ref configData);
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
            string appConfigdirectory = Path.GetDirectoryName(AppConfigFilePath);
            if (!Directory.Exists(appConfigdirectory))
            {
                Directory.CreateDirectory(appConfigdirectory);
            }

            try
            {
                var configXmlObject = new ConfigXml(AppConfigFilePath);
                configXmlObject.Export(configData);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                throw;
            }
        }

        public void SetLinkageApp(ExtraAppSetting linkageApp)
        {
            this.configData.LinkageApp = linkageApp;
        }

        public void RemoveLinkageApp()
        {
            this.configData.LinkageApp = null;
        }
    }

    /// <summary>
    /// アプリケーション設定情報のXML管理クラス
    /// </summary>
    public class ConfigXml
    {
        // XMLの要素名
        private const string PREVIOUS_FOLDER_ELEM_NAME = "previous_folder";
        private const string PREVIOUS_PATH_ELEM_NAME = "previous_path";
        private const string LINK_APP_ELEM_NAME = "linkage_app_data";
        private const string LINK_APP_ID_ELEM_NAME = "id";
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
        private string appConfigFilePath;

        public ConfigXml(string filePath)
        {
            this.appConfigFilePath = filePath;
        }

        public void Import(ref ConfigData configData)
        {
            var xdoc = XDocument.Load(appConfigFilePath);

            ParsePreviousPathXml(xdoc, ref configData);
            ParseLinkageAppXml(xdoc, ref configData);
            ParseWindowPlacementXml(xdoc, ref configData);
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
        private void ParsePreviousPathXml(XDocument xdoc, ref ConfigData configData)
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
            var dataElement = new XElement(LINK_APP_ELEM_NAME);
            var appIdElement = new XElement(LINK_APP_ID_ELEM_NAME, configData.LinkageApp == null ? new XText("") : new XText(configData.LinkageApp.AppId.ToString()));
            var appNameElement = new XElement(LINK_APP_NAME_ELEM_NAME, configData.LinkageApp == null ? new XText("") : new XText(configData.LinkageApp.AppName));
            var appPathElement = new XElement(LINK_APP_PATH_ELEM_NAME, configData.LinkageApp == null ? new XText("") : new XText(configData.LinkageApp.AppPath));

            dataElement.Add(appIdElement);
            dataElement.Add(appNameElement);
            dataElement.Add(appPathElement);

            return dataElement;
        }

        /// <summary>
        /// 連携アプリのXMLを解析する
        /// </summary>
        private void ParseLinkageAppXml(XDocument xdoc, ref ConfigData configData)
        {
            var dataElement = xdoc.Root.Element(LINK_APP_ELEM_NAME);
            XElement appIdElement = dataElement.Element(LINK_APP_ID_ELEM_NAME);
            XElement appNameElement = dataElement.Element(LINK_APP_NAME_ELEM_NAME);
            XElement appPathElement = dataElement.Element(LINK_APP_PATH_ELEM_NAME);

            if (string.IsNullOrEmpty(appIdElement.Value) || string.IsNullOrEmpty(appNameElement.Value) || string.IsNullOrEmpty(appPathElement.Value))
            {
                configData.LinkageApp = null;
            }
            else
            {
                var linkageApp = new ExtraAppSetting(Convert.ToInt32(appIdElement.Value), appNameElement.Value, appPathElement.Value);
                configData.LinkageApp = linkageApp;
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
        private void ParseWindowPlacementXml(XDocument xdoc, ref ConfigData configData)
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

            configData.WindowPlaceData = windowPlaceData;
        }
    }

    #region WindowPlacement
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
    #endregion
}
