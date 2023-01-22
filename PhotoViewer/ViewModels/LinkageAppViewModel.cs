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
    public sealed class LinkageAppViewModel : BindableBase, IDisposable
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
        public ReactivePropertySlim<string> LinkAppPath { get; } = new();

        /// <summary>
        /// 登録アプリリスト
        /// </summary>
        public ObservableCollection<ExtraAppSetting> LinkageAppList { get; } = new();

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
        public ReactiveCommand<ExtraAppSetting> DeleteLinkAppCommand { get; }

        #endregion Command

        /// <summary>
        /// 登録アプリを変更した場合のイベント
        /// </summary>
        public EventHandler ChangeLinkageAppEvent { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinkageAppViewModel()
        {
            LinkAppReferenceCommand = new ReactiveCommand().WithSubscribe(LinkAppReferenceButtonClicked).AddTo(disposables);
            RegisterLinkAppCommand = new ReactiveCommand().WithSubscribe(RegisterLinkAppButtonClicked).AddTo(disposables);
            DeleteLinkAppCommand = new ReactiveCommand<ExtraAppSetting>().WithSubscribe(DeleteLinkAppButtonClicked).AddTo(disposables);

            var linkageAppList = AppConfig.GetInstance().GetAvailableLinkageApps();
            if (linkageAppList?.Any() != true)
            {
                return;
            }

            LinkageAppList.Clear();
            LinkageAppList.AddRange(linkageAppList);
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
            var previousLinkAppPath = LinkAppPath.Value;

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
                LinkAppPath.Value = previousLinkAppPath;
                return;
            }

            LinkAppPath.Value = dialog.FileName;
        }

        /// <summary>
        /// 登録ボタンを押下時の処理
        /// </summary>
        private void RegisterLinkAppButtonClicked()
        {
            if (LinkageAppList.Count > MaxLinkAppNum || !FileUtil.CheckFilePath(LinkAppPath.Value))
            {
                return;
            }

            var linkageApp = new ExtraAppSetting
            {
                AppName = FileUtil.GetFileName(LinkAppPath.Value, true),
                AppPath = LinkAppPath.Value
            };
            if (LinkageAppList.Any(x => x.AppName == linkageApp.AppName || x.AppPath == linkageApp.AppPath))
            {
                return;
            }

            LinkageAppList.Add(linkageApp);

            var applicationManager = AppConfig.GetInstance();
            applicationManager.AddLinkageApp(linkageApp);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
            LinkAppPath.Value = "";
        }

        /// <summary>
        /// 削除ボタンを押下時の処理
        /// </summary>
        private void DeleteLinkAppButtonClicked(ExtraAppSetting deleteAppSetting)
        {
            if (LinkageAppList?.Any() != true || deleteAppSetting == null)
            {
                return;
            }

            if (LinkageAppList.All(x => x != deleteAppSetting))
            {
                return;
            }
            LinkageAppList.Remove(deleteAppSetting);

            var applicationManager = AppConfig.GetInstance();
            applicationManager.RemoveLinkageApp(deleteAppSetting);
            applicationManager.Export();

            ChangeLinkageAppEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}