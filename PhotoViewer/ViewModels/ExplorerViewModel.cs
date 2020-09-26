using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExplorerViewModel : BindableBase
    {
        public event EventHandler ChangeSelectItemEvent;

        public ObservableCollection<ExplorerItem> ExplorerItems { get; } = new ObservableCollection<ExplorerItem>();

        private ExplorerItem selectedItem;

        /// <summary>
        /// Selected item information
        /// </summary>
        public ExplorerItem SelectedItem
        {
            get => selectedItem;
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
        }

        /// <summary>
        /// Create a TreeItem for drive.
        /// </summary>
        /// <param name="driveList"></param>
        public void CreateDriveTreeItem()
        {
            DriveInfo[] allDriveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDriveList)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                // Create drive treeitem
                ExplorerItem driveItem = new ExplorerItem(drive.Name, true);
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

            foreach (string parentPath in parentPathList)
            {
                if (count == 0)
                {
                    // Check drive information and expand tree.
                    string previousDrive = parentPath;
                    ExplorerItem driveItem = ExplorerItems.Where(item => item.ExplorerItemPath == previousDrive).First();
                    if (driveItem == null)
                    {
                        return;
                    }

                    driveItem.IsExpanded = true;

                    previousItem = driveItem;
                }
                else if (count == parentPathList.Count - 1)
                {
                    // Confirm directory information and select item.
                    ExplorerItem directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null)
                    {
                        return;
                    }

                    directoryItem.IsSelected = true;
                }
                else
                {
                    // Confirm directory information and select item
                    ExplorerItem directoryItem = GetDirectoryItem(parentPath, previousItem);
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
        /// Get all parent directory names.
        /// </summary>
        private void GetAllParentPathList(string previousFolderPath, List<string> parentPathList)
        {
            // Get the lowest directory name.
            DirectoryInfo directoryInfo = new DirectoryInfo(previousFolderPath);
            parentPathList.Add(directoryInfo.FullName);

            // Obtain parent directories in order and add to the list.
            GetParentPathList(previousFolderPath, parentPathList);
        }

        /// <summary>
        /// Get list of parent directory names.
        /// </summary>
        private void GetParentPathList(string directoryPath, List<string> parentPathList)
        {
            DirectoryInfo parentDirectory = Directory.GetParent(directoryPath);
            if (parentDirectory == null)
            {
                return;
            }

            parentPathList.Add(parentDirectory.FullName);

            GetParentPathList(parentDirectory.FullName, parentPathList);
        }

        /// <summary>
        /// Get ExplorerItem of directory.
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