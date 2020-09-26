using Kchary.PhotoViewer.Model;
using Kchary.PhotoViewer.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        public ExplorerView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 選択項目変更時の処理
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">引数情報</param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender as TreeView == null)
            {
                return;
            }

            if (!((sender as TreeView).SelectedItem is ExplorerItem selectedExplorerItem))
            {
                return;
            }

            // スクロールして、フォーカスを当てる
            selectedExplorerItem.BringIntoView();
            selectedExplorerItem.Focus();

            ExplorerViewModel vm = DataContext as ExplorerViewModel;
            Debug.Assert(vm != null);

            if (vm.SelectedItem != selectedExplorerItem)
            {
                vm.SelectedItem = selectedExplorerItem;
            }
        }
    }
}