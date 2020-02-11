using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Prism.Commands;
using PhotoViewer.Model;

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

        /** バージョンアップ時に有効化予定
        private bool isResizeImage;
        public bool IsResizeImage
        {
            get { return isResizeImage; }
            set { SetProperty(ref isResizeImage, value); }
        }

        private string resizeWidth;
        public string ResizeWidth
        {
            get { return resizeWidth; }
            set { SetProperty(ref resizeWidth, value); }
        }

        private string resizeHeight;
        public string ResizeHeight
        {
            get { return resizeHeight; }
            set { SetProperty(ref resizeHeight, value); }
        }

        private bool isExifDelete;
        public bool IsExifDelete
        {
            get { return isExifDelete; }
            set { SetProperty(ref isExifDelete, value); }
        }
        **/

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

        // 編集対象のファイルパス
        private string EditFilePath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImageEditToolViewModel()
        {
            ImageSaveQualityItems.Add(new ImageQuality("高画質", 90));
            ImageSaveQualityItems.Add(new ImageQuality("標準", 80));
            ImageSaveQualityItems.Add(new ImageQuality("低画質", 60));
            SelectedQuality = ImageSaveQualityItems.First();

            ImageFormItems.Add(new ImageForm("Jpeg", ImageForm.ImageForms.Jpeg));
            ImageFormItems.Add(new ImageForm("Png", ImageForm.ImageForms.Png));
            ImageFormItems.Add(new ImageForm("Bmp", ImageForm.ImageForms.Bmp));
            SelectedForm = ImageFormItems.First();

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

        }
    }

    public class ImageQuality
    {
        public enum SaveQuality
        {
            High,
            Mediam,
            Low,
        }

        public string Name { get; set; }
        public int QualityValue { get; set; }

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
        }

        public string Name { get; set; }
        public ImageForms Form { get; set; }

        public ImageForm(string name, ImageForms imageForm)
        {
            this.Name = name;
            this.Form = imageForm;
        }
    }
}
