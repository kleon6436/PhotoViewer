using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExifInfoViewModel : BindableBase
    {
        /// <summary>
        /// Exif情報データを保持するリスト
        /// </summary>
        public ObservableCollection<ExifInfo> ExifDataList { get; } = new();

        /// <summary>
        /// Exif情報を設定する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        public void SetExif(string filePath)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ExifDataList.Clear();
                ExifDataList.AddRange(ExifParser.GetExifDataFromFile(filePath));
            });
        }
    }
}