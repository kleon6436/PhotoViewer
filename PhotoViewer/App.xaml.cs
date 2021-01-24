using System;
using System.Diagnostics;
using System.Windows;

namespace Kchary.PhotoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        private void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            ShowErrorMessageBox("File access error occurred", "File access error");
        }

        /// <summary>
        /// Output an error message to the console when an exception occurs.
        /// </summary>
        /// <param name="ex">The message of exception.</param>
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
        public static void RunGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}