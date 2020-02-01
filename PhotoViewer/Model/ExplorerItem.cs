using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer.Model
{
    public class ExplorerItem : TreeViewItem, INotifyPropertyChanged
    {
        private enum ExplorerItemStatus
        {
            Expand,
            Contract,
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string treeItemName;
        public string TreeItemName
        {
            get { return treeItemName; }
            set
            {
                if (treeItemName != value)
                {
                    treeItemName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TreeItemName)));
                }
            }
        }

        private BitmapSource treeIcon;
        public BitmapSource TreeIcon
        {
            get { return treeIcon; }
            set
            {
                if (treeIcon != value)
                {
                    treeIcon = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TreeIcon)));
                }
            }
        }

        // 内部ディレクトリ情報
        private DirectoryInfo InnerDirectory;
        // アイテムの展開状態
        private ExplorerItemStatus ItemStatus;
        // フォルダ監視用
        private FileSystemWatcher FileSystemWatcher;

        public ExplorerItem(string path, bool isDrive)
        {
            // イベントと初期値の設定
            Expanded += ExplorerItem_Expanded;
            Selected += ExplorerItem_Selected;
            ItemStatus = ExplorerItemStatus.Contract;

            // ドライブ情報の設定
            this.Header = CreateExplorerItemHeader(path, isDrive);

            // ドライブ内のディレクトリ情報(存在する場合は、空のTreeItemを作成しておく)
            // ※ メモリ消費を抑えるため、中身はツリー展開時に作成する
            InnerDirectory = new DirectoryInfo(path);
            if (InnerDirectory.GetDirectories().Length > 0)
            {
                this.Items.Add(new TreeViewItem());
            }

            // ディレクトリ情報を監視
            SetDriveWatcher(path);
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
                TextWrapping = TextWrapping.Wrap,
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
                const string tempRecycleFileIndicator = "$";

                // 最初の文字が"$"だった場合は、Windowsの特殊ファイルのためスキップする
                if (fileNameFirst == tempRecycleFileIndicator)
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

        private void ExplorerItem_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ExplorerItem_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (ItemStatus)
            {
                case ExplorerItemStatus.Contract:
                    ItemStatus = ExplorerItemStatus.Expand;
                    UpdateDirectoryTree();
                    return;

                case ExplorerItemStatus.Expand:
                    ItemStatus = ExplorerItemStatus.Contract;
                    return;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// ドライブ内の情報を監視する
        /// </summary>
        /// <param name="drive">監視するドライブ情報</param>
        private void SetDriveWatcher(string path)
        {
            // ディレクトリ情報ごとに監視を設定
            FileSystemWatcher = new FileSystemWatcher();
            FileSystemWatcher.Path = path;
            FileSystemWatcher.Filter = "*";
            FileSystemWatcher.IncludeSubdirectories = false;
            FileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // ディレクトリ監視で発生するイベントの設定
            FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            FileSystemWatcher.Created += FileSystemWatcher_Changed;
            FileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            FileSystemWatcher.Renamed += FileSystemWatcher_Changed;

            // 監視を開始
            FileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
