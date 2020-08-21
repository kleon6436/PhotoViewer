namespace PhotoViewer.Model
{
    public class ImageQuality
    {
        public string Name { get; private set; }
        public int QualityValue { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">表示名</param>
        /// <param name="qualityValue">品質値</param>
        public ImageQuality(string name, int qualityValue)
        {
            this.Name = name;
            this.QualityValue = qualityValue;
        }
    }
}