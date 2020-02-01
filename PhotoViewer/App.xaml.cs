using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 例外発生時はコンソールにエラーメッセージを出力する
        /// </summary>
        /// <param name="_ex">例外時のメッセージ</param>
        public static void LogException(Exception ex,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            Console.WriteLine("ERROR -> " + ex.Message + ", LineNumber:" + callerLineNumber + ", FilePath:" + callerFilePath);
        }
    }
}
