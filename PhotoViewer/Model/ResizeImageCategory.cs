using System;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ResizeImageCategory
    {
        public enum ResizeCategory
        {
            None,
            Print,
            Blog,
            Twitter,
        }

        public string Name { get; set; }
        public ResizeCategory Category { get; set; }
        public int ResizelongSideValue { get; set; }
    }
}