using System;
using System.Linq;
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
    }
}
