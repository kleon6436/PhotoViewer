using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        DisplayPage = null;
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

        }
    }
}
