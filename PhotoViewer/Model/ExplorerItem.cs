using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class ExplorerItem : TreeViewItem
    {
        // アイテム情報
        public string ExplorerItemPath { get; private set; }

        // 内部ディレクトリ情報
        private readonly DirectoryInfo innerDirectory;

        // アイテムの展開状態
        private bool isExpand;

        // ディレクトリ監視用
        private FileSystemWatcher fileSystemWatcher;

        public ExplorerItem(string path, bool isDrive)
        {
            // イベントと初期値の設定
            Expanded += ExplorerItem_Expanded;
            isExpand = false;
            ExplorerItemPath = path;

            // Explorerに表示するヘッダー情報の設定
            if (isDrive)
            {
                Header = CreateExplorerItemHeader(ExplorerItemPath, true);
            }
            else
            {
                Header = CreateExplorerItemHeader(Path.GetFileName(ExplorerItemPath), false);
            }

            // ドライブ内のディレクトリ情報(存在する場合は、空のTreeItemを作成しておく)
            // ※ メモリ消費を抑えるため、中身はツリー展開時に作成する
            innerDirectory = new DirectoryInfo(ExplorerItemPath);

            try
            {
                if (innerDirectory.Exists && innerDirectory.GetDirectories().Length > 0)
                {
                    Items.Add(new TreeViewItem());
                }
            }
            catch (Exception ex)
            {
                // アクセス拒否などでエラーが発生した時は、ログ収集を行い、そのフォルダをスキップ
                App.LogException(ex);
            }

            // ディレクトリの監視をスタート
            StartWatcher(path);
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
                FontWeight = FontWeights.Normal,
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
            List<DirectoryInfo> sortDirectoryInfos = innerDirectory.GetDirectories().ToList();
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
            // フォーカスを外す
            Keyboard.ClearFocus();

            if (!isExpand)
            {
                // 展開先のディレクトリ情報を子要素に設定
                UpdateDirectoryTree();
            }

            isExpand = true;
        }

        /// <summary>
        /// ドライブ内の情報を監視する
        /// </summary>
        /// <param name="drive">監視するディレクトリ情報</param>
        private void StartWatcher(string path)
        {
            // ディレクトリ情報ごとに監視を設定
            fileSystemWatcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*",
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            // ディレクトリ監視で発生するイベントの設定
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;

            // 監視を開始
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // 再度読み込み直す
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                UpdateDirectoryTree();
            }));
        }
    }
}