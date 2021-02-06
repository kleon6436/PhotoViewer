namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExifInfoView.xaml
    /// </summary>
    public partial class ExifInfoView
    {
        public ExifInfoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// マウスクリック時のイベント
        /// </summary>
        /// <param name="sender">DataGrid</param>
        /// <param name="e">引数情報</param>
        private void DataGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // マウスクリック時の動作をブロック
            e.Handled = true;
        }
    }
}