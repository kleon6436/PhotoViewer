using Kchary.PhotoViewer.Views;
using Prism.Mvvm;
using System;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
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
        /// Page selected by radio button
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
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private Page displayPage;

        /// <summary>
        /// Page information to display
        /// </summary>
        public Page DisplayPage
        {
            get => displayPage;
            set => SetProperty(ref displayPage, value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingViewModel()
        {
            // Default page setup
            SelectPageButtonValue = SelectPage.LinkageAppPage;
        }

        /// <summary>
        /// Event when the status of the linked application is changed.
        /// </summary>
        /// <param name="sender">LinkageAppViewModel</param>
        /// <param name="e">Argument</param>
        private void ChangeLinkageApp(object sender, EventArgs e)
        {
            if (!(sender is LinkageAppViewModel linkageAppVM))
            {
                return;
            }

            ReloadContextMenuEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}