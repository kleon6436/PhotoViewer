namespace Kchary.PhotoViewer.Model
{
    public sealed class ResizeImageCategory
    {
        /// <summary>
        /// Resize category enum
        /// </summary>
        public enum ResizeCategory
        {
            None,
            Print,
            Blog,
            Twitter,
        }

        /// <summary>
        /// Resize category name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Resize category
        /// </summary>
        public ResizeCategory Category { get; init; }

        /// <summary>
        /// Resize long side value
        /// </summary>
        public int ResizeLongSideValue { get; init; }
    }
}