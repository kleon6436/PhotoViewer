using Kchary.PhotoViewer.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ImageEditToolViewModel : BindableBase
    {
        #region UI binding parameter

        private BitmapSource editImage;

        public BitmapSource EditImage
        {
            get => editImage;
            private set => SetProperty(ref editImage, value);
        }

        public ObservableCollection<ResizeImageCategory> ResizeCategoryItems { get; }

        private ResizeImageCategory resizeCategoryItem;

        public ResizeImageCategory ResizeCategoryItem
        {
            get => resizeCategoryItem;
            set
            {
                SetProperty(ref resizeCategoryItem, value);
                if (resizeCategoryItem == null)
                {
                    return;
                }

                double scale = 1;
                if (resizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
                {
                    // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                    scale = ResizeCategoryItem.ResizeLongSideValue / readImageSize.Width;
                    if (readImageSize.Width < readImageSize.Height)
                    {
                        scale = ResizeCategoryItem.ResizeLongSideValue / readImageSize.Height;
                    }
                }

                var resizeWidth = (int)(readImageSize.Width * scale);
                var resizeHeight = (int)(readImageSize.Height * scale);

                ResizeSizeText = $"(Width: {resizeWidth}, Height: {resizeHeight} [pixel])";
            }
        }

        private bool isEnableImageSaveQuality;

        public bool IsEnableImageSaveQuality
        {
            get => isEnableImageSaveQuality;
            set => SetProperty(ref isEnableImageSaveQuality, value);
        }

        public ObservableCollection<ImageQuality> ImageSaveQualityItems { get; }

        private ImageQuality selectedQuality;

        public ImageQuality SelectedQuality
        {
            get => selectedQuality;
            set => SetProperty(ref selectedQuality, value);
        }

        public ObservableCollection<ImageForm> ImageFormItems { get; }

        private ImageForm selectedForm;

        public ImageForm SelectedForm
        {
            get => selectedForm;
            set => SetProperty(ref selectedForm, value);
        }

        private string resizeSizeText;

        public string ResizeSizeText
        {
            get => resizeSizeText;
            private set => SetProperty(ref resizeSizeText, value);
        }

        #endregion UI binding parameter

        #region Command

        public ICommand SaveButtonCommand { get; private set; }

        #endregion Command

        public EventHandler CloseView { get; set; }

        // File path to be edited
        private string editFilePath;

        // Image size before editing
        private Size readImageSize;

        public ImageEditToolViewModel()
        {
            ResizeCategoryItems = new ObservableCollection<ResizeImageCategory>
            {
                new() { Name = "No resize", Category = ResizeImageCategory.ResizeCategory.None },
                new() { Name = "Print size", Category = ResizeImageCategory.ResizeCategory.Print, ResizeLongSideValue = 2500 },
                new() { Name = "Blog size", Category = ResizeImageCategory.ResizeCategory.Blog, ResizeLongSideValue = 1500 },
                new() { Name = "SNS size", Category = ResizeImageCategory.ResizeCategory.Twitter, ResizeLongSideValue = 1000 }
            };

            ImageSaveQualityItems = new ObservableCollection<ImageQuality>
            {
                new() { Name = "High", QualityValue = 90 },
                new() { Name = "standard", QualityValue = 80 },
                new() { Name = "Low", QualityValue = 60 }
            };

            ImageFormItems = new ObservableCollection<ImageForm>
            {
                new() { Name = "Jpeg", Form = ImageForm.ImageForms.Jpeg },
                new() { Name = "Png", Form = ImageForm.ImageForms.Png },
                new() { Name = "Bmp", Form = ImageForm.ImageForms.Bmp },
                new() { Name = "Tiff", Form = ImageForm.ImageForms.Tiff }
            };

            SaveButtonCommand = new DelegateCommand(SaveButtonClicked);
        }

        /// <summary>
        /// Set the image file information to be edited.
        /// </summary>
        /// <param name="filePath">File path</param>
        public void SetEditFileData(string filePath)
        {
            editFilePath = filePath;

            EditImage = ImageController.CreatePictureEditViewThumbnail(editFilePath, out var defaultPictureWidth, out var defaultPictureHeight, out var rotation);

            if (rotation == 5 || rotation == 6 || rotation == 7 || rotation == 8)
            {
                readImageSize = new Size { Width = defaultPictureHeight, Height = defaultPictureWidth };
            }
            else
            {
                readImageSize = new Size { Width = defaultPictureWidth, Height = defaultPictureHeight };
            }

            // Set each initial value.
            ResizeCategoryItem = ResizeCategoryItems[0];
            SelectedQuality = ImageSaveQualityItems[0];
            SelectedForm = ImageFormItems[0];
            IsEnableImageSaveQuality = true;

            // Release memory of Writable Bitmap.
            App.RunGc();
        }

        /// <summary>
        /// Event when the save button is pressed.
        /// </summary>
        private void SaveButtonClicked()
        {
            var dialog = new SaveFileDialog();
            const string DialogTitle = "Save as...";
            dialog.Title = DialogTitle;

            switch (SelectedForm.Form)
            {
                case ImageForm.ImageForms.Bmp:
                    const string BmpFilter = "Bmp file(*.bmp)|*.bmp";
                    dialog.Filter = BmpFilter;
                    break;

                case ImageForm.ImageForms.Jpeg:
                    const string JpegFilter = "Jpeg file(*.jpg;*.jpeg)|*.jpg;*.jpeg";
                    dialog.Filter = JpegFilter;
                    break;

                case ImageForm.ImageForms.Png:
                    const string PngFilter = "Png file(*.png)|*.png";
                    dialog.Filter = PngFilter;
                    break;

                case ImageForm.ImageForms.Tiff:
                    const string TiffFilter = "Tiff file(*.tif)|*.tif";
                    dialog.Filter = TiffFilter;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var saveFilePath = dialog.FileName;

            // Create a save image.
            var scale = 1.0; // No scaling
            if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
            {
                // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                scale = ResizeCategoryItem.ResizeLongSideValue / readImageSize.Width;
                if (readImageSize.Width < readImageSize.Height)
                {
                    scale = ResizeCategoryItem.ResizeLongSideValue / readImageSize.Height;
                }
            }
            var saveImageSource = ImageController.CreateSavePicture(editFilePath, scale);

            // Select the same encoder as the selected format.
            BitmapEncoder encoder = selectedForm.Form switch
            {
                ImageForm.ImageForms.Bmp => new BmpBitmapEncoder(),
                ImageForm.ImageForms.Jpeg => new JpegBitmapEncoder() {QualityLevel = SelectedQuality.QualityValue},
                ImageForm.ImageForms.Png => new PngBitmapEncoder(),
                ImageForm.ImageForms.Tiff => new TiffBitmapEncoder(),
                _ => throw new ArgumentOutOfRangeException()
            };

            try
            {
                // Add a frame to the encoder and save the file.
                encoder.Frames.Add(BitmapFrame.Create(saveImageSource));
                using (var dstStream = File.OpenWrite(saveFilePath))
                {
                    encoder.Save(dstStream);
                }

                const string SaveSuccessMessage = "Success to save image file.";
                const string SaveSuccessTitle = "Successful save";
                App.ShowSuccessMessageBox(SaveSuccessMessage, SaveSuccessTitle);
            }
            catch (Exception ex)
            {
                App.LogException(ex);

                const string SaveFailedMessage = "Failed to save the image.";
                const string SaveFailedTitle = "Save failure";
                App.ShowErrorMessageBox(SaveFailedMessage, SaveFailedTitle);
            }
            finally
            {
                CloseView?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}