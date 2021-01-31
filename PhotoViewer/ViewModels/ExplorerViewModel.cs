using Kchary.PhotoViewer.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExplorerViewModel : BindableBase
    {
        public event EventHandler ChangeSelectItemEvent;

        public ObservableCollection<ExplorerItem> ExplorerItems { get; } = new();

        private ExplorerItem selectedItem;

        /// <summary>
        /// Selected item information
        /// </summary>
        public ExplorerItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                ChangeSelectItemEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create a TreeItem for drive.
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

                // Create drive tree item
                var driveItem = new ExplorerItem(drive.Name, true);
                ExplorerItems.Add(driveItem);
            }
        }

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
                    // Check drive information and expand tree.
                    var previousDrive = parentPath;
                    var driveItem = ExplorerItems.First(item => item.ExplorerItemPath == previousDrive);
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
                    var directoryItem = GetDirectoryItem(parentPath, previousItem);
                    if (directoryItem == null)
                    {
                        return;
                    }

                    directoryItem.IsSelected = true;
                }
                else
                {
                    // Confirm directory information and select item
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
        /// Get all parent directory names.
        /// </summary>
        private static void GetAllParentPathList(string previousFolderPath, ICollection<string> parentPathList)
        {
            // Get the lowest directory name.
            var directoryInfo = new DirectoryInfo(previousFolderPath);
            parentPathList.Add(directoryInfo.FullName);

            // Obtain parent directories in order and add to the list.
            GetParentPathList(previousFolderPath, parentPathList);
        }

        /// <summary>
        /// Get list of parent directory names.
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
        /// Get ExplorerItem of directory.
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