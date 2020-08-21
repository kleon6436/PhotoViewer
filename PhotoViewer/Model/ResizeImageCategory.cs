using System;

namespace PhotoViewer.Model
{
    public class ResizeImageCategory
    {
        public enum ResizeCategory
        {
            None,    // リサイズしない
            Print,   // 印刷用
            Blog,    // ブログ用
            Twitter, // Twitter用
        }

        public string Name { get; private set; }
        public ResizeCategory Category { get; private set; }
        public int ResizelongSideValue { get; private set; }

        public ResizeImageCategory(string name, ResizeCategory category)
        {
            this.Name = name;
            this.Category = category;

            switch (Category)
            {
                case ResizeCategory.None:
                    return;

                case ResizeCategory.Print:
                    ResizelongSideValue = 2500;
                    return;

                case ResizeCategory.Blog:
                    ResizelongSideValue = 1500;
                    return;

                case ResizeCategory.Twitter:
                    ResizelongSideValue = 1000;
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}