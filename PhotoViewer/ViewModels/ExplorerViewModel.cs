using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExplorerViewModel : BindableBase
    {
        private ExplorerItem selectedItem;

        /// <summary>
        /// アイテム選択を変更したときのイベント
        /// </summary>
        public event EventHandler ChangeSelectItemEvent;

        /// <summary>
        /// ツリーに表示するアイテムリスト
        /// </summary>
        public List<ExplorerItem> ExplorerItems { get; } = new();

        /// <summary>
        /// 選択したアイテム情報
        /// </summary>
        public ExplorerItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value)
                {
                    return;
                }

                selectedItem = value;
                ChangeSelectItemEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// ドライブツリーを作成する
        /// </summary>
        public void CreateDriveTreeItem()
        {
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
    }
}