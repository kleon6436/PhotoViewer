using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExifInfoViewModel : BindableBase
    {
        /// <summary>
        /// List of exif information
        /// </summary>
        public ObservableCollection<ExifInfo> ExifDataList { get; } = new ObservableCollection<ExifInfo>();

        public ExifInfoViewModel()
        {
        }

        /// <summary>
        /// Set exif information.
        /// </summary>
        /// <param name="filePath">File path</param>
        public void SetExif(string filePath)
        {
            Application.Current.Dispatcher.Invoke((Action)(() => 
            {
                ExifDataList.Clear();
                ExifDataList.AddRange(ExifParser.GetExifDataFromFile(filePath));
            }));
        }
    }
}