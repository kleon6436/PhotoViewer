using System;
using System.Windows;
using System.Windows.Controls;
using PhotoViewer.Model;
using PhotoViewer.ViewModels;

namespace PhotoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var model = new MainWindowViewModel();
            this.DataContext = model;
        }

        /// <summary>
        /// リストボックスで選択されたアイテムが変更されたとき
        /// </summary>
        /// <param name="sender">mediaListBox</param>
        /// <param name="e">引数情報</param>
        private void mediaListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var mediaInfo = listBox.SelectedItem as MediaInfo;
            if (mediaInfo == null) return;

            var vm = this.DataContext as MainWindowViewModel;
            vm.LoadMedia(mediaInfo);
        }

        /// <summary>
        /// コンテキストメニューがクリックされたとき
        /// </summary>
        /// <param name="sender">MenuItem</param>
        /// <param name="e">引数情報</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var vm = this.DataContext as MainWindowViewModel;
            vm.ExecuteContextMenu(Convert.ToString(menuItem.Header));
        }
    }
}
