﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PhotoViewer.Model;
using PhotoViewer.ViewModels;

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