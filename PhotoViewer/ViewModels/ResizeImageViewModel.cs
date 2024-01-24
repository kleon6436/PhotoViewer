using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Models;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ResizeImageViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// リサイズカテゴリ
        /// </summary>
        public enum ResizeCategory
        {
            None,
            Print,
            Blog,
            Twitter,
        }

        /// <summary>
        ///  編集画面に表示する画像保存形式レコード
        /// </summary>
        /// <param name="Name">画像保存形式名</param>
        /// <param name="Form">画像保存形式</param>
        public record ImageForm(string Name, FileExtensionType Form);

        /// <summary>
        /// 編集画面に表示する画質設定メニューレコード
        /// </summary>
        /// <param name="Name">画質名</param>
        /// <param name="QualityValue">画質に対応する値</param>
        public record ImageQuality(string Name, int QualityValue);

        /// <summary>
        /// 編集画面に表示するリサイズカテゴリ情報レコード
        /// </summary>
        /// <param name="Name">リサイズカテゴリ名</param>
        /// <param name="Category">リサイズカテゴリ</param>
        /// <param name="ResizeLongSideValue">リサイズする長辺の長さ</param>
        public record ResizeImageCategory(string Name, ResizeCategory Category, int ResizeLongSideValue);

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
        public PhotoInfo ResizeMediaInfo { get; set; }

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
        /// 読み込んだ画像のサイズ
        /// </summary>
        private Size readImageSize;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResizeImageViewModel()
        {
            ResizeCategoryItems =
            [
                new("No resize", ResizeCategory.None, 0),
                new("Print size", ResizeCategory.Print, 2500),
                new("Blog size", ResizeCategory.Blog, 1500),
                new("SNS size", ResizeCategory.Twitter, 1000)
            ];

            ImageSaveQualityItems =
            [
                new("High", 90),
                new("standard", 80),
                new("Low", 60)
            ];

            ImageFormItems =
            [
                new("Jpeg", FileExtensionType.Jpeg),
                new("Png", FileExtensionType.Png),
                new("Bmp", FileExtensionType.Bmp),
                new("Tiff", FileExtensionType.Tiff)
            ];

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
        /// <param name="resizeMediaInfo">リサイズ対象のメディア情報</param>
        public void SetEditFileData(PhotoInfo resizeMediaInfo)
        {
            ResizeMediaInfo = resizeMediaInfo;
            EditImage.Value = ResizeMediaInfo.CreateEditViewImage(out var defaultPictureWidth, out var defaultPictureHeight, out var rotation);

            readImageSize = rotation is 5 or 6 or 7 or 8
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
                    FileExtensionType.Bmp => "Bmp file(*.bmp)|*.bmp",
                    FileExtensionType.Jpeg => "Jpeg file(*.jpg;*.jpeg)|*.jpg;*.jpeg",
                    FileExtensionType.Png => "Png file(*.png)|*.png",
                    FileExtensionType.Tiff => "Tiff file(*.tif)|*.tif",
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
                scale = ResizeCategoryItem.Value.ResizeLongSideValue / readImageSize.Width;
                if (readImageSize.Width < readImageSize.Height)
                {
                    scale = ResizeCategoryItem.Value.ResizeLongSideValue / readImageSize.Height;
                }
            }
            var saveImageSource = ResizeMediaInfo.CreateSaveImage(scale);

            // 選択された保存形式と同じエンコーダーを用意
            BitmapEncoder encoder = SelectedForm.Value.Form switch
            {
                FileExtensionType.Bmp => new BmpBitmapEncoder(),
                FileExtensionType.Jpeg => new JpegBitmapEncoder { QualityLevel = SelectedQuality.Value.QualityValue },
                FileExtensionType.Png => new PngBitmapEncoder(),
                FileExtensionType.Tiff => new TiffBitmapEncoder(),
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

            var scale = 1.0;
            if (resizeCategoryItem.Category != ResizeCategory.None)
            {
                // 倍率計算(この値をもとにリサイズする)
                scale = ResizeCategoryItem.Value.ResizeLongSideValue / readImageSize.Width;
                if (readImageSize.Width < readImageSize.Height)
                {
                    scale = ResizeCategoryItem.Value.ResizeLongSideValue / readImageSize.Height;
                }
            }

            var resizeWidth = (int)(readImageSize.Width * scale);
            var resizeHeight = (int)(readImageSize.Height * scale);

            ResizeSizeWidthText.Value = $"Width: {resizeWidth} [pixel]";
            ResizeSizeHeightText.Value = $"Height: {resizeHeight} [pixel]";
        }
    }
}