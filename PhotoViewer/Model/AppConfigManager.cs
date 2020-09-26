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
        public AppConfigData ConfigData { get; private set; }

        private static readonly string appConfigFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KcharyPhotoViewer\\Setting.conf";
        private static readonly AppConfigManager singleInstance = new AppConfigManager { ConfigData = new AppConfigData() };
        private static readonly AppConfigXml ConfigXmlObject = new AppConfigXml(appConfigFilePath);

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
                ConfigXmlObject.Import(ConfigData);
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
            var appConfigdirectory = Path.GetDirectoryName(appConfigFilePath);
            if (!Directory.Exists(appConfigdirectory))
            {
                Directory.CreateDirectory(appConfigdirectory);
            }

            try
            {
                ConfigXmlObject.Export(ConfigData);
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
        public void SetLinkageApp(IEnumerable<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist.ToList();
        }

        /// <summary>
        /// Remove linked app from linked app list.
        /// </summary>
        /// <param name="linkageApplist">List of linked apps</param>
        public void RemoveLinkageApp(IEnumerable<ExtraAppSetting> linkageApplist)
        {
            ConfigData.LinkageAppList = linkageApplist.ToList();
        }
    }
}