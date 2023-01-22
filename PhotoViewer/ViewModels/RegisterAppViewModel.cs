using Kchary.PhotoViewer.Helpers;
using Kchary.PhotoViewer.Models;
using Microsoft.Win32;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class RegisterAppViewModel : BindableBase, IDisposable
    {
        /// <summary>
        /// 登録アプリ数の最大値
        /// </summary>
        private const int MaxLinkAppNum = 10;

        /// <summary>
        /// IDisposableをまとめるCompositeDisposable
        /// </summary>
        private readonly CompositeDisposable disposables = new();

        #region UI binding parameter

        /// <summary>
        /// 登録アプリのパス
        /// </summary>
        public ReactivePropertySlim<string> RegisterAppPath { get; } = new();

        /// <summary>
        /// 登録アプリリスト
        /// </summary>
        public ObservableCollection<RegisterApp> RegisterAppList { get; } = new();

        #endregion UI binding parameter

        #region Command

        /// <summary>
        /// 参照ボタンのコマンド
        /// </summary>
        public ReactiveCommand LinkAppReferenceCommand { get; }

        /// <summary>
        /// 登録ボタンのコマンド
        /// </summary>
        public ReactiveCommand RegisterLinkAppCommand { get; }

        /// <summary>
        /// 削除ボタンのコマンド
        /// </summary>
        public ReactiveCommand<RegisterApp> DeleteLinkAppCommand { get; }

        #endregion Command

        /// <summary>
        /// 登録アプリを変更した場合のイベント
        /// </summary>
        public EventHandler ChangeLinkageAppEvent { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RegisterAppViewModel()
        {
            LinkAppReferenceCommand = new ReactiveCommand().WithSubscribe(LinkAppReferenceButtonClicked).AddTo(disposables);
            RegisterLinkAppCommand = new ReactiveCommand().WithSubscribe(RegisterLinkAppButtonClicked).AddTo(disposables);
            DeleteLinkAppCommand = new ReactiveCommand<RegisterApp>().WithSubscribe(DeleteLinkAppButtonClicked).AddTo(disposables);

            var registerAppList = AppConfig.GetInstance().GetAvailableRegisterApps();
            if (registerAppList?.Any() != true)
            {
                return;
            }

            RegisterAppList.Clear();
            RegisterAppList.AddRange(registerAppList);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => disposables.Dispose();

        /// <summary>
        /// 参照ボタンを押下時の処理
        /// </summary>
        private void LinkAppReferenceButtonClicked()
        {
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

            var previousRegisterAppPath = RegisterAppPath.Value;
            if (dialog.ShowDialog() != true)
            {
                RegisterAppPath.Value = previousRegisterAppPath;
                return;
            }

            RegisterAppPath.Value = dialog.FileName;
        }

        /// <summary>
        /// 登録ボタンを押下時の処理
        /// </summary>
        private void RegisterLinkAppButtonClicked()
        {
            if (RegisterAppList.Count > MaxLinkAppNum || !FileUtil.CheckFilePath(RegisterAppPath.Value))
            {
                return;
            }

            var linkageApp = new RegisterApp
            {
                AppName = FileUtil.GetFileName(RegisterAppPath.Value, true),
                AppPath = RegisterAppPath.Value
            };
            if (RegisterAppList.Any(x => x.AppName == linkageApp.AppName || x.AppPath == linkageApp.AppPath))
            {
                return;
            }

            RegisterAppList.Add(linkageApp);

            var applicationManager = AppConfig.GetInstance();
            applicationManager.AddRegisterApp(linkageApp);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            RegisterAppPath.Value = "";
        }

        /// <summary>
        /// 削除ボタンを押下時の処理
        /// </summary>
        private void DeleteLinkAppButtonClicked(RegisterApp deleteAppSetting)
        {
            if (RegisterAppList?.Any() != true || deleteAppSetting == null)
            {
                return;
            }

            if (RegisterAppList.All(x => x != deleteAppSetting))
            {
                return;
            }

            RegisterAppList.Remove(deleteAppSetting);

            var applicationManager = AppConfig.GetInstance();
            applicationManager.RemoveRegisterApp(deleteAppSetting);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}