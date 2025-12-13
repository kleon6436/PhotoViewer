using Kchary.PhotoViewer.ViewModels;
using Microsoft.UI.Xaml;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public sealed partial class SettingView : Window
    {
        public SettingView()
        {
            this.InitializeComponent();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            var vm = DataContext as SettingViewModel;
            vm?.Dispose();
        }
    }
}