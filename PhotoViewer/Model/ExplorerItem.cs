using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public sealed class ExplorerItem : TreeViewItem
    {
        /// <summary>
        /// Item information
        /// </summary>
        public string ExplorerItemPath { get; private set; }

        /// <summary>
        /// Internal directory information
        /// </summary>
        private readonly DirectoryInfo innerDirectory;

        /// <summary>
        /// Item expansion status
        /// </summary>
        private bool isExpand;

        /// <summary>
        /// Directory monitoring system watcher
        /// </summary>
        private FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="isDrive">Drive flag</param>
        public ExplorerItem(string path, bool isDrive)
        {
            // Set events and initial values.
            Expanded += ExplorerItem_Expanded;
            isExpand = false;
            ExplorerItemPath = path;

            // Set header information to be displayed in explorer.
            Header = isDrive ? CreateExplorerItemHeader(ExplorerItemPath, true) : CreateExplorerItemHeader(Path.GetFileName(ExplorerItemPath), false);

            // Directory information in the drive(if it exists, create an empty TreeItem)
            // Note: Create contents when expanding the tree to reduce memory consumption.
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
                // When an error occurs in access denial, logs are collected and the folder is skipped.
                App.LogException(ex);
            }

            // Start monitoring the directory.
            StartWatcher(path);
        }

        /// <summary>
        /// Set stack panel.
        /// </summary>
        /// <returns>Stack panel settings shown in TreeView.</returns>
        private static StackPanel CreateExplorerItemHeader(string path, bool isDrive)
        {
            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            // Generate Icon.
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
        /// Set Directory information to Tree.
        /// </summary>
        /// <returns>List of directory information</returns>
        private void UpdateDirectoryTree()
        {
            // Clear the set empty item.
            Items.Clear();

            // Regenerate the directory order by rearranging it in natural order.
            var sortDirectoryInfos = innerDirectory.GetDirectories().OrderBy(directory => directory, new NaturalDirectoryInfoNameComparer());

            foreach (var directory in sortDirectoryInfos)
            {
                // Get the first character of the file name.
                var fileNameFirst = Path.GetFileName(directory.FullName).Substring(0, 1);

                // If the first character is "$" or the attribute is hidden, it is skipped because it is a Windows special file.
                if (fileNameFirst == "$" || (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                var node = new ExplorerItem(directory.FullName, false);
                Items.Add(node);
            }
        }

        /// <summary>
        /// Generate the image of the icon displayed in TreeView.
        /// </summary>
        /// <param name="isDrive">Whether it is a drive</param>
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
        /// Event when Explorer tree is expanded
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">Argument information</param>
        private void ExplorerItem_Expanded(object sender, RoutedEventArgs e)
        {
            // Out of focus.
            Keyboard.ClearFocus();

            if (!isExpand)
            {
                // Set the directory information of the extraction destination in the child element.
                UpdateDirectoryTree();
            }

            isExpand = true;
        }

        /// <summary>
        /// Monitor information in the drive.
        /// </summary>
        /// <param name="path">Watch path</param>
        private void StartWatcher(string path)
        {
            // Set monitoring for each directory information.
            fileSystemWatcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*",
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            // Set for events that occur in directory monitoring
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;

            // Start monitoring.
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Reload.
            Application.Current.Dispatcher.Invoke(UpdateDirectoryTree);
        }
    }
}