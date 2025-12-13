using Kchary.PhotoViewer.Models;
using Kchary.PhotoViewer.ViewModels;
using Microsoft.UI.Xaml;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExifDeleteToolView.xaml
    /// </summary>
    public sealed partial class ImageEditToolView : Window
    {
        public ImageEditToolView()
        {
            this.InitializeComponent();

            DataContextChanged += (_, _) =>
            {
                if (DataContext is ResizeImageViewModel vm)
                {
                    vm.CloseView += (_, _) => Close();
                }
            };
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender">ImageEditWindow</param>
        /// <param name="e">引数情報</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 保存形式コンボボックスの選択状態が変化したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is not ResizeImageViewModel vm)
            {
                return;
            }

            // 保存形式に応じて、品質設定の表示を切り替え
            vm.IsEnableImageSaveQuality.Value = vm.SelectedForm.Value.Form == FileExtensionType.Jpeg;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            var vm = DataContext as ResizeImageViewModel;
            vm?.Dispose();
        }
    }
}