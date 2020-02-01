using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using PhotoViewer.Model;

namespace PhotoViewer.ViewModels
{
    public class ExplorerViewModel : BindableBase
    {
        private ObservableCollection<ExplorerItem> explorerItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get { return explorerItems; }
            set { SetProperty(ref explorerItems, value); }
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
            }
        }
    }
}
