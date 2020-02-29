using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
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
        private void MediaListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        /// <summary>
        /// ウィンドウロード処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            var hwnd = new WindowInteropHelper(this).Handle;
            var windowPlacement = appConfigManager.configData.WindowPlaceData;
            WindowPlacement.SetWindowPlacement(hwnd, ref windowPlacement);
        }

        /// <summary>
        /// ウィンドウクローズ処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            if (!vm.StopThreadAndTask())
            {
                // 少し待ってからクローズ
                Thread.Sleep(200);
            }

            // ウィンドウ情報を保存
            WINDOWPLACEMENT placement;
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowPlacement.GetWindowPlacement(hwnd, out placement);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.configData.WindowPlaceData = placement;
            appConfigManager.Export();
        }
    }
}
