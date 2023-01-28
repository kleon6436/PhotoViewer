using Kchary.PhotoViewer.ViewModels;
using Prism.Mvvm;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// Exif表示用クラス
    /// </summary>
    public sealed class ExifInfo : BindableBase
    {
        /// <summary>
        /// Exif情報のプロパティタイプ
        /// </summary>
        public PropertyType ExifPropertyType { get; }

        /// <summary>
        /// Exifパラメータ名
        /// </summary>
        public string ExifParameterText { get; }

        private string exifParameterValue;
        /// <summary>
        /// Exifパラメータ値
        /// </summary>
        public string ExifParameterValue
        {
            get => exifParameterValue;
            set => SetProperty(ref exifParameterValue, value);
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