using System;
using System.Windows.Controls;
using Prism.Mvvm;
using PhotoViewer.Views;

namespace PhotoViewer.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        public enum SelectPage
        {
            LinkageAppPage,
            InformationPage,
        }

        public event EventHandler ReloadContextMenuEvent;

        private SelectPage selectPageButtonValue;
        /// <summary>
        /// ラジオボタンで選択されているページ
        /// </summary>
        public SelectPage SelectPageButtonValue
        {
            get { return selectPageButtonValue; }
            set 
            {
                SetProperty(ref selectPageButtonValue, value);
                
                switch (selectPageButtonValue)
                {
                    case SelectPage.LinkageAppPage:
                        var vm = new LinkageAppViewModel();
                        vm.ChangeLinkageAppEvent += ChangeLinkageApp;

                        DisplayPage = new LinkageAppView();
                        DisplayPage.DataContext = vm;
                        break;

                    case SelectPage.InformationPage:
                        DisplayPage = new InformationView();
                        break;

                    default:
                        DisplayPage = null;
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private Page displayPage;
        /// <summary>
        /// 表示するページ情報
        /// </summary>
        public Page DisplayPage
        {
            get { return displayPage; }
            set { SetProperty(ref displayPage, value); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingViewModel()
        {
            // デフォルトのページ設定
            SelectPageButtonValue = SelectPage.LinkageAppPage;
        }

        /// <summary>
        /// 連携アプリの状態が変更されたとき
        /// </summary>
        /// <param name="sender">LinkageAppViewModel</param>
        /// <param name="e">引数情報</param>
        private void ChangeLinkageApp(object sender, EventArgs e)
        {
            var linkageAppVM = sender as LinkageAppViewModel;
            if (linkageAppVM == null) return;

            ReloadContextMenuEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
