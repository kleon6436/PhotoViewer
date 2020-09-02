using System.Collections.Generic;

namespace PhotoViewer.Model
{
    /// <summary>
    /// Data class of application setting information
    /// </summary>
    public sealed class AppConfigData
    {
        /// <summary>
        /// Folder path at the time of the previous end.
        /// </summary>
        public string PreviousFolderPath { get; set; }

        /// <summary>
        /// Registered linked application.
        /// </summary>
        public List<ExtraAppSetting> LinkageAppList { get; set; }

        /// <summary>
        /// Window display status
        /// </summary>
        public MainWindow.WINDOWPLACEMENT WindowPlaceData;

        public AppConfigData()
        {
            PreviousFolderPath = null;
            LinkageAppList = new List<ExtraAppSetting>();
        }
    }
}