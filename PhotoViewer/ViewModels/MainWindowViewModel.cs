using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoViewer.ViewModels
{
    public class MainWindowViewModel
    {
        public ExplorerViewModel ExplorerViewModel { get; set; }

        public MainWindowViewModel()
        {
            ExplorerViewModel = new ExplorerViewModel();
            UpdateExplorerTree();
        }

        private void UpdateExplorerTree()
        {
            List<DriveInfo> allDriveList = DriveInfo.GetDrives().ToList();
            ExplorerViewModel.CreateDriveTreeItem(allDriveList);
        }
    }
}
