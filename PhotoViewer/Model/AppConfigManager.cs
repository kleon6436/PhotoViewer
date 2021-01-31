using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// Management class of application setting information.
    /// </summary>
    public sealed class AppConfigManager
    {
        /// <summary>
        /// Data of application setting information.
        /// </summary>
        public AppConfigData ConfigData { get; private init; }

        private static readonly string AppConfigFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KcharyPhotoViewer\\Setting.conf";
        private static readonly AppConfigManager SingleInstance = new() { ConfigData = new AppConfigData() };
        private static readonly AppConfigXml ConfigXmlObject = new(AppConfigFilePath);

        /// <summary>
        /// Get instance of application setting information class.
        /// </summary>
        public static AppConfigManager GetInstance()
        {
            return SingleInstance;
        }

        /// <summary>
        /// Import application data from application configure file.
        /// </summary>
        public void Import()
        {
            try
            {
                ConfigXmlObject.Import(ConfigData);
            }
            catch (Exception ex)
            {
                // Ignore exceptions when loading.
                App.LogException(ex);
            }
        }

        /// <summary>
        /// Export application data to application configure file.
        /// </summary>
        public void Export()
        {
            // Create folder if it does not exist.
            var appConfigDirectory = Path.GetDirectoryName(AppConfigFilePath);
            if (!Directory.Exists(appConfigDirectory))
            {
                Directory.CreateDirectory(appConfigDirectory ?? throw new InvalidOperationException());
            }

            try
            {
                ConfigXmlObject.Export(ConfigData);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
        }

        /// <summary>
        /// Set list of linked applications.
        /// </summary>
        /// <param name="linkageAppList">List of linked applications</param>
        public void SetLinkageApp(IEnumerable<ExtraAppSetting> linkageAppList)
        {
            ConfigData.LinkageAppList = linkageAppList.ToList();
        }

        /// <summary>
        /// Remove linked application from linked application list.
        /// </summary>
        /// <param name="linkageAppList">List of linked applications</param>
        public void RemoveLinkageApp(IEnumerable<ExtraAppSetting> linkageAppList)
        {
            ConfigData.LinkageAppList = linkageAppList.ToList();
        }
    }
}