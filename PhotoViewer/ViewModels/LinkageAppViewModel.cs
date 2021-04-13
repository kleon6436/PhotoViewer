using Kchary.PhotoViewer.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Kchary.PhotoViewer.ViewModels
{
    public class LinkageAppViewModel : BindableBase
    {
        /// <summary>
        /// Maximum number of link application.
        /// </summary>
        private const int MaxLinkAppNum = 10;

        #region UI binding parameter

        private string linkAppPath;

        public string LinkAppPath
        {
            get => linkAppPath;
            set => SetProperty(ref linkAppPath, value);
        }

        public ObservableCollection<ExtraAppSetting> LinkageAppList { get; } = new();

        #endregion UI binding parameter

        #region Command

        public ICommand LinkAppReferenceCommand { get; }
        public ICommand RegisterLinkAppCommand { get; }
        public ICommand DeleteLinkAppCommand { get; }

        #endregion Command

        public event EventHandler ChangeLinkageAppEvent;

        /// <summary>
        /// Application configuration manager
        /// </summary>
        private static readonly AppConfigManager AppConfigManager = AppConfigManager.GetInstance();

        /// <summary>
        /// Constructor
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new DelegateCommand(LinkAppReferenceButtonClicked);
            RegisterLinkAppCommand  = new DelegateCommand(RegisterLinkAppButtonClicked);
            DeleteLinkAppCommand    = new DelegateCommand<ExtraAppSetting>(DeleteLinkAppButtonClicked);

            var linkageAppList = AppConfigManager.ConfigData.LinkageAppList;
            if (linkageAppList == null || !linkageAppList.Any())
            {
                return;
            }

            LinkageAppList.Clear();
            LinkageAppList.AddRange(linkageAppList);
        }

        /// <summary>
        /// Event when the reference button is pressed.
        /// </summary>
        private void LinkAppReferenceButtonClicked()
        {
            var previousLinkAppPath = LinkAppPath;

            const string DialogTitle = "Linked application selection";
            const string DialogDefaultExt = ".exe";

            var dialog = new OpenFileDialog
            {
                Title = DialogTitle,
                DefaultExt = DialogDefaultExt,
                InitialDirectory = Environment.GetFolderPath(Environment.Is64BitProcess
                    ? Environment.SpecialFolder.ProgramFiles
                    : Environment.SpecialFolder.ProgramFilesX86)
            };


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
            if (LinkageAppList.Count > MaxLinkAppNum || string.IsNullOrEmpty(LinkAppPath))
            {
                return;
            }

            var linkageApp = new ExtraAppSetting { AppName = Path.GetFileNameWithoutExtension(LinkAppPath), AppPath = LinkAppPath };
            if (LinkageAppList.Any(x => x.AppName == linkageApp.AppName || x.AppPath == linkageApp.AppPath))
            {
                return;
            }

            LinkageAppList.Add(linkageApp);

            // Export information to Configure file.
            AppConfigManager.AddLinkageApp(linkageApp);
            AppConfigManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            LinkAppPath = "";
        }

        /// <summary>
        /// Event when delete button is pressed.
        /// </summary>
        private void DeleteLinkAppButtonClicked(ExtraAppSetting deleteAppSetting)
        {
            if (LinkageAppList == null || !LinkageAppList.Any() || deleteAppSetting == null)
            {
                return;
            }

            if (LinkageAppList.All(x => x != deleteAppSetting))
            {
                return;
            }
            LinkageAppList.Remove(deleteAppSetting);

            // Export information to configure file.
            AppConfigManager.RemoveLinkageApp(deleteAppSetting);
            AppConfigManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}