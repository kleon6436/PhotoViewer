using Kchary.PhotoViewer.ViewModels;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingView
    {
        public SettingView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as SettingViewModel;
            vm?.Dispose();
        }
    }
}