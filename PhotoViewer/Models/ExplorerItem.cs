using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// エクスプローラーツリー情報表示用クラス
    /// </summary>
    public sealed partial class ExplorerItem : ObservableObject
    {
        /// <summary>
        /// アイテムパス
        /// </summary>
        public string ExplorerItemPath { get; }

        /// <summary>
        /// 表示名
        /// </summary>
        [ObservableProperty]
        private string displayName;

        /// <summary>
        /// アイコン
        /// </summary>
        [ObservableProperty]
        private BitmapImage icon;

        /// <summary>
        /// 子アイテム
        /// </summary>
        public ObservableCollection<ExplorerItem> Children { get; } = new();

        /// <summary>
        /// 内部ディレクトリ情報
        /// </summary>
        private readonly DirectoryInfo innerDirectory;

        /// <summary>
        /// 展開済みのアイテムかどうかのフラグ
        /// </summary>
        private bool isExpanded;

        /// <summary>
        /// 子要素があるかどうか
        /// </summary>
        [ObservableProperty]
        private bool hasChildren;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="isDrive">ドライブかどうかのフラグ</param>
        public ExplorerItem(string path, bool isDrive)
        {
            // 初期値の設定
            isExpanded = false;
            ExplorerItemPath = path;
            
            // 表示名を設定
            DisplayName = isDrive ? path : FileUtil.GetFileName(path, false);
            
            // アイコンを設定
            Icon = CreateTreeIcon(isDrive);

            // ドライブ内のディレクトリを作成
            // 展開時にツリーを作成することで、メモリ消費を抑える
            innerDirectory = new DirectoryInfo(ExplorerItemPath);

            try
            {
                if (innerDirectory.Exists && innerDirectory.GetDirectories().Length > 0)
                {
                    HasChildren = true;
                    // 仮の子要素を追加（遅延読み込み用）
                    Children.Add(null);
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }
        }

        /// <summary>
        /// ディレクトリのツリー表示を更新する
        /// </summary>
        public void ExpandItem()
        {
            if (isExpanded)
            {
                return;
            }

            Children.Clear();

            try
            {
                // 自然ソート順でディレクトリリストをソート
                foreach (var directory in innerDirectory.GetDirectories().OrderBy(directory => directory, new NaturalDirectoryInfoNameComparer()))
                {
                    // 1文字目の文字を確認
                    var fileName = FileUtil.GetFileName(directory.FullName, false);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }
                    
                    var fileNameFirst = fileName[..1];

                    // Windowsの特殊フォルダ以外を表示する
                    if (fileNameFirst == "$" ||
                        (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }
                    var node = new ExplorerItem(directory.FullName, false);
                    Children.Add(node);
                }
            }
            catch (Exception ex)
            {
                App.LogException(ex);
            }

            isExpanded = true;
        }

        /// <summary>
        /// ツリーに表示する各種アイコンを作成する
        /// </summary>
        /// <param name="isDrive">ドライブかどうかのフラグ</param>
        private static BitmapImage CreateTreeIcon(bool isDrive)
        {
            if (isDrive)
            {
                return WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SiidDrivefixed);
            }
            else
            {
                return WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SiidFolder);
            }
        }
    }
}