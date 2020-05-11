using System.Collections.ObjectModel;
using Prism.Mvvm;
using PhotoViewer.Model;

namespace PhotoViewer.ViewModels
{
    public class ExifInfoViewModel : BindableBase
    {
        /// <summary>
        /// Exif情報のリスト
        /// </summary>
        public ObservableCollection<ExifInfo> ExifDataList { get; } = new ObservableCollection<ExifInfo>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExifInfoViewModel()
        {

        }

        /// <summary>
        /// Exif情報を設定する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public void SetExif(string filePath)
        {
            ExifDataList.Clear();
            ExifDataList.AddRange(ExifParser.GetExifDataFromFile(filePath));
        }
    }
}
