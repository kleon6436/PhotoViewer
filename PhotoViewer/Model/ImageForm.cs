namespace PhotoViewer.Model
{
    public class ImageForm
    {
        public enum ImageForms
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
        }

        public string Name { get; private set; }
        public ImageForms Form { get; private set; }

        public ImageForm(string name, ImageForms form)
        {
            this.Name = name;
            this.Form = form;
        }
    }
}