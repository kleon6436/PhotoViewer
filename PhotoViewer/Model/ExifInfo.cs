using Prism.Mvvm;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ExifInfo : BindableBase
    {
        private string exifParameterText;

        /// <summary>
        /// Exif parameter name
        /// </summary>
        public string ExifParameterText
        {
            get => exifParameterText;
            set => SetProperty(ref exifParameterText, value);
        }

        private string exifParameterValue;

        /// <summary>
        /// Exif parameter value
        /// </summary>
        public string ExifParameterValue
        {
            get => exifParameterValue;
            set => SetProperty(ref exifParameterValue, value);
        }

        public ExifInfo(string exifText, string exifValue)
        {
            ExifParameterText = exifText;
            ExifParameterValue = exifValue;
        }
    }
}