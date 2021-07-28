using Kchary.PhotoViewer.Views;
using Prism.Mvvm;
using System;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class SettingViewModel : BindableBase
    {
        private SelectPage selectPageButtonValue;
        private Page displayPage;

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
        public SelectPage SelectPageButtonValue
        {
            get => selectPageButtonValue;
            set
            {
                SetProperty(ref selectPageButtonValue, value);

                switch (selectPageButtonValue)
                {
                    case SelectPage.LinkageAppPage:
                        var vm = new LinkageAppViewModel();
                        vm.ChangeLinkageAppEvent += ChangeLinkageApp;

                        DisplayPage = new LinkageAppView
                        {
                            DataContext = vm
                        };
                        break;

                    case SelectPage.InformationPage:
                        DisplayPage = new InformationView();
                        break;

                    default:
                        DisplayPage = null;
                        throw new ArgumentOutOfRangeException(nameof(selectPageButtonValue), "Invalid name");
                }
            }
        }

        /// <summary>
        /// Page information to display
        /// </summary>
        public Page DisplayPage
        {
            get => displayPage;
            private set => SetProperty(ref displayPage, value);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingViewModel()
        {
            // デフォルトはリンク登録ページとする
            SelectPageButtonValue = SelectPage.LinkageAppPage;
        }

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