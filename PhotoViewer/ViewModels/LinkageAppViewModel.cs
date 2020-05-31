using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using PhotoViewer.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace PhotoViewer.ViewModels
{
    public class LinkageAppViewModel : BindableBase
    {
        private readonly int MAX_LINK_APP_NUM = 10;

        #region UI binding parameter
        
        private string linkAppPath;
        public string LinkAppPath
        {
            get { return linkAppPath; }
            set { SetProperty(ref linkAppPath, value); }
        }

        public ObservableCollection<ExtraAppSetting> LinkageAppList { get; } = new ObservableCollection<ExtraAppSetting>();

        #endregion

        #region Command
        public ICommand LinkAppReferenceCommand { get; private set; }
        public ICommand RegisterLinkAppCommand { get; private set; }
        public ICommand DeleteLinkAppCommand { get; private set; }
        #endregion

        public event EventHandler ChangeLinkageAppEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new DelegateCommand(LinkAppReferenceButtonClicked);
            RegisterLinkAppCommand = new DelegateCommand(RegisterLinkAppButtonClicked);
            DeleteLinkAppCommand = new DelegateCommand<ExtraAppSetting>(DeleteLinkAppButtonClicked);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var linkageAppList = appConfigManager.configData.LinkageAppList;
            if (linkageAppList != null && linkageAppList.Count > 0)
            {
                LinkageAppList.Clear();
                LinkageAppList.AddRange(linkageAppList);
            }
        }

        /// <summary>
        /// 参照ボタンを押下時
        /// </summary>
        private void LinkAppReferenceButtonClicked()
        {
            string previousLinkAppPath = LinkAppPath;

            var dialog = new OpenFileDialog();
            dialog.Title = "連携アプリ選択ダイアログ";
            dialog.DefaultExt = ".exe";

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
        /// 登録ボタンを押下時
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

            // Configファイルに情報を書き出し
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.SetLinkageApp(LinkageAppList.ToList());
            appConfigManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            LinkAppPath = "";
        }

        /// <summary>
        /// 削除ボタンを押下時
        /// </summary>
        private void DeleteLinkAppButtonClicked(ExtraAppSetting deleteAppSetting)
        {
            // 登録済みではない場合は、何もせず終了
            if (LinkageAppList == null || LinkageAppList.Count == 0 || deleteAppSetting == null)
            {
                return;
            }

            if (LinkageAppList.Any(x => x == deleteAppSetting))
            {
                LinkageAppList.Remove(deleteAppSetting);

                // Configファイルに情報を書き出し
                AppConfigManager appConfigManager = AppConfigManager.GetInstance();
                appConfigManager.RemoveLinkageApp(LinkageAppList.ToList());
                appConfigManager.Export();

                ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
