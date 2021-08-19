using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Windows;

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
        public ExifInfo[] ExifDataList { get; } = new ExifInfo[Enum.GetNames<PropertyType>().Length];

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
        /// <remarks>
        /// 基本的に別スレッドから呼ばれる仕組みのため、UIスレッドで動くようにしている
        /// </remarks>
        /// <param name="filePath">画像ファイルパス</param>
        public void SetExif(string filePath)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ExifParser.SetExifDataFromFile(filePath, ExifDataList);
            });
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