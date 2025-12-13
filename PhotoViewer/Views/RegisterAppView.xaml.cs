using Kchary.PhotoViewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for RegisterAppView.xaml
    /// </summary>
    public sealed partial class RegisterAppView : Page
    {
        public RegisterAppView()
        {
            this.InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as RegisterAppViewModel;
            vm?.Dispose();
        }
    }
}