namespace PhotoViewer.Model
{
    public class ImageQuality
    {
        public string Name { get; private set; }
        public int QualityValue { get; private set; }

        public ImageQuality(string name, int qualityValue)
        {
            this.Name = name;
            this.QualityValue = qualityValue;
        }
    }
}