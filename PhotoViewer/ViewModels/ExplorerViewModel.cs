using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExplorerViewModel : BindableBase
    {
        /// <summary>
        /// アイテム選択を変更したときのイベント
        /// </summary>
        public event EventHandler ChangeSelectItemEvent;

        /// <summary>
        /// ツリーに表示するアイテムリスト
        /// </summary>
        public ObservableCollection<ExplorerItem> ExplorerItems { get; } = new();

        /// <summary>
        /// ファイルシステム管理のウォッチャー
        /// </summary>
        public FileSystemWatcher FileWatcher { get; }

        /// <summary>
        /// 監視しているドライブ情報
        /// </summary>
        public string PreviousWatchDrive { get; private set; }

        /// <summary>
        /// 表示中のフォルダパス
        /// </summary>
        public string ShowExplorerPath { get; private set; }

        /// <summary>
        /// 選択中のツリーアイテム
        /// </summary>
        private ExplorerItem selectedItem;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExplorerViewModel()
        {
            // 監視設定
            FileWatcher = new FileSystemWatcher
            {
                Filter = "*",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };

            FileWatcher.Changed += FileSystemWatcher_Changed;
            FileWatcher.Created += FileSystemWatcher_Changed;
            FileWatcher.Deleted += FileSystemWatcher_Changed;
            FileWatcher.Renamed += FileSystemWatcher_Changed;

            // 初期値設定
            PreviousWatchDrive = "";
            ShowExplorerPath = "";
        }

        /// <summary>
        /// 選択したアイテム情報
        /// </summary>
        public ExplorerItem SelectedItem
        {
            get => selectedItem;
            set
            {
                // 同じアイテムを選択している、もしくは、表示中のパスと同じだった場合は更新しない
                if (selectedItem == value || ShowExplorerPath == value.ExplorerItemPath)
                {
                    return;
                }

                selectedItem = value;
                ShowExplorerPath = selectedItem.ExplorerItemPath;

                var drive = Path.GetPathRoot(ShowExplorerPath);
                if (PreviousWatchDrive != drive)
                {
                    UpdateWatcher(drive);
                    PreviousWatchDrive = drive;
                }

                ChangeSelectItemEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// ドライブツリーを作成(更新)する
        /// </summary>
        public void UpdateDriveTreeItem()
        {
            ExplorerItems.Clear();
            var allDriveList = DriveInfo.GetDrives();

            foreach (var drive in allDriveList)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                var driveItem = new ExplorerItem(drive.Name, true);
                ExplorerItems.Add(driveItem);
            }
        }

        /// <summary>
        /// 前回表示していたフォルダパスを展開する
        /// </summary>
        /// <param name="previousFolderPath">前回表示していたフォルダパス</param>
        public void ExpandPreviousPath(string previousFolderPath)
        {
            var parentPathList = new List<string>();

            GetAllParentPathList(previousFolderPath, parentPathList);
            parentPathList.Reverse();

            var count = 0;
            ExplorerItem previousItem = null;

            foreach (var parentPath in parentPathList)
            {
                if (count == 0)
                {
                    // ドライブの情報を確認し、ツリーを展開する
                    var previousDrive = parentPath;
                    var driveItem = ExplorerItems.First(item => item.ExplorerItemPath == previousDrive);

                    driveItem.IsExpanded = true;

                    previousItem = driveItem;
                }
                else if (count == parentPathList.Count - 1)
                {
                    // 末尾のフォルダは選択状態にする
                    var directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null)
                    {
                        return;
                    }

                    directoryItem.IsSelected = true;
                }
                else
                {
                    var directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null)
                    {
                        return;
                    }

                    directoryItem.IsExpanded = true;
                    previousItem = directoryItem;
                }

                count++;
            }
        }

        /// <summary>
        /// すべての親ディレクトリのリストを取得する
        /// </summary>
        private static void GetAllParentPathList(string previousFolderPath, ICollection<string> parentPathList)
        {
            var directoryInfo = new DirectoryInfo(previousFolderPath);
            parentPathList.Add(directoryInfo.FullName);
            GetParentPathList(previousFolderPath, parentPathList);
        }

        /// <summary>
        /// 親ディレクトリのパスを取得する
        /// </summary>
        private static void GetParentPathList(string directoryPath, ICollection<string> parentPathList)
        {
            while (true)
            {
                var parentDirectory = Directory.GetParent(directoryPath);
                if (parentDirectory == null)
                {
                    return;
                }

                parentPathList.Add(parentDirectory.FullName);
                directoryPath = parentDirectory.FullName;
            }
        }

        /// <summary>
        /// ディレクトリに含まれるアイテムを取得する
        /// </summary>
        private static ExplorerItem GetDirectoryItem(string parentPath, ItemsControl previousItem)
        {
            var previousDirectory = parentPath;
            var explorerItemList = new List<ExplorerItem>();
            explorerItemList.AddRange(previousItem.Items.OfType<ExplorerItem>());

            return explorerItemList.First(item => item.ExplorerItemPath == previousDirectory);
        }

        /// <summary>
        /// フォルダの変更などの監視対象のパスを更新する
        /// </summary>
        /// <param name="drive">監視対象のドライブパス</param>
        private void UpdateWatcher(string drive)
        {
            FileWatcher.EnableRaisingEvents = false;
            FileWatcher.Path = drive;

            // 監視開始
            try
            {
                FileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                // 監視できるフォルダだけ監視するので、ログだけ収集して終了
                App.LogException(ex);
            }
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
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                UpdateDriveTreeItem();
                ExpandPreviousPath(ShowExplorerPath);
            });
        }
    }
}