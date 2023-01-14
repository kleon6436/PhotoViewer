using System;
using System.Collections.Generic;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// サポート対象のファイル拡張子のタイプ定義
    /// </summary>
    public enum FileExtensionType
    {
        Jpeg,
        Bmp,
        Png,
        Tiff,
        Gif,
        Dng,
        Nef,
        Unknown
    }

    /// <summary>
    /// アプリで使用する定数管理クラス
    /// </summary>
    /// <remarks>
    /// アプリ全体で使用されている定数が固定値で記述されている
    /// </remarks>
    public static class Const
    {
        #region  アプリ設定ファイルのXMLに記述する要素名

        public static readonly string PreviousFolderElemName = "previous_folder";
        public static readonly string PreviousPathElemName = "previous_path";
        public static readonly string LinkAppElemName = "linkage_app";
        public static readonly string LinkAppDataName = "linkage_app_data";
        public static readonly string LinkAppNameElemName = "name";
        public static readonly string LinkAppPathElemName = "path";
        public static readonly string PlacementElemName = "window_placement";
        public static readonly string PlacementTopElemName = "top";
        public static readonly string PlacementLeftElemName = "left";
        public static readonly string PlacementRightElemName = "right";
        public static readonly string PlacementBottomElemName = "buttom";
        public static readonly string PlacementMaxPosXElemName = "maxPosX";
        public static readonly string PlacementMaxPosYElemName = "maxPosY";
        public static readonly string PlacementMinPosXElemName = "minPosX";
        public static readonly string PlacementMinPosYElemName = "minPosY";
        public static readonly string PlacementFlagElemName = "windowFlag";
        public static readonly string PlacementSwElemName = "sw";

        #endregion

        #region アプリがサポートする拡張子設定

        /// <summary>
        /// サポートする画像の拡張子名
        /// </summary>
        public static readonly string[] SupportPictureExtensions = { ".jpg", ".bmp", ".png", ".tiff", ".tif", ".gif", ".dng", ".nef" };

        /// <summary>
        /// サポートするRaw画像の拡張子名
        /// </summary>
        public static readonly string[] SupportRawPictureExtensions = { ".dng", ".nef" };

        /// <summary>
        /// サポートしている拡張子の文字列とEnumのマップ
        /// </summary>
        public static readonly IReadOnlyDictionary<string, FileExtensionType> SupportExtensionMap = new Dictionary<string, FileExtensionType>()
        {
            { ".jpg", FileExtensionType.Jpeg },
            { ".bmp", FileExtensionType.Bmp },
            { ".png", FileExtensionType.Png },
            { ".tiff", FileExtensionType.Tiff },
            { ".tif", FileExtensionType.Tiff },
            { ".gif", FileExtensionType.Gif },
            { ".dng", FileExtensionType.Dng },
            { ".nef", FileExtensionType.Nef }
        };

        #endregion

        /// <summary>
        /// アプリケーション設定ファイルの絶対パス
        /// </summary>
        public static readonly string AppConfigFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KcharyPhotoViewer\\Setting.conf";
    }
}
