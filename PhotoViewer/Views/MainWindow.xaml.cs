using Kchary.PhotoViewer.Model;
using Kchary.PhotoViewer.ViewModels;
using Kchary.PhotoViewer.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Kchary.PhotoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MIN_SPL_TIME = 1000;

        public MainWindow()
        {
            // SplashScreenの表示
            var splashScreen = new SplashScreenView();
            splashScreen.Show();

            // SplashScreen表示中にViewModelの読み込み
            var timer = new Stopwatch();
            timer.Start();
            var vm = new MainWindowViewModel();
            vm.InitViewFolder();
            DataContext = vm;
            timer.Stop();

            // 一定時間待機後、SplashScreenを閉じる
            if (MIN_SPL_TIME - timer.ElapsedMilliseconds > 0)
            {
                Thread.Sleep(MIN_SPL_TIME);
            }
            splashScreen.Close();

            InitializeComponent();
        }

        /// <summary>
        /// ウィンドウオープン処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedMedia == null && vm.MediaInfoList.Any())
            {
                MediaInfo firstImageData = vm.MediaInfoList.First();
                if (!MediaChecker.CheckNikonRawFileExtension(Path.GetExtension(firstImageData.FilePath).ToLower()))
                {
                    vm.SelectedMedia = firstImageData;
                }
            }
        }

        /// <summary>
        /// ウィンドウクローズ処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (!vm.StopThreadAndTask())
            {
                // 少し待ってからクローズ
                Thread.Sleep(200);
            }

            // ウィンドウ情報を保存
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            GetWindowPlacement(hwnd, out WINDOWPLACEMENT placement);

            var appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.ConfigData.WindowPlaceData = placement;
            appConfigManager.Export();
        }

        /// <summary>
        /// ウィンドウサイズ切り替え時の処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ListBoxのアイテム選択時は、そのアイテムまでスクロールする
            var selectedItem = mediaListBox.SelectedItem;
            if (selectedItem != null)
            {
                mediaListBox.ScrollIntoView(selectedItem);
            }

            // Run GC.
            App.RunGC();
        }

        /// <summary>
        /// リストボックスで選択されたアイテムが変更されたとき
        /// </summary>
        /// <param name="sender">mediaListBox</param>
        /// <param name="e">引数情報</param>
        private async void MediaListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
            {
                return;
            }

            if (listBox.SelectedItem is not MediaInfo mediaInfo)
            {
                return;
            }

            var vm = DataContext as MainWindowViewModel;
            await vm.LoadMediaAsync(mediaInfo);
        }

        /// <summary>
        /// コンテキストメニューがクリックされたとき
        /// </summary>
        /// <param name="sender">MenuItem</param>
        /// <param name="e">引数情報</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
            {
                return;
            }

            var vm = DataContext as MainWindowViewModel;
            vm.ExecuteContextMenu(Convert.ToString(menuItem.Header));
        }

        /// <summary>
        /// ウィンドウ表示時
        /// </summary>
        /// <param name="e">引数情報</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var appConfigManager = AppConfigManager.GetInstance();

            WINDOWPLACEMENT windowPlacement = appConfigManager.ConfigData.WindowPlaceData;
            windowPlacement.showCmd = (windowPlacement.showCmd == SW.SHOWMINIMIZED) ? SW.SHOWNORMAL : windowPlacement.showCmd;

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowPlacement(hwnd, ref windowPlacement);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public SW showCmd;
            public POINT minPosition;
            public POINT maxPosition;
            public RECT normalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum SW
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
        }

        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
    }
}