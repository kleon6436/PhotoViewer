using System;
using System.Collections.Generic;
using System.IO;

namespace PhotoViewer.Model
{
    /// <summary>
    /// Management class of application setting information.
    /// </summary>
    public sealed class AppConfigManager
    {
        /// <summary>
        /// Data of application setting information.
        /// </summary>
        public AppConfigData ConfigData { get; set; }

        private readonly string appConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\KcharyPhotoViewer\Setting.conf";
        private static readonly AppConfigManager singleInstance = new AppConfigManager();

        /// <summary>
        /// Get instance of application setting information class.
        /// </summary>
        public static AppConfigManager GetInstance()
        {
            return singleInstance;
        }

        /// <summary>
        /// Import application data from app config file.
        /// </summary>
        public void Import()
        {
            try
            {
                var configXmlObject = new AppConfigXml(appConfigFilePath);
                configXmlObject.Import(ConfigData);
            }
            catch (Exception ex)
            {
                // Ignore exceptions when loading.
                App.LogException(ex);
            }
        }

        /// <summary>
        /// Export application data to app config file.
        /// </summary>
        public void Export()
        {
            // Create folder if it does not exist.
            string appConfigdirectory = Path.GetDirectoryName(appConfigFilePath);
            if (!Directory.Exists(appConfigdirectory))
            {
                Directory.CreateDirectory(appConfigdirectory);
            }

            try
            {
                var configXmlObject = new AppConfigXml(appConfigFilePath);
                configXmlObject.Export(ConfigData);
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// Set list of linked apps.
        /// </summary>
        /// <param name="linkageApplist">List of linked apps</param>
        public void SetLinkageApp(List<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist;
        }

        /// <summary>
        /// Remove linked app from linked app list.
        /// </summary>
        /// <param name="linkageApplist">List of linked apps</param>
        public void RemoveLinkageApp(List<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist;
        }

        private AppConfigManager()
        {
            ConfigData = new AppConfigData();
        }
    }
}