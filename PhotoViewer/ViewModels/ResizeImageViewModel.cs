using Kchary.PhotoViewer.Data;
using Kchary.PhotoViewer.Models;
using Microsoft.Win32;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ResizeImageViewModel : BindableBase, IDisposable
    {
        #region UI binding parameter

        /// <summary>
        /// リサイズカテゴリの表示用リスト
        /// </summary>
        public ResizeImageCategory[] ResizeCategoryItems { get; }

        /// <summary>
        /// 画質カテゴリの表示用リスト
        /// </summary>
        public ImageQuality[] ImageSaveQualityItems { get; }

        /// <summary>
        /// 保存形式の表示用リスト
        /// </summary>
        public ImageForm[] ImageFormItems { get; }

        /// <summary>
        /// 編集画面に表示する画像
        /// </summary>
        public ReactivePropertySlim<BitmapSource> EditImage { get; } = new();

        /// <summary>
        /// 編集画面に表示するリサイズカテゴリアイテム
        /// </summary>
        public ReactivePropertySlim<ResizeImageCategory> ResizeCategoryItem { get; } = new();

        /// <summary>
        /// 編集画面の保存ボタンのON・OFFフラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsEnableImageSaveQuality { get; } = new();

        /// <summary>
        /// 選択した保存画質
        /// </summary>
        public ReactivePropertySlim<ImageQuality> SelectedQuality { get; } = new();

        /// <summary>
        /// 選択した保存形式
        /// </summary>
        public ReactivePropertySlim<ImageForm> SelectedForm { get; } = new();

        /// <summary>
        /// 選択したリサイズサイズ(幅)
        /// </summary>
        public ReactivePropertySlim<string> ResizeSizeWidthText { get; } = new();

        /// <summary>
        /// 選択したリサイズサイズ(高さ)
        /// </summary>
        public ReactivePropertySlim<string> ResizeSizeHeightText { get; } = new();

        /// <summary>
        /// リサイズする画像ファイル名
        /// </summary>
        public string ResizeImageName { get; set; }

        #endregion UI binding parameter

        #region Command

        /// <summary>
        /// 保存ボタンのコマンド
        /// </summary>
        public ReactiveCommand SaveButtonCommand { get; }

        #endregion Command

        /// <summary>
        /// 画面を閉じるイベント
        /// </summary>
        public EventHandler CloseView { get; set; }

        /// <summary>
        /// IDisposableをまとめるCompositeDisposable
        /// </summary>
        private readonly CompositeDisposable disposable = new();

        /// <summary>
        /// リサイズ対象のファイルパス
        /// </summary>
        private string ResizeFilePath { get; set; }

        /// <summary>
        /// 読み込んだ画像のサイズ
        /// </summary>
        private Size ReadImageSize { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResizeImageViewModel()
        {
            ResizeCategoryItems = new ResizeImageCategory[]
            {
                new() { Name = "No resize" , Category = ResizeCategory.None },
                new() { Name = "Print size", Category = ResizeCategory.Print  , ResizeLongSideValue = 2500 },
                new() { Name = "Blog size" , Category = ResizeCategory.Blog   , ResizeLongSideValue = 1500 },
                new() { Name = "SNS size"  , Category = ResizeCategory.Twitter, ResizeLongSideValue = 1000 }
            };

            ImageSaveQualityItems = new ImageQuality[]
            {
                new() { Name = "High"    , QualityValue = 90 },
                new() { Name = "standard", QualityValue = 80 },
                new() { Name = "Low"     , QualityValue = 60 }
            };

            ImageFormItems = new ImageForm[]
            {
                new() { Name = "Jpeg", Form = ImageForms.Jpeg },
                new() { Name = "Png" , Form = ImageForms.Png },
                new() { Name = "Bmp" , Form = ImageForms.Bmp },
                new() { Name = "Tiff", Form = ImageForms.Tiff }
            };

            ResizeCategoryItem.Subscribe(OnResizeCategoryItemChanged).AddTo(disposable);

            SaveButtonCommand = new ReactiveCommand().WithSubscribe(SaveButtonClicked).AddTo(disposable);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => disposable.Dispose();

        /// <summary>
        /// 編集対象の保存ファイルパスをViewModelに設定する
        /// </summary>
        /// <param name="filePath">編集対象のファイルパス</param>
        public void SetEditFileData(string filePath)
        {
            ResizeFilePath = filePath;
            ResizeImageName = Path.GetFileName(filePath);

            EditImage.Value = ImageController.CreatePictureEditViewThumbnail(ResizeFilePath, out var defaultPictureWidth, out var defaultPictureHeight, out var rotation);

            ReadImageSize = rotation is 5 or 6 or 7 or 8
                ? new Size { Width = defaultPictureHeight, Height = defaultPictureWidth }
                : new Size { Width = defaultPictureWidth, Height = defaultPictureHeight };

            ResizeCategoryItem.Value = ResizeCategoryItems[0];
            SelectedQuality.Value = ImageSaveQualityItems[0];
            SelectedForm.Value = ImageFormItems[0];
            IsEnableImageSaveQuality.Value = true;
        }

        /// <summary>
        /// 保存ボタンを押下時の処理
        /// </summary>
        private void SaveButtonClicked()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save as...",
                Filter = SelectedForm.Value.Form switch
                {
                    ImageForms.Bmp => "Bmp file(*.bmp)|*.bmp",
                    ImageForms.Jpeg => "Jpeg file(*.jpg;*.jpeg)|*.jpg;*.jpeg",
                    ImageForms.Png => "Png file(*.png)|*.png",
                    ImageForms.Tiff => "Tiff file(*.tif)|*.tif",
                    _ => throw new ArgumentOutOfRangeException(),
                }
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var saveFilePath = dialog.FileName;

            // 保存する画像の作成
            // デフォルトでは、リサイズなしとする
            var scale = 1.0;
            if (ResizeCategoryItem.Value.Category != ResizeCategory.None)
            {
                scale = ResizeCategoryItem.Value.ResizeLongSideValue / ReadImageSize.Width;
                if (ReadImageSize.Width < ReadImageSize.Height)
                {
                    scale = ResizeCategoryItem.Value.ResizeLongSideValue / ReadImageSize.Height;
                }
            }
            var saveImageSource = ImageController.CreateSavePicture(ResizeFilePath, scale);

            // 選択された保存形式と同じエンコーダーを用意
            BitmapEncoder encoder = SelectedForm.Value.Form switch
            {
                ImageForms.Bmp => new BmpBitmapEncoder(),
                ImageForms.Jpeg => new JpegBitmapEncoder { QualityLevel = SelectedQuality.Value.QualityValue },
                ImageForms.Png => new PngBitmapEncoder(),
                ImageForms.Tiff => new TiffBitmapEncoder(),
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

        /// <summary>
        /// リサイズカテゴリの選択が変更されたときの処理
        /// </summary>
        /// <param name="resizeCategoryItem">リサイズカテゴリ</param>
        private void OnResizeCategoryItemChanged(ResizeImageCategory resizeCategoryItem)
        {
            if (resizeCategoryItem == null)
            {
                return;
            }

            double scale = 1;
            if (resizeCategoryItem.Category != ResizeCategory.None)
            {
                // 倍率計算(この値をもとにリサイズする)
                scale = ResizeCategoryItem.Value.ResizeLongSideValue / ReadImageSize.Width;
                if (ReadImageSize.Width < ReadImageSize.Height)
                {
                    scale = ResizeCategoryItem.Value.ResizeLongSideValue / ReadImageSize.Height;
                }
            }

            var resizeWidth = (int)(ReadImageSize.Width * scale);
            var resizeHeight = (int)(ReadImageSize.Height * scale);

            ResizeSizeWidthText.Value = $"Width: {resizeWidth} [pixel]";
            ResizeSizeHeightText.Value = $"Height: {resizeHeight} [pixel]";
        }
    }
}