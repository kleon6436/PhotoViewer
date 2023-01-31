using CommunityToolkit.Mvvm.ComponentModel;
using Kchary.PhotoViewer.Helpers;
using Kchary.PhotoViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kchary.PhotoViewer.ViewModels
{
    public sealed class ExplorerViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// ファイルシステム管理のウォッチャー
        /// </summary>
        private readonly FileSystemWatcher fileWatcher;

        /// <summary>
        /// 監視しているドライブ情報
        /// </summary>
        private string previousWatchDrive;

        /// <summary>
        /// アイテム選択を変更したときのイベント
        /// </summary>
        public EventHandler ChangeSelectItemEvent { get; set; }

        /// <summary>
        /// ツリーに表示するアイテムリスト
        /// </summary>
        public ObservableCollection<ExplorerItem> ExplorerItems { get; } = new();

        /// <summary>
        /// 表示中のフォルダパス
        /// </summary>
        public string ShowExplorerPath { get; private set; }

        private ExplorerItem selectedItem;
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
                if (previousWatchDrive != drive)
                {
                    UpdateWatcher(drive);
                    previousWatchDrive = drive;
                }

                ChangeSelectItemEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExplorerViewModel()
        {
            // 監視設定
            fileWatcher = new FileSystemWatcher
            {
                Filter = "*",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };

            fileWatcher.Changed += FolderOrFileChanged;
            fileWatcher.Created += FolderOrFileChanged;
            fileWatcher.Deleted += FolderOrFileChanged;
            fileWatcher.Renamed += FolderOrFileChanged;

            // 初期値設定
            previousWatchDrive = "";
            ShowExplorerPath = "";
        }

        /// <summary>
        /// Dispose処理
        /// </summary>
        public void Dispose() => fileWatcher.Dispose();

        /// <summary>
        /// ドライブツリーを作成(更新)する
        /// </summary>
        public void UpdateDriveTreeItem()
        {
            ExplorerItems.Clear();
            foreach (var drive in DriveInfo.GetDrives())
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

            FileUtil.GetAllParentPathList(previousFolderPath, parentPathList);
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
        /// ディレクトリに含まれるアイテムを取得する
        /// </summary>
        /// <param name="parentPath">親ディレクトリのパス</param>
        /// <param name="previousItem">前回展開したアイテム情報</param>
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
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Path = drive;

            // 監視開始
            try
            {
                fileWatcher.EnableRaisingEvents = true;
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
        private void FolderOrFileChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                UpdateDriveTreeItem();
                ExpandPreviousPath(ShowExplorerPath);
            });
        }
    }
}