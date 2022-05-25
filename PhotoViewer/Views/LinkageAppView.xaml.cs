using Kchary.PhotoViewer.ViewModels;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for LinkageAppView.xaml
    /// </summary>
    public partial class LinkageAppView
    {
        public LinkageAppView()
        {
            InitializeComponent();
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as LinkageAppViewModel;
            vm?.Dispose();
        }
    }
}