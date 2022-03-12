using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    /// <summary>
    /// エクスプローラーツリー情報表示用クラス
    /// </summary>
    public sealed class ExplorerItem : TreeViewItem
    {
        /// <summary>
        /// アイテムパス
        /// </summary>
        public string ExplorerItemPath { get; }

        /// <summary>
        /// 内部ディレクトリ情報
        /// </summary>
        private readonly DirectoryInfo innerDirectory;

        /// <summary>
        /// 展開済みのアイテムかどうかのフラグ
        /// </summary>
        private bool isExpand;

        /// <summary>
        /// ファイルシステム管理のウォッチャー
        /// </summary>
        private FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="isDrive">ドライブかどうかのフラグ</param>
        public ExplorerItem(string path, bool isDrive)
        {
            // 初期値の設定
            Expanded += ExplorerItem_Expanded;
            isExpand = false;
            ExplorerItemPath = path;

            // エクスプローラーツリーに表示するヘッダー情報を作成
            Header = isDrive ? CreateExplorerItemHeader(ExplorerItemPath, true) : CreateExplorerItemHeader(Path.GetFileName(ExplorerItemPath), false);

            // ドライブ内のディレクトリを作成
            // 展開時にツリーを作成することで、メモリ消費を抑える
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
                App.LogException(ex);
            }

            // 監視開始
            StartWatcher(path);
        }

        /// <summary>
        /// スタックパネルのレイアウトを作成する
        /// </summary>
        /// <returns>スタックパネルのレイアウト情報</returns>
        private static StackPanel CreateExplorerItemHeader(string path, bool isDrive)
        {
            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            var iconSource = CreateTreeIcon(isDrive);
            stackPanel.Children.Add(new Image()
            {
                Source = iconSource,
                Width = 20,
                Height = 20
            });
            stackPanel.Children.Add(new TextBlock()
            {
                Text = path,
                FontWeight = FontWeights.Normal,
                ToolTip = path,
                Margin = new Thickness(2.5, 0, 0, 0)
            });
            stackPanel.Margin = new Thickness(0, 5, 0, 0);

            return stackPanel;
        }

        /// <summary>
        /// ディレクトリのツリー表示を更新する
        /// </summary>
        private void UpdateDirectoryTree()
        {
            Items.Clear();

            // 自然ソート順でディレクトリリストをソート
            var sortDirectoryInfos = innerDirectory.GetDirectories().OrderBy(directory => directory, new NaturalDirectoryInfoNameComparer());

            foreach (var directory in sortDirectoryInfos)
            {
                // 1文字目の文字を確認
                var fileNameFirst = Path.GetFileName(directory.FullName)[..1];

                // Windowsの特殊フォルダ以外を表示する
                if (fileNameFirst == "$" ||
                    (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }
                var node = new ExplorerItem(directory.FullName, false);
                Items.Add(node);
            }
        }

        /// <summary>
        /// ツリーに表示する各種アイコンを作成する
        /// </summary>
        /// <param name="isDrive">ドライブかどうかのフラグ</param>
        private static BitmapSource CreateTreeIcon(bool isDrive)
        {
            if (isDrive)
            {
                var iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SiidDrivefixed);
                return iconImage;
            }
            else
            {
                var iconImage = WindowsIconCreator.GetWindowsIcon(WindowsIconCreator.StockIconId.SiidFolder);
                return iconImage;
            }
        }

        /// <summary>
        /// ツリー表示でアイテムが展開されたときの動作
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">引数情報</param>
        private void ExplorerItem_Expanded(object sender, RoutedEventArgs e)
        {
            // キーボードフォーカスを外す
            Keyboard.ClearFocus();

            if (!isExpand)
            {
                UpdateDirectoryTree();
            }

            isExpand = true;
        }

        /// <summary>
        /// フォルダの変更などの監視を開始する
        /// </summary>
        /// <param name="path">監視対象のフォルダパス</param>
        private void StartWatcher(string path)
        {
            // 監視対象のフォルダを設定
            fileSystemWatcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*",
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;

            // 監視開始
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// ファイルシステムのウォッチャが変更検知したときの動作
        /// </summary>
        /// <remarks>
        /// ディレクトリツリーの表示を更新する
        /// </remarks>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">引数情報</param>
        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(UpdateDirectoryTree);
        }
    }
}