using CommunityToolkit.Mvvm.ComponentModel;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// Exif表示用クラス
    /// </summary>
    public sealed partial class ExifInfo : ObservableObject
    {
        /// <summary>
        /// Exif情報のプロパティタイプ
        /// </summary>
        public PropertyType ExifPropertyType { get; }

        /// <summary>
        /// Exifパラメータ名
        /// </summary>
        public string ExifParameterText { get; }

        /// <summary>
        /// Exifパラメータ値
        /// </summary>
        [ObservableProperty]
        private string exifParameterValue;

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