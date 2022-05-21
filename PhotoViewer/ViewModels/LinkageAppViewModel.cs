using Kchary.PhotoViewer.Models;
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
    public sealed class LinkageAppViewModel : BindableBase
    {
        /// <summary>
        /// 登録アプリ数の最大値
        /// </summary>
        private const int MaxLinkAppNum = 10;

        #region UI binding parameter

        private string linkAppPath;

        /// <summary>
        /// 登録アプリのパス
        /// </summary>
        public string LinkAppPath
        {
            get => linkAppPath;
            set => SetProperty(ref linkAppPath, value);
        }

        /// <summary>
        /// 登録アプリリスト
        /// </summary>
        public ObservableCollection<ExtraAppSetting> LinkageAppList { get; } = new();

        #endregion UI binding parameter

        #region Command

        /// <summary>
        /// 参照ボタンのコマンド
        /// </summary>
        public ICommand LinkAppReferenceCommand { get; }
        
        /// <summary>
        /// 登録ボタンのコマンド
        /// </summary>
        public ICommand RegisterLinkAppCommand { get; }
        
        /// <summary>
        /// 削除ボタンのコマンド
        /// </summary>
        public ICommand DeleteLinkAppCommand { get; }

        #endregion Command

        /// <summary>
        /// 登録アプリを変更した場合のイベント
        /// </summary>
        public event EventHandler ChangeLinkageAppEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new DelegateCommand(LinkAppReferenceButtonClicked);
            RegisterLinkAppCommand  = new DelegateCommand(RegisterLinkAppButtonClicked);
            DeleteLinkAppCommand    = new DelegateCommand<ExtraAppSetting>(DeleteLinkAppButtonClicked);

            var linkageAppList = AppConfigManager.GetInstance().ConfigData.LinkageAppList;
            if (linkageAppList == null || !linkageAppList.Any())
            {
                return;
            }

            LinkageAppList.Clear();
            LinkageAppList.AddRange(linkageAppList);
        }

        /// <summary>
        /// 参照ボタンを押下時の処理
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
        /// 登録ボタンを押下時の処理
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

            var applicationManager = AppConfigManager.GetInstance();
            applicationManager.AddLinkageApp(linkageApp);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            LinkAppPath = "";
        }

        /// <summary>
        /// 削除ボタンを押下時の処理
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

            var applicationManager = AppConfigManager.GetInstance();
            applicationManager.RemoveLinkageApp(deleteAppSetting);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}