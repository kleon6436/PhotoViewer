namespace Kchary.PhotoViewer.Model
{
    public sealed class ImageForm
    {
        /// <summary>
        /// Image type enum
        /// </summary>
        public enum ImageForms
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
        }

        /// <summary>
        /// Image type name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Image type
        /// </summary>
        public ImageForms Form { get; init; }
    }
}