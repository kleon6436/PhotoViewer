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
        /// <summary>
        /// 編集対象のファイルパス
        /// </summary>
        private string EditFilePath { get; set; }
        
        /// <summary>
        /// 読み込んだ画像のサイズ
        /// </summary>
        private Size ReadImageSize { get; set; }

        #region UI binding parameter

        private BitmapSource editImage;
        private ResizeImageCategory resizeCategoryItem;
        private bool isEnableImageSaveQuality;
        private ImageQuality selectedQuality;
        private ImageForm selectedForm;
        private string resizeSizeText;

        /// <summary>
        /// リサイズカテゴリの表示用リスト
        /// </summary>
        public ObservableCollection<ResizeImageCategory> ResizeCategoryItems { get; }
        
        /// <summary>
        /// 画質カテゴリの表示用リスト
        /// </summary>
        public ObservableCollection<ImageQuality> ImageSaveQualityItems { get; }

        /// <summary>
        /// 保存形式の表示用リスト
        /// </summary>
        public ObservableCollection<ImageForm> ImageFormItems { get; }

        /// <summary>
        /// 編集画面に表示する画像
        /// </summary>
        public BitmapSource EditImage
        {
            get => editImage;
            private set => SetProperty(ref editImage, value);
        }

        /// <summary>
        /// 編集画面に表示するリサイズカテゴリアイテム
        /// </summary>
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
                    scale = ResizeCategoryItem.ResizeLongSideValue / ReadImageSize.Width;
                    if (ReadImageSize.Width < ReadImageSize.Height)
                    {
                        scale = ResizeCategoryItem.ResizeLongSideValue / ReadImageSize.Height;
                    }
                }

                var resizeWidth = (int)(ReadImageSize.Width * scale);
                var resizeHeight = (int)(ReadImageSize.Height * scale);

                ResizeSizeText = $"(Width: {resizeWidth}, Height: {resizeHeight} [pixel])";
            }
        }

        /// <summary>
        /// 編集画面の保存ボタンのON・OFFフラグ
        /// </summary>
        public bool IsEnableImageSaveQuality
        {
            get => isEnableImageSaveQuality;
            set => SetProperty(ref isEnableImageSaveQuality, value);
        }

        /// <summary>
        /// 選択した保存画質
        /// </summary>
        public ImageQuality SelectedQuality
        {
            get => selectedQuality;
            set => SetProperty(ref selectedQuality, value);
        }

        /// <summary>
        /// 選択した保存形式
        /// </summary>
        public ImageForm SelectedForm
        {
            get => selectedForm;
            set => SetProperty(ref selectedForm, value);
        }

        /// <summary>
        /// 選択したリサイズサイズ(幅x高さ)
        /// </summary>
        public string ResizeSizeText
        {
            get => resizeSizeText;
            private set => SetProperty(ref resizeSizeText, value);
        }

        #endregion UI binding parameter

        #region Command

        /// <summary>
        /// 保存ボタンのコマンド
        /// </summary>
        public ICommand SaveButtonCommand { get; }

        #endregion Command

        /// <summary>
        /// 画面を閉じるイベント
        /// </summary>
        public EventHandler CloseView { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImageEditToolViewModel()
        {
            ResizeCategoryItems = new ObservableCollection<ResizeImageCategory>
            {
                new() { Name = "No resize" , Category = ResizeImageCategory.ResizeCategory.None },
                new() { Name = "Print size", Category = ResizeImageCategory.ResizeCategory.Print  , ResizeLongSideValue = 2500 },
                new() { Name = "Blog size" , Category = ResizeImageCategory.ResizeCategory.Blog   , ResizeLongSideValue = 1500 },
                new() { Name = "SNS size"  , Category = ResizeImageCategory.ResizeCategory.Twitter, ResizeLongSideValue = 1000 }
            };

            ImageSaveQualityItems = new ObservableCollection<ImageQuality>
            {
                new() { Name = "High"    , QualityValue = 90 },
                new() { Name = "standard", QualityValue = 80 },
                new() { Name = "Low"     , QualityValue = 60 }
            };

            ImageFormItems = new ObservableCollection<ImageForm>
            {
                new() { Name = "Jpeg", Form = ImageForm.ImageForms.Jpeg },
                new() { Name = "Png" , Form = ImageForm.ImageForms.Png },
                new() { Name = "Bmp" , Form = ImageForm.ImageForms.Bmp },
                new() { Name = "Tiff", Form = ImageForm.ImageForms.Tiff }
            };

            SaveButtonCommand = new DelegateCommand(SaveButtonClicked);
        }

        /// <summary>
        /// 編集対象の保存ファイルパスをViewModelに設定する
        /// </summary>
        /// <param name="filePath">編集対象のファイルパス</param>
        public void SetEditFileData(string filePath)
        {
            EditFilePath = filePath;

            EditImage = ImageController.CreatePictureEditViewThumbnail(EditFilePath, out var defaultPictureWidth, out var defaultPictureHeight, out var rotation);

            ReadImageSize = rotation is 5 or 6 or 7 or 8
                ? new Size { Width = defaultPictureHeight, Height = defaultPictureWidth }
                : new Size { Width = defaultPictureWidth, Height = defaultPictureHeight };

            ResizeCategoryItem = ResizeCategoryItems[0];
            SelectedQuality = ImageSaveQualityItems[0];
            SelectedForm = ImageFormItems[0];
            IsEnableImageSaveQuality = true;

            App.RunGc();
        }

        /// <summary>
        /// 保存ボタンを押下時の処理
        /// </summary>
        private void SaveButtonClicked()
        {
            var dialog = new SaveFileDialog();
            const string DialogTitle = "Save as...";
            dialog.Title = DialogTitle;

            dialog.Filter = SelectedForm.Form switch
            {
                ImageForm.ImageForms.Bmp => "Bmp file(*.bmp)|*.bmp",
                ImageForm.ImageForms.Jpeg => "Jpeg file(*.jpg;*.jpeg)|*.jpg;*.jpeg",
                ImageForm.ImageForms.Png => "Png file(*.png)|*.png",
                ImageForm.ImageForms.Tiff => "Tiff file(*.tif)|*.tif",
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var saveFilePath = dialog.FileName;

            // 保存する画像の作成
            // デフォルトでは、リサイズなしとする
            var scale = 1.0; 
            if (ResizeCategoryItem.Category != ResizeImageCategory.ResizeCategory.None)
            {
                scale = ResizeCategoryItem.ResizeLongSideValue / ReadImageSize.Width;
                if (ReadImageSize.Width < ReadImageSize.Height)
                {
                    scale = ResizeCategoryItem.ResizeLongSideValue / ReadImageSize.Height;
                }
            }
            var saveImageSource = ImageController.CreateSavePicture(EditFilePath, scale);

            // 選択された保存形式と同じエンコーダーを用意
            BitmapEncoder encoder = selectedForm.Form switch
            {
                ImageForm.ImageForms.Bmp  => new BmpBitmapEncoder(),
                ImageForm.ImageForms.Jpeg => new JpegBitmapEncoder {QualityLevel = SelectedQuality.QualityValue},
                ImageForm.ImageForms.Png  => new PngBitmapEncoder(),
                ImageForm.ImageForms.Tiff => new TiffBitmapEncoder(),
                _ => throw new ArgumentOutOfRangeException()
            };

            try
            {
                // 画像をエンコーダーを使って保存する
                encoder.Frames.Add(BitmapFrame.Create(saveImageSource));
                using (var dstStream = File.OpenWrite(saveFilePath))
                {
                    encoder.Save(dstStream);
                }

                App.ShowSuccessMessageBox("Success to save image file.", "Successful save");
            }
            catch (Exception ex)
            {
                App.LogException(ex);
                App.ShowErrorMessageBox("Failed to save the image.", "Save failure");
            }
            finally
            {
                CloseView?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}