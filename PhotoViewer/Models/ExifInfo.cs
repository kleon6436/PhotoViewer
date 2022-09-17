using Kchary.PhotoViewer.ViewModels;
using System.ComponentModel;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// Exif表示用クラス
    /// </summary>
    public sealed record ExifInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Exif情報のプロパティタイプ
        /// </summary>
        public PropertyType ExifPropertyType { get; init; }

        /// <summary>
        /// Exifパラメータ名
        /// </summary>
        public string ExifParameterText { get; init; }

        /// <summary>
        /// プロパティ変更ハンドラー
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string exifParameterValue;
        /// <summary>
        /// Exifパラメータ値
        /// </summary>
        public string ExifParameterValue
        {
            get => exifParameterValue;
            set
            {
                exifParameterValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExifParameterValue)));
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exifText">Exifパラメータ名</param>
        /// <param name="exifValue">Exifパラメータ値</param>
        /// <param name="propertyType">Exif情報のプロパティタイプ</param>
        public ExifInfo(string exifText, string exifValue, PropertyType propertyType)
        {
            ExifParameterText = exifText;
            ExifParameterValue = exifValue;
            ExifPropertyType = propertyType;
        }
    }
}