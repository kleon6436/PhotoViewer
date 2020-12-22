using System.Collections.Generic;

namespace Kchary.PhotoViewer.Model
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
        public List<ExtraAppSetting> LinkageAppList { get; set; } = new List<ExtraAppSetting>();

        /// <summary>
        /// Window display status
        /// </summary>
        public MainWindow.WINDOWPLACEMENT WindowPlaceData;
    }
}