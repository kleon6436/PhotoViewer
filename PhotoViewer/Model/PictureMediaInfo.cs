namespace PhotoViewer.Model
{
    public class PictureMediaInfo : MediaInfo
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaInfo() : base(null)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PictureMediaInfo(MediaInfo _mediaFileInfo) : base(_mediaFileInfo)
        {
        }
    }
}