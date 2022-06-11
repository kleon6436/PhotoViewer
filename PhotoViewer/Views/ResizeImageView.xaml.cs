using Kchary.PhotoViewer.Data;
using Kchary.PhotoViewer.ViewModels;
using System.Windows;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExifDeleteToolView.xaml
    /// </summary>
    public partial class ImageEditToolView
    {
        public ImageEditToolView()
        {
            InitializeComponent();

            DataContextChanged += (_, _) =>
            {
                if (DataContext is ResizeImageViewModel vm)
                {
                    vm.CloseView += (_, _) =>
                    {
                        Close();
                    };
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
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is not ResizeImageViewModel vm)
            {
                return;
            }

            // 保存形式に応じて、品質設定の表示を切り替え
            vm.IsEnableImageSaveQuality.Value = vm.SelectedForm.Value.Form == ImageForm.ImageForms.Jpeg;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as ResizeImageViewModel;
            vm?.Dispose();
        }
    }
}