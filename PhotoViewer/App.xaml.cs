using Microsoft.UI.Xaml;
using Reactive.Bindings;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kchary.PhotoViewer
{
    public partial class App : Application
    {
        private static Mutex Mutex = new(false, "PhotoViewer");
        private Window m_window;

        internal static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool IsWindowVisible(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);
        }

        public App()
        {
            this.InitializeComponent();
            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// ログ出力する
        /// </summary>
        /// <param name="ex">The message of exception.</param>
        /// <param name="callerFilePath">CallerFilePath</param>
        /// <param name="callerLineNumber">CallerLineNumber</param>
        public static void LogException(Exception ex,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            Debug.WriteLine($"ERROR -> {ex.Message}, LineNumber: {callerLineNumber}, FilePath: {callerFilePath}");
        }

        /// <summary>
        /// エラーメッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static async void ShowErrorMessageBox(string message, string caption)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = caption,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = (Current as App)?.m_window?.Content?.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// 成功メッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static async void ShowSuccessMessageBox(string message, string caption)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = caption,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = (Current as App)?.m_window?.Content?.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// マウスをブロックした状態で処理を実行する
        /// </summary>
        /// <remarks>
        /// エラー時は、エラーメッセージボックスを表示する
        /// </remarks>
        /// <param name="action">処理</param>
        /// <param name="errorCaption">エラー時のキャプション</param>
        /// <param name="errorMessage">エラー時のメッセージ</param>
        public static void CallMouseBlockMethod(Action action, string errorCaption, string errorMessage)
        {
            try
            {
                // WinUIでは Window.Current.CoreWindow.PointerCursor を使用
                var window = (Current as App)?.m_window;
                if (window?.Content?.XamlRoot != null)
                {
                    window.Content.XamlRoot.Content.ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Wait);
                }
                action.Invoke();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ShowErrorMessageBox(errorMessage, errorCaption);
            }
            finally
            {
                var window = (Current as App)?.m_window;
                if (window?.Content?.XamlRoot != null)
                {
                    window.Content.XamlRoot.Content.ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
                }
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            ReactivePropertyScheduler.SetDefault(new ReactivePropertySynchronizationContextScheduler(SynchronizationContext.Current));

            if (Mutex.WaitOne(0, false))
            {
                m_window = new Views.MainWindow();
                m_window.Activate();
                return;
            }

            var hMWnd = NativeMethods.FindWindow(null, "PhotoViewer");
            if (!NativeMethods.IsWindow(hMWnd))
            {
                return;
            }

            var hCWnd = NativeMethods.GetLastActivePopup(hMWnd);
            if (!NativeMethods.IsWindow(hCWnd) || !NativeMethods.IsWindowVisible(hCWnd))
            {
                return;
            }

            _ = NativeMethods.ShowWindow(hCWnd, 1);
            NativeMethods.SetForegroundWindow(hCWnd);

            Mutex.Close();
            Mutex = null;
            Exit();
        }

        /// <summary>
        /// アプリケーションのハンドルされていないExceptionが発生したとき
        /// </summary>
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            ShowErrorMessageBox("File access error occurred", "File access error");
            e.Handled = true;
        }
    }
}