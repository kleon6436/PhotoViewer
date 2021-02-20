using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Kchary.PhotoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        internal class NativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll")]
            internal static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(IntPtr hWnd);
            [DllImport("user32.dll")]
            internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);
            [DllImport("user32.dll")]
            internal static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);
        }

        private Mutex mutex = new(false, "PhotoViewer");

        public App()
        {
            DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        /// <summary>
        /// Application startup event.
        /// </summary>
        /// <remarks>
        /// Set mutex to prevent multiple startup.
        /// </remarks>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                return;
            }

            // Active a launched windows.
            var hMWnd = NativeMethods.FindWindow(null, "PhotoViewer");
            if (!NativeMethods.IsWindow((hMWnd)))
            {
                return;
            }

            var hCWnd = NativeMethods.GetLastActivePopup(hMWnd);
            if (!NativeMethods.IsWindow(hCWnd) || !NativeMethods.IsWindowVisible(hCWnd))
            {
                return;
            }

            // Show normal window.
            _ = NativeMethods.ShowWindow(hCWnd, 1);
            NativeMethods.SetForegroundWindow(hCWnd);

            mutex.Close();
            mutex = null;
            Shutdown();
        }

        /// <summary>
        /// Application close event.
        /// </summary>
        /// <remarks>
        /// Release mutex to prevent multiple startup.
        /// </remarks>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (mutex == null)
            {
                return;
            }

            mutex.ReleaseMutex();
            mutex.Close();
        }

        /// <summary>
        /// Application Unhandled Exception
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        private static void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            ShowErrorMessageBox("File access error occurred", "File access error");
        }

        /// <summary>
        /// Output an error message to the console when an exception occurs.
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
        /// Display an error message box.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caption">Title</param>
        public static void ShowErrorMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Display am success message box.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caption">Title</param>
        public static void ShowSuccessMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Explicit call to garbage collection.
        /// </summary>
        public static void RunGc()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}