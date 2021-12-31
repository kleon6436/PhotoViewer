using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using Kchary.PhotoViewer.Views;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class SettingViewModel : BindableBase
    {
        private readonly CompositeDisposable disposable = new();
        private ReactivePropertySlim<SelectPage> selectPageButtonValue;

        /// <summary>
        /// コンテキストメニューの再読み込みイベント
        /// </summary>
        public event EventHandler ReloadContextMenuEvent;

        /// <summary>
        /// 読み込むページのEnum
        /// </summary>
        public enum SelectPage
        {
            LinkageAppPage,
            InformationPage,
        }

        /// <summary>
        /// ラジオボタンで設定された読み込むページの情報
        /// </summary>
        public ReactivePropertySlim<SelectPage> SelectPageButtonValue
        {
            get
            {
                if (selectPageButtonValue == null)
                {
                    return selectPageButtonValue ??= new ReactivePropertySlim<SelectPage>().AddTo(disposable);
                }

                switch (selectPageButtonValue.Value)
                {
                    case SelectPage.LinkageAppPage:
                        var vm = new LinkageAppViewModel();
                        vm.ChangeLinkageAppEvent += ChangeLinkageApp;
                        DisplayPage.Value = new LinkageAppView
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
        }

        /// <summary>
        /// 表示する画面
        /// </summary>
        public ReactivePropertySlim<Page> DisplayPage { get; set; } = new();

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
            if (sender is not LinkageAppViewModel)
            {
                return;
            }

            ReloadContextMenuEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}