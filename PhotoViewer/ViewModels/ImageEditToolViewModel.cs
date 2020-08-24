using Microsoft.Win32;
using PhotoViewer.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            get { return Path.GetFileName(EditFilePath); }
        }

        public ObservableCollection<ResizeImageCategory> ResizeCategoryItems { get; } = new ObservableCollection<ResizeImageCategory>();

        private ResizeImageCategory resizeCategoryItem;

        public ResizeImageCategory ResizeCategoryItem
        {
            get { return resizeCategoryItem; }
            set
            {
                SetProperty(ref resizeCategoryItem, value);
                if (resizeCategoryItem != null)
                {
                    double scale = 1;
                    bool isLandscape = true;
                    if (resizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
                    {
                        // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                        scale = (double)ResizeCategoryItem.ResizelongSideValue / ReadImageSize.Width;
                        if (ReadImageSize.Width < ReadImageSize.Height)
                        {
                            scale = (double)ResizeCategoryItem.ResizelongSideValue / ReadImageSize.Height;
                        }
                    }

                    int resizeWidth = (int)(ReadImageSize.Width * scale);
                    int resizeHeight = (int)(ReadImageSize.Height * scale);
                    if (!isLandscape)
                    {
                        resizeWidth = (int)(ReadImageSize.Height * scale);
                        resizeHeight = (int)(ReadImageSize.Width * scale);
                    }
                    ResizeSizeText = string.Format("(Width: {0}, Height: {1} [pixel])", resizeWidth, resizeHeight);
                }
            }
        }

        private bool isEnableImageSaveQuality;

        public bool IsEnableImageSaveQuality
        {
            get { return isEnableImageSaveQuality; }
            set { SetProperty(ref isEnableImageSaveQuality, value); }
        }

        public ObservableCollection<ImageQuality> ImageSaveQualityItems { get; } = new ObservableCollection<ImageQuality>();

        private ImageQuality selectedQuality;

        public ImageQuality SelectedQuality
        {
            get { return selectedQuality; }
            set { SetProperty(ref selectedQuality, value); }
        }

        public ObservableCollection<ImageForm> ImageFormItems { get; } = new ObservableCollection<ImageForm>();

        private ImageForm selectedForm;

        public ImageForm SelectedForm
        {
            get { return selectedForm; }
            set { SetProperty(ref selectedForm, value); }
        }

        private string resizeSizeText;

        public string ResizeSizeText
        {
            get { return resizeSizeText; }
            set { SetProperty(ref resizeSizeText, value); }
        }

        #endregion UI binding parameter

        #region Command

        public ICommand SaveButtonCommand { get; set; }

        #endregion Command

        public EventHandler CloseView { get; set; }

        // File path to be edited
        private string EditFilePath;

        // Image size before editing
        private Size ReadImageSize;

        // Image before editing
        private BitmapSource DecodedPictureSource;

        public ImageEditToolViewModel()
        {
            const string NoResize = "リサイズなし";
            const string PrintResize = "印刷向け";
            const string BlogResize = "ブログ向け";
            const string SnsResize = "SNS向け";
            ResizeCategoryItems.Add(new ResizeImageCategory(NoResize, ResizeImageCategory.ResizeCategory.None));
            ResizeCategoryItems.Add(new ResizeImageCategory(PrintResize, ResizeImageCategory.ResizeCategory.Print));
            ResizeCategoryItems.Add(new ResizeImageCategory(BlogResize, ResizeImageCategory.ResizeCategory.Blog));
            ResizeCategoryItems.Add(new ResizeImageCategory(SnsResize, ResizeImageCategory.ResizeCategory.Twitter));

            const string HighQuality = "高画質";
            const string StandardQuality = "標準";
            const string LowQuality = "低画質";
            ImageSaveQualityItems.Add(new ImageQuality(HighQuality, 90));
            ImageSaveQualityItems.Add(new ImageQuality(StandardQuality, 80));
            ImageSaveQualityItems.Add(new ImageQuality(LowQuality, 60));

            const string Jpeg = "Jpeg";
            const string Png = "Png";
            const string Bmp = "Bmp";
            const string Tiff = "Tiff";
            ImageFormItems.Add(new ImageForm(Jpeg, ImageForm.ImageForms.Jpeg));
            ImageFormItems.Add(new ImageForm(Png, ImageForm.ImageForms.Png));
            ImageFormItems.Add(new ImageForm(Bmp, ImageForm.ImageForms.Bmp));
            ImageFormItems.Add(new ImageForm(Tiff, ImageForm.ImageForms.Tiff));

            SaveButtonCommand = new DelegateCommand(SaveButtonClicked);
        }

        /// <summary>
        /// Set the image file information to be edited.
        /// </summary>
        /// <param name="filePath">File path</param>
        public void SetEditFileData(string filePath)
        {
            EditFilePath = filePath;
            EditImage = ImageControl.CreatePictureEditViewThumbnail(EditFilePath);

            // Release memory of Writable Bitmap.
            App.RunGC();

            DecodedPictureSource = ImageControl.DecodePicture(EditFilePath);
            ReadImageSize = new Size(DecodedPictureSource.PixelWidth, DecodedPictureSource.PixelHeight);

            // Release memory of Writable Bitmap.
            App.RunGC();

            // Set each initial value.
            ResizeCategoryItem = ResizeCategoryItems.First();
            SelectedQuality = ImageSaveQualityItems.First();
            SelectedForm = ImageFormItems.First();
            IsEnableImageSaveQuality = true;
        }

        /// <summary>
        /// Event when the save button is pressed.
        /// </summary>
        private void SaveButtonClicked()
        {
            var dialog = new SaveFileDialog();
            const string DialogTitle = "名前を付けて保存";
            dialog.Title = DialogTitle;

            switch (SelectedForm.Form)
            {
                case ImageForm.ImageForms.Bmp:
                    const string BmpFilter = "BMPファイル(*.bmp)|*.bmp";
                    dialog.Filter = BmpFilter;
                    break;

                case ImageForm.ImageForms.Jpeg:
                    const string JpegFilter = "Jpegファイル(*.jpg;*.jpeg)|*.jpg;*.jpeg";
                    dialog.Filter = JpegFilter;
                    break;

                case ImageForm.ImageForms.Png:
                    const string PngFilter = "PNGファイル(*.png)|*.png";
                    dialog.Filter = PngFilter;
                    break;

                case ImageForm.ImageForms.Tiff:
                    const string TiffFilter = "TIFFファイル(*.tif)|*.tif";
                    dialog.Filter = TiffFilter;
                    break;

                default:
                    break;
            }

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            string saveFilePath = dialog.FileName;

            // Create a scaled bitmap.
            double scale = 1; // No scaling
            if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
            {
                // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                scale = (double)ResizeCategoryItem.ResizelongSideValue / DecodedPictureSource.PixelWidth;
                if (DecodedPictureSource.PixelWidth < DecodedPictureSource.PixelHeight)
                {
                    scale = (double)ResizeCategoryItem.ResizelongSideValue / DecodedPictureSource.PixelHeight;
                }
            }
            var scaledBitmapSource = new TransformedBitmap(DecodedPictureSource, new ScaleTransform(scale, scale));

            // Select the same encoder as the selected format.
            BitmapEncoder encoder = null;
            switch (selectedForm.Form)
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
                // Add a frame to the encoder and save the file.
                encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource));
                using (var dstStream = File.OpenWrite(saveFilePath))
                {
                    encoder.Save(dstStream);
                }

                const string SaveSuccessMessage = "画像の保存に成功しました。";
                const string SaveSuccessTitle = "保存成功";
                App.ShowSuccessMessageBox(SaveSuccessMessage, SaveSuccessTitle);
            }
            catch (Exception ex)
            {
                App.LogException(ex);

                const string SaveFailedMessage = "画像の保存に失敗しました。";
                const string SaveFailedTitle = "保存失敗";
                App.ShowErrorMessageBox(SaveFailedMessage, SaveFailedTitle);
            }
            finally
            {
                CloseView?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}