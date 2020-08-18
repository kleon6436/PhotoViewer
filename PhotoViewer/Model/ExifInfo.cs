using Prism.Mvvm;

namespace PhotoViewer.Model
{
    public class ExifInfo : BindableBase
    {
        private string exifParameterText;

        /// <summary>
        /// Exifのパラメータ名
        /// </summary>
        public string ExifParameterText
        {
            get { return exifParameterText; }
            set { SetProperty(ref exifParameterText, value); }
        }

        private string exifParameterValue;

        /// <summary>
        /// Exifのパラメータ値
        /// </summary>
        public string ExifParameterValue
        {
            get { return exifParameterValue; }
            set { SetProperty(ref exifParameterValue, value); }
        }

        public ExifInfo(string exifText, string exifValue)
        {
            this.ExifParameterText = exifText;
            this.ExifParameterValue = exifValue;
        }
    }
}