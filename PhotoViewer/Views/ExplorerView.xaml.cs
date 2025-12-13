using Kchary.PhotoViewer.Models;
using Kchary.PhotoViewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kchary.PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public sealed partial class ExplorerView : UserControl
    {
        public ExplorerView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 選択項目変更時の処理
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">引数情報</param>
        private void TreeView_SelectionChanged(object sender, TreeViewSelectionChangedEventArgs e)
        {
            if ((sender as TreeView)?.SelectedItem is not ExplorerItem selectedExplorerItem)
            {
                return;
            }

            // 選択したアイテムをVMに通知
            if (DataContext is ExplorerViewModel vm && vm.SelectedItem != selectedExplorerItem)
            {
                vm.SelectedItem = selectedExplorerItem;
            }
        }
    }
}