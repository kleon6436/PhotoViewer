namespace PhotoViewer.Model
{
    public sealed class ImageForm
    {
        public enum ImageForms
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
        }

        public string Name { get; set; }
        public ImageForms Form { get; set; }
    }
}