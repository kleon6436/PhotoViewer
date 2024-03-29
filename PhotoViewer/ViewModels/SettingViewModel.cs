﻿using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    /// <summary>
    /// 読み込むページのEnum
    /// </summary>
    public enum SelectPage
    {
        LinkageAppPage,
        InformationPage,
    }

    public sealed class SettingViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// IDisposableをまとめるCompositeDisposable
        /// </summary>
        private readonly CompositeDisposable disposable = [];

        /// <summary>
        /// コンテキストメニューの再読み込みイベント
        /// </summary>
        public EventHandler ReloadContextMenuEvent { get; set; }

        /// <summary>
        /// 表示する画面
        /// </summary>
        public ReactivePropertySlim<Page> DisplayPage { get; }

        private ReactivePropertySlim<SelectPage> selectPageButtonValue;
        /// <summary>
        /// ラジオボタンで設定された読み込むページの情報
        /// </summary>
        public ReactivePropertySlim<SelectPage> SelectPageButtonValue
        {
            get
            {
                switch (selectPageButtonValue.Value)
                {
                    case SelectPage.LinkageAppPage:
                        var vm = new RegisterAppViewModel();
                        vm.ChangeLinkageAppEvent += ChangeLinkageApp;
                        DisplayPage.Value = new RegisterAppView
                        {
                            DataContext = vm
                        };
                        break;

                    case SelectPage.InformationPage:
                        DisplayPage.Value = new InformationView();
                        break;

                    default:
                        DisplayPage.Value = null;
                        throw new ArgumentOutOfRangeException(nameof(selectPageButtonValue.Value), "Invalid name");
                }
                return selectPageButtonValue ??= new ReactivePropertySlim<SelectPage>().AddTo(disposable);
            }
            private set => selectPageButtonValue = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingViewModel()
        {
            DisplayPage = new ReactivePropertySlim<Page>().AddTo(disposable);
            SelectPageButtonValue = new ReactivePropertySlim<SelectPage>().AddTo(disposable);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => disposable.Dispose();

        /// <summary>
        /// 登録アプリ変更時の処理
        /// </summary>
        /// <param name="sender">LinkageAppViewModel</param>
        /// <param name="e">引数情報</param>
        private void ChangeLinkageApp(object sender, EventArgs e)
        {
            if (sender is not RegisterAppViewModel)
            {
                return;
            }

            ReloadContextMenuEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}