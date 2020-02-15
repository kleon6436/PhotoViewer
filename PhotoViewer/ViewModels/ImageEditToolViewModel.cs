using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Prism.Commands;
using Microsoft.Win32;
using PhotoViewer.Model;
using System.Windows.Media;

namespace PhotoViewer.ViewModels
{
    public class ImageEditToolViewModel : BindableBase
    {
        #region UI binding parameter
        private BitmapSource editImage;
        public BitmapSource EditImage
        {
            get { return editImage; }
            set { SetProperty(ref editImage, value); }
        }

        public string EditFileName
        {
            get { return Path.GetFileName(this.EditFilePath); }
        }

        private ObservableCollection<ResizeImageCategory> resizeCategoryItems = new ObservableCollection<ResizeImageCategory>();
        public ObservableCollection<ResizeImageCategory> ResizeCategoryItems
        {
            get { return resizeCategoryItems; }
            set { SetProperty(ref resizeCategoryItems, value); }
        }

        private ResizeImageCategory resizeCategoryItem;
        public ResizeImageCategory ResizeCategoryItem
        {
            get { return resizeCategoryItem; }
            set { SetProperty(ref resizeCategoryItem, value); }
        }

        private bool isEnableImageSaveQuality;
        public bool IsEnableImageSaveQuality
        {
            get { return isEnableImageSaveQuality; }
            set { SetProperty(ref isEnableImageSaveQuality, value); }
        }

        private ObservableCollection<ImageQuality> imageSaveQualityItems = new ObservableCollection<ImageQuality>();
        public ObservableCollection<ImageQuality> ImageSaveQualityItems
        {
            get { return imageSaveQualityItems; }
            set { SetProperty(ref imageSaveQualityItems, value); }
        }

        private ImageQuality selectedQuality;
        public ImageQuality SelectedQuality
        {
            get { return selectedQuality; }
            set { SetProperty(ref selectedQuality, value); }
        }

        private ObservableCollection<ImageForm> imageFormItems = new ObservableCollection<ImageForm>();
        public ObservableCollection<ImageForm> ImageFormItems
        {
            get { return imageFormItems; }
            set { SetProperty(ref imageFormItems, value); }
        }

        private ImageForm selectedForm;
        public ImageForm SelectedForm
        {
            get { return selectedForm; }
            set { SetProperty(ref selectedForm, value); }
        }
        #endregion

        #region Command
        public ICommand SaveButtonCommand { get; set; }
        #endregion

        public EventHandler CloseView;
        // 編集対象のファイルパス
        private string EditFilePath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImageEditToolViewModel()
        {
            ResizeCategoryItems.Add(new ResizeImageCategory("リサイズなし", ResizeImageCategory.ResizeCategory.None));
            ResizeCategoryItems.Add(new ResizeImageCategory("ブログ向け", ResizeImageCategory.ResizeCategory.Blog));
            ResizeCategoryItems.Add(new ResizeImageCategory("Twitter向け", ResizeImageCategory.ResizeCategory.Twitter));
            ResizeCategoryItem = ResizeCategoryItems.First();

            ImageSaveQualityItems.Add(new ImageQuality("高画質", 90));
            ImageSaveQualityItems.Add(new ImageQuality("標準", 80));
            ImageSaveQualityItems.Add(new ImageQuality("低画質", 60));
            SelectedQuality = ImageSaveQualityItems.First();

            ImageFormItems.Add(new ImageForm("Jpeg", ImageForm.ImageForms.Jpeg));
            ImageFormItems.Add(new ImageForm("Png", ImageForm.ImageForms.Png));
            ImageFormItems.Add(new ImageForm("Bmp", ImageForm.ImageForms.Bmp));
            ImageFormItems.Add(new ImageForm("Tiff", ImageForm.ImageForms.Tiff));
            SelectedForm = ImageFormItems.First();
            IsEnableImageSaveQuality = true;

            SaveButtonCommand = new DelegateCommand(SaveButtonClicked);
        }

        /// <summary>
        /// 編集対象の画像ファイル情報を設定
        /// </summary>
        /// <param name="filePath">選択されている画像ファイルパス</param>
        public void SetEditFileData(string filePath)
        {
            this.EditFilePath = filePath;
            this.EditImage = ImageControl.CreatePictureEditViewThumbnail(this.EditFilePath);
        }

        /// <summary>
        /// 保存ボタン押下時の動作
        /// </summary>
        private void SaveButtonClicked()
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "名前を付けて保存";

            switch (SelectedForm.Form)
            {
                case ImageForm.ImageForms.Bmp:
                    dialog.Filter = "BMPファイル(*.bmp)|*.bmp";
                    break;

                case ImageForm.ImageForms.Jpeg:
                    dialog.Filter = "Jpegファイル(*.jpg;*.jpeg)|*.jpg;*.jpeg";
                    break;

                case ImageForm.ImageForms.Png:
                    dialog.Filter = "PNGファイル(*.png)|*.png";
                    break;

                case ImageForm.ImageForms.Tiff:
                    dialog.Filter = "TIFFファイル(*.tif)|*.tif";
                    break;

                default:
                    break;
            }

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            string saveFilePath = dialog.FileName;

            using (var sourceStream = File.OpenRead(EditFilePath))
            {
                // 画像データの取得
                var decoder = BitmapDecoder.Create(sourceStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                var bitmapSource = decoder.Frames[0];

                // 拡大・縮小されたビットマップを作成
                double scale = 1; // 拡大縮小なし
                if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
                {
                    // 拡大率を計算(縦の方が長い場合は、縦の長さに対して拡大率を計算)
                    scale = (double)ResizeCategoryItem.ResizelongSideValue / bitmapSource.PixelWidth;
                    if (bitmapSource.PixelWidth < bitmapSource.PixelHeight)
                    {
                        scale = (double)ResizeCategoryItem.ResizelongSideValue / bitmapSource.PixelHeight;
                    }
                }
                var scaledBitmapSource = new TransformedBitmap(bitmapSource, new ScaleTransform(scale, scale));

                // 選択されている形式と同じエンコーダを選択
                BitmapEncoder encoder = null;
                switch (SelectedForm.Form)
                {
                    case ImageForm.ImageForms.Bmp:
                        encoder = new BmpBitmapEncoder();
                        break;

                    case ImageForm.ImageForms.Jpeg:
                        encoder = new JpegBitmapEncoder() { QualityLevel = SelectedQuality.QualityValue };
                        break;

                    case ImageForm.ImageForms.Png:
                        encoder = new PngBitmapEncoder();
                        break;

                    case ImageForm.ImageForms.Tiff:
                        encoder = new TiffBitmapEncoder();
                        break;

                    default:
                        break;
                }

                try
                {
                    // エンコーダにフレームを追加し、ファイルを保存する
                    encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource));
                    using (var dstStream = File.OpenWrite(saveFilePath))
                    {
                        encoder.Save(dstStream);
                    }

                    App.ShowSuccessMessageBox("画像の保存に成功しました。", "保存成功");
                }
                catch (Exception ex)
                {
                    App.LogException(ex);
                    App.ShowErrorMessageBox("画像の保存に失敗しました。", "保存失敗");
                }
                finally
                {
                    CloseView?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public class ResizeImageCategory
    {
        public enum ResizeCategory
        {
            None,    // リサイズしない
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

        public ImageForm(string name, ImageForms imageForm)
        {
            this.Name = name;
            this.Form = imageForm;
        }
    }
}
