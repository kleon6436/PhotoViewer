using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Prism.Mvvm;
using PhotoViewer.Model;

namespace PhotoViewer.ViewModels
{
    public class ExplorerViewModel : BindableBase
    {
        public event EventHandler ChangeSelectItemEvent;

        private ObservableCollection<ExplorerItem> explorerItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get { return explorerItems; }
            set { SetProperty(ref explorerItems, value); }
        }

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

        // ドライブ監視用
        private FileSystemWatcher FileSystemWatcher;

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
        public void CreateDriveTreeItem(List<DriveInfo> driveList)
        {
            foreach (var drive in driveList)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                // ドライブのTreeItemを作成
                var driveItem = new ExplorerItem(drive.Name, true);
                ExplorerItems.Add(driveItem);

                // ディレクトリ情報を監視
                StartWatcher(drive);
            }
        }

        private void DriveItem_ExplorerSelectEvent(ExplorerItem selectExplorerItem)
        {
            
        }

        /// <summary>
        /// ドライブ内の情報を監視する
        /// </summary>
        /// <param name="drive">監視するドライブ情報</param>
        private void StartWatcher(DriveInfo drive)
        {
            // ディレクトリ情報ごとに監視を設定
            FileSystemWatcher = new FileSystemWatcher();
            FileSystemWatcher.Path = drive.Name;
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

        private void StopWatcher()
        {
            if (FileSystemWatcher != null)
            {
                FileSystemWatcher.EnableRaisingEvents = false;
                FileSystemWatcher.Dispose();
            }
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //throw new System.NotImplementedException();
        }
    }
}
