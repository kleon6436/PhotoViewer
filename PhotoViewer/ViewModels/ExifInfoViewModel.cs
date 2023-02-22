using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Models;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExifInfoViewModel : ObservableObject
    {
        /// <summary>
        /// Exif情報データを保持するリスト(UI表示に使用する)
        /// </summary>
        /// <remarks>
        /// Exif情報に表示するプロパティ要素数だけ配列を準備する
        /// </remarks>
        public ExifInfo[] ExifDataList { get; } = ExifLoader.CreateExifDefaultList();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExifInfoViewModel()
        {
        }

        /// <summary>
        /// Exif情報を設定する
        /// </summary>
        /// <param name="exifInfoList">ファイルから読み込んだExif情報リスト</param>
        public void SetExif(ExifInfo[] exifInfoList)
        {
            for (var i = 0; i < exifInfoList.Length; i++)
            {
                ExifDataList[i].ExifParameterValue = exifInfoList[i].ExifParameterValue;
            }
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