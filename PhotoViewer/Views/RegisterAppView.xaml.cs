using Kchary.PhotoViewer.ViewModels;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for RegisterAppView.xaml
    /// </summary>
    public partial class RegisterAppView
    {
        public RegisterAppView()
        {
            InitializeComponent();
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as RegisterAppViewModel;
            vm?.Dispose();
        }
    }
}