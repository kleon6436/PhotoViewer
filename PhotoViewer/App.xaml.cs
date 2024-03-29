﻿using Reactive.Bindings;
using Reactive.Bindings.Schedulers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Kchary.PhotoViewer
{
    public partial class App
    {
        private static Mutex Mutex = new(false, "PhotoViewer");

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
            DispatcherUnhandledException += AppDispatcherUnhandledException;
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
                Mouse.OverrideCursor = Cursors.Wait;
                action.Invoke();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ShowErrorMessageBox(errorMessage, errorCaption);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// アプリスタート時の処理
        /// </summary>
        /// <remarks>
        /// mutexを作成して多重起動を抑制する
        /// </remarks>
        /// <param name="sender">sender</param>
        /// <param name="e">引数情報</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ReactivePropertyScheduler.SetDefault(new ReactivePropertyWpfScheduler(Dispatcher));

            if (Mutex.WaitOne(0, false))
            {
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
            Shutdown();
        }

        /// <summary>
        /// アプリ終了時の処理
        /// </summary>
        /// <remarks>
        /// Release mutex to prevent multiple startup.
        /// </remarks>
        /// <param name="sender">sender</param>
        /// <param name="e">引数情報</param>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (Mutex == null)
            {
                return;
            }

            Mutex.ReleaseMutex();
            Mutex.Close();
        }

        /// <summary>
        /// アプリケーションのハンドルされていないExceptionが発生したとき
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">引数情報</param>
        private static void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            ShowErrorMessageBox("File access error occurred", "File access error");
        }
    }
}