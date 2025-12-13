using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Graphics.Imaging;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 写真情報クラス
    /// </summary>
    public sealed partial class PhotoInfo : ObservableObject
    {
        #region Media Parameters

        /// <summary>
        /// サムネイル画像
        /// </summary>
        [ObservableProperty]
        private BitmapImage thumbnailImage;

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath { get; set; }

        #endregion Media Parameters

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public PhotoInfo(string filePath)
        {
            if (!FileUtil.CheckFilePath(filePath))
            {
                throw new FileNotFoundException();
            }

            FilePath = filePath;
            FileName = FileUtil.GetFileName(filePath, false);
        }

        /// <summary>
        /// サポート画像フラグ
        /// </summary>
        public bool IsSupportImage
        {
            get { return Const.SupportPictureExtensions.Any(supportExtension => supportExtension == FileExtension); }
        }

        /// <summary>
        /// RAW画像フラグ
        /// </summary>
        public bool IsRawImage
        {
            get { return Const.SupportRawPictureExtensions.Any(x => x == FileExtension); }
        }

        /// <summary>
        /// ファイルの拡張子
        /// </summary>
        public string FileExtension
        {
            get { return FileUtil.GetFileExtensions(FilePath); }
        }

        /// <summary>
        /// ファイル拡張子タイプ
        /// </summary>
        public FileExtensionType FileExtensionType
        {
            get
            {
                if (!IsSupportImage)
                {
                    return FileExtensionType.Unknown;
                }

                return !Const.SupportExtensionMap.TryGetValue(FileExtension, out var extensionType) ? FileExtensionType.Unknown : extensionType;
            }
        }

        /// <summary>
        /// ピクチャビューに表示する画像を作成する
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage CreatePictureViewImage(CancellationToken cancellationToken)
        {
            const int LongSideLength = 2200;
            return ImageUtil.DecodePicture(FilePath, LongSideLength, IsRawImage, cancellationToken);
        }

        /// <summary>
        /// 編集画面に表示するサムネイル画像を作成する
        /// </summary>
        /// <param name="defaultPictureWidth">DefaultPictureWidth</param>
        /// <param name="defaultPictureHeight">DefaultPictureHeight</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage CreateEditViewImage(out int defaultPictureWidth, out int defaultPictureHeight, out uint rotation)
        {
            defaultPictureWidth = 0;
            defaultPictureHeight = 0;
            rotation = 0;

            var longSideLength = rotation is 5 or 6 or 7 or 8 ? 240 : 350;
            return ImageUtil.DecodePicture(FilePath, longSideLength, IsRawImage);
        }

        /// <summary>
        /// 保存用の画像を作成する
        /// </summary>
        /// <param name="scale">scale</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage CreateSaveImage(double scale)
        {
            // WinUI版では画像処理を簡略化
            return ImageUtil.DecodePicture(FilePath, 2200, IsRawImage);
        }
    }
}