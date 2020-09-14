using Microsoft.Win32;
using Kchary.PhotoViewer.Model;
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

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ImageEditToolViewModel : BindableBase
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
            get { return Path.GetFileName(editFilePath); }
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
                        scale = (double)ResizeCategoryItem.ResizelongSideValue / readImageSize.Width;
                        if (readImageSize.Width < readImageSize.Height)
                        {
                            scale = (double)ResizeCategoryItem.ResizelongSideValue / readImageSize.Height;
                        }
                    }

                    int resizeWidth = (int)(readImageSize.Width * scale);
                    int resizeHeight = (int)(readImageSize.Height * scale);
                    if (!isLandscape)
                    {
                        resizeWidth = (int)(readImageSize.Height * scale);
                        resizeHeight = (int)(readImageSize.Width * scale);
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
        private string editFilePath;

        // Image size before editing
        private Size readImageSize;

        // Image before editing
        private BitmapSource decodedPictureSource;

        public ImageEditToolViewModel()
        {
            ResizeCategoryItems.Add(new ResizeImageCategory("リサイズなし", ResizeImageCategory.ResizeCategory.None));
            ResizeCategoryItems.Add(new ResizeImageCategory("印刷向け", ResizeImageCategory.ResizeCategory.Print));
            ResizeCategoryItems.Add(new ResizeImageCategory("ブログ向け", ResizeImageCategory.ResizeCategory.Blog));
            ResizeCategoryItems.Add(new ResizeImageCategory("SNS向け", ResizeImageCategory.ResizeCategory.Twitter));

            ImageSaveQualityItems.Add(new ImageQuality { Name = "高画質", QualityValue = 90 });
            ImageSaveQualityItems.Add(new ImageQuality { Name = "標準", QualityValue = 80 });
            ImageSaveQualityItems.Add(new ImageQuality { Name = "低画質", QualityValue = 60 });

            ImageFormItems.Add(new ImageForm { Name = "Jpeg", Form = ImageForm.ImageForms.Jpeg });
            ImageFormItems.Add(new ImageForm { Name = "Png", Form = ImageForm.ImageForms.Png });
            ImageFormItems.Add(new ImageForm { Name = "Bmp", Form = ImageForm.ImageForms.Bmp });
            ImageFormItems.Add(new ImageForm { Name = "Tiff", Form = ImageForm.ImageForms.Tiff });

            SaveButtonCommand = new DelegateCommand(SaveButtonClicked);
        }

        /// <summary>
        /// Set the image file information to be edited.
        /// </summary>
        /// <param name="filePath">File path</param>
        public void SetEditFileData(string filePath)
        {
            editFilePath = filePath;
            EditImage = ImageControl.CreatePictureEditViewThumbnail(editFilePath);

            decodedPictureSource = ImageControl.DecodePicture(editFilePath);
            readImageSize = new Size(decodedPictureSource.PixelWidth, decodedPictureSource.PixelHeight);

            // Release memory of Writable Bitmap.
            App.RunGC();

            // Set each initial value.
            ResizeCategoryItem = ResizeCategoryItems[0];
            SelectedQuality = ImageSaveQualityItems[0];
            SelectedForm = ImageFormItems[0];
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

            var saveFilePath = dialog.FileName;

            // Create a scaled bitmap.
            var scale = 1.0; // No scaling
            if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
            {
                // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                scale = (double)ResizeCategoryItem.ResizelongSideValue / decodedPictureSource.PixelWidth;
                if (decodedPictureSource.PixelWidth < decodedPictureSource.PixelHeight)
                {
                    scale = (double)ResizeCategoryItem.ResizelongSideValue / decodedPictureSource.PixelHeight;
                }
            }
            var scaledBitmapSource = new TransformedBitmap(decodedPictureSource, new ScaleTransform(scale, scale));

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