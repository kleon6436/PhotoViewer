using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// Exif表示用クラス
    /// </summary>
    public sealed class ExifInfo
    {
        /// <summary>
        /// Exifパラメータ名
        /// </summary>
        public string ExifParameterText { get; set; }

        /// <summary>
        /// Exifパラメータ値
        /// </summary>
        public string ExifParameterValue { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exifText">Exifパラメータ名</param>
        /// <param name="exifValue">Exifパラメータ値</param>
        public ExifInfo(string exifText, string exifValue)
        {
            ExifParameterText = exifText;
            ExifParameterValue = exifValue;
        }
    }
}