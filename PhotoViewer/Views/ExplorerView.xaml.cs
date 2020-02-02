using PhotoViewer.Model;
using PhotoViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoViewer.Views
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
            var treeView = sender as TreeView;
            if (treeView == null) return;

            var selectedExplorerItem = treeView.SelectedItem as ExplorerItem;
            if (selectedExplorerItem == null) return;

            var vm = this.DataContext as ExplorerViewModel;
            Debug.Assert(vm != null);

            if (vm.SelectedItem != selectedExplorerItem)
            {
                vm.SelectedItem = selectedExplorerItem;
            }
        }
    }
}
