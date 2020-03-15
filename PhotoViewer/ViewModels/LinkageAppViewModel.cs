using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using PhotoViewer.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace PhotoViewer.ViewModels
{
    public class LinkageAppViewModel : BindableBase
    {
        #region UI binding parameter
        private string linkAppPath;
        public string LinkAppPath
        {
            get { return linkAppPath; }
            set { SetProperty(ref linkAppPath, value); }
        }
        #endregion

        #region Command
        public ICommand LinkAppReferenceCommand { get; private set; }
        public ICommand RegisterLinkAppCommand { get; private set; }
        public ICommand DeleteLinkAppCommand { get; private set; }
        #endregion

        public event EventHandler ChangeLinkageAppEvent;

        // 登録フラグ
        public bool IsRegistered { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new DelegateCommand(LinkAppReferenceButtonClicked);
            RegisterLinkAppCommand = new DelegateCommand(RegisterLinkAppButtonClicked);
            DeleteLinkAppCommand = new DelegateCommand(DeleteLinkAppButtonClicked);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            if (appConfigManager.configData.LinkageApp != null)
            {
                LinkAppPath = appConfigManager.configData.LinkageApp.AppPath;
                IsRegistered = true;
            }
            else
            {
                LinkAppPath = "";
                IsRegistered = false;
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
            var linkageApp = new ExtraAppSetting(1, Path.GetFileNameWithoutExtension(LinkAppPath), LinkAppPath);

            // Configファイルに情報を書き出し
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.SetLinkageApp(linkageApp);
            appConfigManager.Export();

            IsRegistered = true;
            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);

            App.ShowSuccessMessageBox("連携アプリの登録が完了しました", "登録完了");
        }

        /// <summary>
        /// 削除ボタンを押下時
        /// </summary>
        private void DeleteLinkAppButtonClicked()
        {
            // 登録済みではない場合は、何もせず終了
            if (!IsRegistered)
            {
                return;
            }

            // Configファイルに情報を書き出し
            LinkAppPath = "";
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.RemoveLinkageApp();
            appConfigManager.Export();

            IsRegistered = false;
            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);

            App.ShowSuccessMessageBox("連携アプリの削除が完了しました", "削除完了");
        }
    }
}
