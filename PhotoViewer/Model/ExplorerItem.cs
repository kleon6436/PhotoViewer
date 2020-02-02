using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class ExplorerItem : TreeViewItem
    {
        // アイテム情報
        public string ExplorerItemPath { get; private set; }
        // 内部ディレクトリ情報
        private DirectoryInfo InnerDirectory;
        // アイテムの展開状態
        private bool IsExpand;

        public ExplorerItem(string path, bool isDrive)
        {
            // イベントと初期値の設定
            Expanded += ExplorerItem_Expanded;
            IsExpand = false;
            ExplorerItemPath = path;

            // Explorerに表示するヘッダー情報の設定
            if (isDrive)
            {
                this.Header = CreateExplorerItemHeader(ExplorerItemPath, true);
            }
            else
            {
                this.Header = CreateExplorerItemHeader(Path.GetFileName(ExplorerItemPath), false);
            }

            // ドライブ内のディレクトリ情報(存在する場合は、空のTreeItemを作成しておく)
            // ※ メモリ消費を抑えるため、中身はツリー展開時に作成する
            InnerDirectory = new DirectoryInfo(ExplorerItemPath);

            try
            {
                if (InnerDirectory.Exists && InnerDirectory.GetDirectories().Length > 0)
                {
                    this.Items.Add(new TreeViewItem());
                }
            }
            catch (Exception ex)
            {
                // アクセス拒否などでエラーが発生した時は、ログ収集を行い、そのフォルダをスキップ
                App.LogException(ex);
            }
        }

        /// <summary>
        /// スタックパネルの設定
        /// </summary>
        /// <returns>TreeViewに見せるスタックパネルの設定</returns>
        private StackPanel CreateExplorerItemHeader(string path, bool isDrive)
        {
            StackPanel stackpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            // Iconを生成
            BitmapSource iconSource = CreateTreeIcon(isDrive);

            stackpanel.Children.Add(new Image()
            {
                Source = iconSource,
                Width = 20,
                Height = 20
            });
            stackpanel.Children.Add(new TextBlock()
            {
                Text = path,
                ToolTip = path,
                Margin = new Thickness(2.5, 0, 0, 0)
            });
            stackpanel.Margin = new Thickness(0, 5, 0, 0);

            return stackpanel;
        }

        /// <summary>
        /// Directoryの情報をTreeに設定する
        /// </summary>
        /// <returns>ディレクトリ情報のリスト</returns>
        private void UpdateDirectoryTree()
        {
            // 設定していた空のアイテムをクリア
            Items.Clear();

            // ディレクトリの順番を自然順ソートで並び変えて再生成
            List<DirectoryInfo> sortDirectoryInfos = InnerDirectory.GetDirectories().ToList();
            sortDirectoryInfos = new List<DirectoryInfo>(sortDirectoryInfos.OrderBy(directory => directory, new NaturalDirectoryInfoNameComparer()));

            foreach (var directory in sortDirectoryInfos)
            {
                // ファイル名の最初の文字を取得
                string fileNameFirst = Path.GetFileName(directory.FullName).Substring(0, 1);

                // 最初の文字が"$"だった場合や属性が隠し状態だった場合は、Windowsの特殊ファイルのためスキップする
                if (fileNameFirst == "$" || (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                var node = new ExplorerItem(directory.FullName, false);
                Items.Add(node);
            }
        }

        /// <summary>
        /// TreeViewで表示するアイコンの画像を生成するメソッド
        /// </summary>
        /// <param name="isDrive">ドライブかどうか</param>
        private BitmapSource CreateTreeIcon(bool isDrive)
        {
            if (isDrive)
            {
                BitmapSource iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SIID_DRIVEFIXED);
                return iconImage;
            }
            else
            {
                BitmapSource iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SIID_FOLDER);
                return iconImage;
            }
        }

        /// <summary>
        /// Explorerのツリー展開時
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">引数情報</param>
        private void ExplorerItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (!IsExpand)
            {
                // 展開先のディレクトリ情報を子要素に設定
                UpdateDirectoryTree();
            }

            IsExpand = true;
        }
    }
}
