using System.Windows;

namespace PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExifDeleteToolView.xaml
    /// </summary>
    public partial class ImageEditToolView : Window
    {
        public ImageEditToolView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender">ImageEditWindow</param>
        /// <param name="e">引数情報</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
