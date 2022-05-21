using Kchary.PhotoViewer.Models;
using Prism.Mvvm;
using FastEnumUtility;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExifInfoViewModel : BindableBase
    {
        /// <summary>
        /// Exif情報データを保持するリスト
        /// </summary>
        /// <remarks>
        /// Exif情報に表示するプロパティ要素数だけ配列を準備する
        /// </remarks>
        public ExifInfo[] ExifDataList { get; } = new ExifInfo[FastEnum.GetNames<PropertyType>().Count];

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExifInfoViewModel()
        {
            // Exif情報の基礎部分を作っておく。
            // 各Exif情報の値は、画像読み込み時に設定する
            ExifParser.CreateExifDefaultList(ExifDataList);
        }

        /// <summary>
        /// Exif情報を設定する
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="stopLoading">ロード停止フラグ</param>
        public void SetExif(string filePath, bool stopLoading)
        {
            ExifParser.SetExifDataFromFile(filePath, ExifDataList, stopLoading);
        }

        /// <summary>
        /// リストのデータからExif情報を削除
        /// </summary>
        /// <remarks>
        /// リストに含まれるExifプロパティ名は削除しない
        /// したがって、中身のパラメータだけを空文字にする
        /// </remarks>
        public void ClearExif()
        {
            foreach (var exifData in ExifDataList)
            {
                exifData.ExifParameterValue = "";
            }
        }
    }
}