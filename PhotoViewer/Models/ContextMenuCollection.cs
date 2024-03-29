﻿using CommunityToolkit.Mvvm.ComponentModel;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{
    public sealed class ContextMenuCollection : ObservableObject
    {
        /// <summary>
        /// コンテキストメニューの情報レコード
        /// </summary>
        /// <param name="DisplayName">表示名</param>
        /// <param name="ContextIcon">アイコン</param>
        public record ContextMenu(string DisplayName, BitmapSource ContextIcon);

        /// <summary>
        /// コンテキストメニューに表示するリスト
        /// </summary>
        public ObservableCollection<ContextMenu> ContextMenuList { get; } = new();

        /// <summary>
        /// コンテキストメニューの表示非表示フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsShowContextMenu { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ContextMenuCollection()
        {
        }

        /// <summary>
        /// 設定ファイルのコンテキストメニューを設定する
        /// </summary>
        public void SetContextMenuFromConfigData()
        {
            var linkageAppList = AppConfig.GetInstance().GetAvailableRegisterApps();
            if (linkageAppList.Length == 0)
            {
                return;
            }

            foreach (var linkageApp in linkageAppList)
            {
                AddContextMenu(linkageApp);
            }

            IsShowContextMenu.Value = ContextMenuList.Any();
        }

        /// <summary>
        /// コンテキストメニューの再読み込み
        /// </summary>
        /// <param name="sender">SettingViewModel</param>
        /// <param name="e">引数情報</param>
        public void ReloadContextMenu(object sender, EventArgs e)
        {
            // コンテキストメニューをクリア
            ContextMenuList.Clear();
            IsShowContextMenu.Value = false;

            // 登録アプリをコンテキストメニューに再登録
            SetContextMenuFromConfigData();
        }

        /// <summary>
        /// コンテキストメニューを読み込み、リストに追加する
        /// </summary>
        /// <param name="linkageApp">連携アプリ情報</param>
        private void AddContextMenu(RegisterApp linkageApp)
        {
            var appIcon = Icon.ExtractAssociatedIcon(linkageApp.AppPath);
            if (appIcon == null)
            {
                return;
            }
            var iconBitmapSource = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            iconBitmapSource.Freeze();

            // コンテキストメニューに追加
            var contextMenu = new ContextMenu(linkageApp.AppName, iconBitmapSource);
            ContextMenuList.Add(contextMenu);
        }
    }
}
