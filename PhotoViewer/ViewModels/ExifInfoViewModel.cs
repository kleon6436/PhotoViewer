using PhotoViewer.Model;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace PhotoViewer.ViewModels
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
            ExifDataList.Clear();
            ExifDataList.AddRange(ExifParser.GetExifDataFromFile(filePath));
        }
    }
}