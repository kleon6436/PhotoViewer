using Microsoft.Win32;
using PhotoViewer.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace PhotoViewer.ViewModels
{
    public class LinkageAppViewModel : BindableBase
    {
        private const int MAX_LINK_APP_NUM = 10;

        #region UI binding parameter

        private string linkAppPath;

        public string LinkAppPath
        {
            get { return linkAppPath; }
            set { SetProperty(ref linkAppPath, value); }
        }

        public ObservableCollection<ExtraAppSetting> LinkageAppList { get; } = new ObservableCollection<ExtraAppSetting>();

        #endregion UI binding parameter

        #region Command

        public ICommand LinkAppReferenceCommand { get; private set; }
        public ICommand RegisterLinkAppCommand { get; private set; }
        public ICommand DeleteLinkAppCommand { get; private set; }

        #endregion Command

        public event EventHandler ChangeLinkageAppEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new DelegateCommand(LinkAppReferenceButtonClicked);
            RegisterLinkAppCommand = new DelegateCommand(RegisterLinkAppButtonClicked);
            DeleteLinkAppCommand = new DelegateCommand<ExtraAppSetting>(DeleteLinkAppButtonClicked);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var linkageAppList = appConfigManager.ConfigData.LinkageAppList.ToArray();
            if (linkageAppList != null && linkageAppList.Any())
            {
                LinkageAppList.Clear();
                LinkageAppList.AddRange(linkageAppList);
            }
        }

        /// <summary>
        /// Event when the reference button is pressed.
        /// </summary>
        private void LinkAppReferenceButtonClicked()
        {
            string previousLinkAppPath = LinkAppPath;

            const string DialogTitle = "連携アプリ選択ダイアログ";
            const string DialogDefaultExt = ".exe";

            var dialog = new OpenFileDialog
            {
                Title = DialogTitle,
                DefaultExt = DialogDefaultExt
            };

            if (Environment.Is64BitProcess)
            {
                dialog.InitialDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            }
            else
            {
                dialog.InitialDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);
            }

            if (dialog.ShowDialog() != true)
            {
                LinkAppPath = previousLinkAppPath;
                return;
            }

            LinkAppPath = dialog.FileName;
        }

        /// <summary>
        /// Event when register button is pressed.
        /// </summary>
        private void RegisterLinkAppButtonClicked()
        {
            if (LinkageAppList.Count > MAX_LINK_APP_NUM)
            {
                return;
            }

            var linkageApp = new ExtraAppSetting(Path.GetFileNameWithoutExtension(LinkAppPath), LinkAppPath);
            if (LinkageAppList.Any(x => x.AppName == linkageApp.AppName || x.AppPath == linkageApp.AppPath))
            {
                return;
            }

            LinkageAppList.Add(linkageApp);

            // Export information to Config file.
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.SetLinkageApp(LinkageAppList.ToList());
            appConfigManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            LinkAppPath = "";
        }

        /// <summary>
        /// Event when delete button is pressed.
        /// </summary>
        private void DeleteLinkAppButtonClicked(ExtraAppSetting deleteAppSetting)
        {
            if (LinkageAppList == null || LinkageAppList.Count == 0 || deleteAppSetting == null)
            {
                return;
            }

            if (LinkageAppList.Any(x => x == deleteAppSetting))
            {
                LinkageAppList.Remove(deleteAppSetting);

                // Export information to config file.
                AppConfigManager appConfigManager = AppConfigManager.GetInstance();
                appConfigManager.RemoveLinkageApp(LinkageAppList.ToList());
                appConfigManager.Export();

                ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}