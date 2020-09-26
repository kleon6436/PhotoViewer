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
            set => SetProperty(ref editImage, value);
        }

        public string EditFileName => Path.GetFileName(editFilePath);

        public ObservableCollection<ResizeImageCategory> ResizeCategoryItems { get; }

        private ResizeImageCategory resizeCategoryItem;

        public ResizeImageCategory ResizeCategoryItem
        {
            get => resizeCategoryItem;
            set
            {
                SetProperty(ref resizeCategoryItem, value);
                if (resizeCategoryItem != null)
                {
                    double scale = 1;
                    var isLandscape = true;
                    if (resizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
                    {
                        // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                        scale = ResizeCategoryItem.ResizelongSideValue / readImageSize.Width;
                        if (readImageSize.Width < readImageSize.Height)
                        {
                            scale = ResizeCategoryItem.ResizelongSideValue / readImageSize.Height;
                        }
                    }

                    var resizeWidth = (int)(readImageSize.Width * scale);
                    var resizeHeight = (int)(readImageSize.Height * scale);
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
            set => SetProperty(ref resizeSizeText, value);
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

        public ImageEditToolViewModel()
        {
            ResizeCategoryItems = new ObservableCollection<ResizeImageCategory>
            {
                new ResizeImageCategory{ Name = "リサイズなし", Category = ResizeImageCategory.ResizeCategory.None },
                new ResizeImageCategory{ Name = "印刷向け", Category = ResizeImageCategory.ResizeCategory.Print, ResizelongSideValue = 2500 },
                new ResizeImageCategory{ Name = "ブログ向け", Category = ResizeImageCategory.ResizeCategory.Blog, ResizelongSideValue = 1500 },
                new ResizeImageCategory{ Name = "SNS向け", Category = ResizeImageCategory.ResizeCategory.Twitter, ResizelongSideValue = 1000 }
            };

            ImageSaveQualityItems = new ObservableCollection<ImageQuality>
            {
                new ImageQuality { Name = "高画質", QualityValue = 90 },
                new ImageQuality { Name = "標準", QualityValue = 80 },
                new ImageQuality { Name = "低画質", QualityValue = 60 }
            };

            ImageFormItems = new ObservableCollection<ImageForm>
            {
                new ImageForm { Name = "Jpeg", Form = ImageForm.ImageForms.Jpeg },
                new ImageForm { Name = "Png", Form = ImageForm.ImageForms.Png },
                new ImageForm { Name = "Bmp", Form = ImageForm.ImageForms.Bmp },
                new ImageForm { Name = "Tiff", Form = ImageForm.ImageForms.Tiff }
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
            EditImage = ImageControl.CreatePictureEditViewThumbnail(editFilePath, out var defaultPictureWidth, out var defaultPictureHeight, out var rotation);
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
            App.RunGC();
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

            // Create a save image.
            var scale = 1.0; // No scaling
            if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
            {
                // Magnification factor is calculated (if the vertical dimension is longer, the magnification factor is calculated for the vertical dimension).
                scale = ResizeCategoryItem.ResizelongSideValue / readImageSize.Width;
                if (readImageSize.Width < readImageSize.Height)
                {
                    scale = ResizeCategoryItem.ResizelongSideValue / readImageSize.Height;
                }
            }
            BitmapSource saveImageSource = ImageControl.CreateSavePicture(editFilePath, scale);

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
                encoder.Frames.Add(BitmapFrame.Create(saveImageSource));
                using (FileStream dstStream = File.OpenWrite(saveFilePath))
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