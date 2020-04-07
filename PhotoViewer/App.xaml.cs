using System;
using System.Windows;

namespace PhotoViewer
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
            ShowErrorMessageBox("ファイルアクセスエラー", "エラー");
        }

        /// <summary>
        /// 例外発生時はコンソールにエラーメッセージを出力する
        /// </summary>
        /// <param name="ex">例外時のメッセージ</param>
        public static void LogException(Exception ex,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            Console.WriteLine("ERROR -> " + ex.Message + ", LineNumber:" + callerLineNumber + ", FilePath:" + callerFilePath);
        }

        /// <summary>
        /// エラーメッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static void ShowErrorMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 成功メッセージボックスを表示する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="caption">タイトル</param>
        public static void ShowSuccessMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// ガベージコレクションの明示的な呼び出し
        /// </summary>
        public static void RunGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
