using PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PhotoViewer.ViewModels
{
    public class ExplorerViewModel : BindableBase
    {
        public event EventHandler ChangeSelectItemEvent;

        public ObservableCollection<ExplorerItem> ExplorerItems { get; } = new ObservableCollection<ExplorerItem>();

        private ExplorerItem selectedItem;

        /// <summary>
        /// 選択されたアイテム情報
        /// </summary>
        public ExplorerItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    ChangeSelectItemEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Constractor
        /// </summary>
        public ExplorerViewModel()
        {
            // 初期値設定
            ExplorerItems.Clear();
        }

        /// <summary>
        /// ドライブのTreeItemを作成する
        /// </summary>
        /// <param name="driveList"></param>
        public void CreateDriveTreeItem()
        {
            List<DriveInfo> allDriveList = DriveInfo.GetDrives().ToList();

            foreach (var drive in allDriveList)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                // ドライブのTreeItemを作成
                var driveItem = new ExplorerItem(drive.Name, true);
                ExplorerItems.Add(driveItem);
            }
        }

        public void ExpandPreviousPath(string previousFolderPath)
        {
            List<string> parentPathList = new List<string>();

            GetAllParentPathList(previousFolderPath, parentPathList);
            parentPathList.Reverse();

            int count = 0;
            ExplorerItem previousItem = null;

            foreach (var parentPath in parentPathList)
            {
                if (count == 0)
                {
                    // ドライブ情報を確認し、ツリーを展開
                    string previousDrive = parentPath;
                    var driveItem = ExplorerItems.Where(item => item.ExplorerItemPath == previousDrive).First();
                    if (driveItem == null) return;
                    driveItem.IsExpanded = true;

                    previousItem = driveItem;
                }
                else if (count == parentPathList.Count - 1)
                {
                    // ディレクトリ情報を確認し、アイテムを選択
                    ExplorerItem directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null) return;
                    directoryItem.IsSelected = true;
                }
                else
                {
                    // ディレクトリ情報を確認し、ツリーを展開
                    ExplorerItem directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null) return;
                    directoryItem.IsExpanded = true;

                    previousItem = directoryItem;
                }

                count++;
            }
        }

        /// <summary>
        /// 親のディレクトリ名を全て取得する
        /// </summary>
        private void GetAllParentPathList(string previousFolderPath, List<string> parentPathList)
        {
            // 最下層のディレクトリ名を取得
            var directoryInfo = new DirectoryInfo(previousFolderPath);
            parentPathList.Add(directoryInfo.FullName);

            // 親ディレクトリを順番に取得し、リストに追加していく
            GetParentPathList(previousFolderPath, parentPathList);
        }

        /// <summary>
        /// 親のディレクトリ名のリストを取得する
        /// </summary>
        private void GetParentPathList(string directoryPath, List<string> parentPathList)
        {
            var parentDirectory = Directory.GetParent(directoryPath);
            if (parentDirectory == null) return;
            parentPathList.Add(parentDirectory.FullName);

            GetParentPathList(parentDirectory.FullName, parentPathList);
        }

        /// <summary>
        /// ディレクトリのExplorerItemを取得する
        /// </summary>
        private ExplorerItem GetDirectoryItem(string parentPath, ExplorerItem previousItem)
        {
            string previousDirectory = parentPath;
            List<ExplorerItem> explorerItemList = new List<ExplorerItem>();
            explorerItemList.AddRange(previousItem.Items.OfType<ExplorerItem>());

            return explorerItemList.Where(item => item.ExplorerItemPath == previousDirectory).First();
        }
    }
}