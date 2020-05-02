using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using PhotoViewer.Model;
using PhotoViewer.ViewModels;
using PhotoViewer.Views;

namespace PhotoViewer
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
            this.DataContext = vm;
            timer.Stop();

            // 一定時間待機後、SplashScreenを閉じる
            if (MIN_SPL_TIME - (int)timer.ElapsedMilliseconds > 0)
            {
                Thread.Sleep(MIN_SPL_TIME);
            }
            splashScreen.Close();

            InitializeComponent();
        }

        /// <summary>
        /// ウィンドウロード後の処理
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">引数情報</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            vm.InitViewFolder();
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
            GetWindowPlacement(hwnd, out placement);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();
            appConfigManager.configData.WindowPlaceData = placement;
            appConfigManager.Export();
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
        /// ウィンドウ表示時
        /// </summary>
        /// <param name="e">引数情報</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            AppConfigManager appConfigManager = AppConfigManager.GetInstance();

            var windowPlacement = appConfigManager.configData.WindowPlaceData;
            windowPlacement.showCmd = (windowPlacement.showCmd == SW.SHOWMINIMIZED) ? SW.SHOWNORMAL : windowPlacement.showCmd;

            var hwnd = new WindowInteropHelper(this).Handle;
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
