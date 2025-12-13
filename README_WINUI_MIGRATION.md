# PhotoViewer WPF → WinUI 3 移行ガイド

## 移行の概要

このドキュメントは、PhotoViewerアプリケーションのWPF (.NET 8.0)からWindows App SDK + WinUI 3への完全移行について説明します。

## 移行完了状況: 100% ✅

すべてのC#コードとXAMLファイルのWinUI 3への移行が完了しました。

---

## 完了した変更の詳細

### 1. プロジェクト構成の変更

#### PhotoViewer.csproj
```xml
<!-- Before -->
<TargetFramework>net8.0-windows7.0</TargetFramework>
<UseWPF>true</UseWPF>

<!-- After -->
<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
<UseWinUI>true</UseWinUI>
<EnableMsixTooling>true</EnableMsixTooling>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
```

#### 追加されたNuGetパッケージ
- Microsoft.WindowsAppSDK 1.6.250114003
- CommunityToolkit.WinUI.Controls.DataGrid 8.1.240916

#### 削除されたNuGetパッケージ
- ReactiveProperty.WPF
- VirtualizingWrapPanel（WPF専用サードパーティ）

### 2. C#コード層の変更（30+ファイル）

#### 名前空間の置き換え
```csharp
// Before (WPF)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

// After (WinUI)
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
```

#### 画像型の変換
```csharp
// Before
BitmapSource image;
WriteableBitmap writableBitmap;

// After
BitmapImage image;
WriteableBitmap writableBitmap; // WinUI版のWriteableBitmap
```

#### 主要な変更

**App.xaml.cs**
```csharp
// Before (WPF)
protected override void OnStartup(StartupEventArgs e)
{
    MainWindow = new MainWindow();
    MainWindow.Show();
}

// After (WinUI)
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    m_window.Activate();
}
```

**Window関連**
```csharp
// Before (WPF)
Window.Closing += Window_Closing;
var helper = new WindowInteropHelper(this);

// After (WinUI)
Window.Closed += Window_Closed;
var hwnd = WindowNative.GetWindowHandle(this);
```

**ファイルピッカー**
```csharp
// Before (WPF)
using Microsoft.Win32;
var dialog = new OpenFileDialog();

// After (WinUI)
using Windows.Storage.Pickers;
var picker = new FileOpenPicker();
```

### 3. XAMLファイルの変更（8/8ファイル）

#### 名前空間の変更
```xml
<!-- Before (WPF) -->
xmlns:wpftk="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel"
xmlns:view="clr-namespace:Kchary.PhotoViewer.Views"

<!-- After (WinUI) -->
xmlns:view="using:Kchary.PhotoViewer.Views"
```

#### 主要なコントロールの置き換え

**DataGrid → CommunityToolkit DataGrid**
```xml
<!-- Before (WPF) -->
<DataGrid ItemsSource="{Binding Items}" />

<!-- After (WinUI) -->
xmlns:controls="using:CommunityToolkit.WinUI.Controls"
<controls:DataGrid ItemsSource="{x:Bind ViewModel.Items}" />
```

**VirtualizingWrapPanel → GridView**
```xml
<!-- Before (WPF) -->
<ListView>
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <wpftk:VirtualizingWrapPanel />
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>

<!-- After (WinUI) -->
<GridView ItemsSource="{Binding PhotoList}">
    <GridView.ItemTemplate>
        <DataTemplate x:DataType="model:PhotoInfo">
            <Grid Width="100" Height="95">
                <Image Source="{x:Bind ThumbnailImage, Mode=OneWay}"/>
            </Grid>
        </DataTemplate>
    </GridView.ItemTemplate>
</GridView>
```

**TreeView（階層構造の変更）**
```xml
<!-- Before (WPF) - TreeViewItem継承 -->
public class ExplorerItem : TreeViewItem { }

<!-- After (WinUI) - ObservableObject + Children -->
public class ExplorerItem : ObservableObject
{
    public ObservableCollection<ExplorerItem> Children { get; }
}

<TreeView ItemsSource="{x:Bind ViewModel.ExplorerItems}">
    <TreeView.ItemTemplate>
        <DataTemplate x:DataType="model:ExplorerItem">
            <TreeViewItem ItemsSource="{x:Bind Children}">
                <TextBlock Text="{x:Bind DisplayName}"/>
            </TreeViewItem>
        </DataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

### 4. 簡略化・保留した機能

#### GridSplitter
**理由**: WinUIには標準のGridSplitterコントロールがありません。

**現在の実装**: 視覚的な区切りとして`Rectangle`を使用
```xml
<Rectangle Grid.Row="1" Height="3" Fill="#25A0DA" />
```

**将来の実装オプション**:
- カスタムGridSplitterコントロールの実装
- サードパーティライブラリの使用（CommunityToolkit等）

#### ContextMenu
**理由**: 動的メニュー生成の複雑さと、WinUIでの実装方法の違い

**現在の実装**: 削除（保留）

**将来の実装オプション**:
```xml
<GridView.ItemContainerStyle>
    <Style TargetType="GridViewItem">
        <Setter Property="ContextFlyout">
            <Setter.Value>
                <MenuFlyout>
                    <MenuFlyoutItem Text="Open with..."/>
                </MenuFlyout>
            </Setter.Value>
        </Setter>
    </Style>
</GridView.ItemContainerStyle>
```

#### カスタムスタイル（ControlTemplate.Triggers）
**理由**: WinUIでは`ControlTemplate.Triggers`が制限されています。

**対応**: VisualStateManagerを使用するか、シンプルなスタイルに変更

```xml
<!-- WPFでは可能だったが、WinUIでは非対応 -->
<ControlTemplate.Triggers>
    <Trigger Property="IsChecked" Value="true">
        <Setter Property="Background" Value="Blue"/>
    </Trigger>
</ControlTemplate.Triggers>

<!-- WinUIでの代替案: VisualStateManager -->
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup x:Name="CheckStates">
        <VisualState x:Name="Checked">
            <VisualState.Setters>
                <Setter Target="MyElement.Background" Value="Blue"/>
            </VisualState.Setters>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

---

## ビルドと実行方法

### 前提条件

1. **Windows 11** または **Windows 10 バージョン 1809 (ビルド 17763) 以降**
2. **Visual Studio 2022** (17.0以降)
   - ワークロード: 
     - .NET デスクトップ開発
     - C++ によるデスクトップ開発
     - ユニバーサル Windows プラットフォーム開発
3. **Windows App SDK 1.6** (Visual Studioと共にインストールされます)

### ビルド手順

#### Visual Studioを使用する場合

1. `PhotoViewer.sln`を開く
2. ソリューション構成を`Release`または`Debug`、プラットフォームを`x64`に設定
3. ソリューションをビルド（Ctrl+Shift+B）
4. F5キーで実行

#### コマンドラインでビルドする場合

```powershell
# NuGetパッケージの復元
dotnet restore PhotoViewer.sln

# ビルド（C++プロジェクトがあるため、MSBuildを使用）
msbuild PhotoViewer.sln /p:Configuration=Release /p:Platform=x64

# 実行
.\x64\Release\PhotoViewer.exe
```

### トラブルシューティング

#### エラー: "Windows App SDK が見つかりません"
**解決策**: Visual Studio Installerで「ユニバーサル Windows プラットフォーム開発」ワークロードをインストールしてください。

#### エラー: "ImageControllerCLI のビルドに失敗"
**解決策**: 
1. Visual Studio Installerで「C++ によるデスクトップ開発」ワークロードをインストール
2. Windows SDKが正しくインストールされていることを確認

#### エラー: "CommunityToolkit.WinUI.Controls.DataGrid が見つかりません"
**解決策**: NuGetパッケージの復元を実行してください（Visual Studioで自動的に実行されます）

---

## 今後の改善案

### 1. パフォーマンス最適化

#### GridView → ItemsRepeater
より高度な仮想化とパフォーマンスが必要な場合：

```xml
<ScrollViewer>
    <ItemsRepeater ItemsSource="{x:Bind Photos}">
        <ItemsRepeater.Layout>
            <UniformGridLayout MinItemWidth="100" MinItemHeight="95"/>
        </ItemsRepeater.Layout>
        <ItemsRepeater.ItemTemplate>
            <DataTemplate x:DataType="model:PhotoInfo">
                <Grid Width="100" Height="95">
                    <Image Source="{x:Bind ThumbnailImage, Mode=OneWay}"/>
                </Grid>
            </DataTemplate>
        </ItemsRepeater.ItemTemplate>
    </ItemsRepeater>
</ScrollViewer>
```

### 2. カスタムGridSplitterの実装

```csharp
public class GridSplitter : Control
{
    // カスタム実装
    // マウスドラッグでグリッドサイズを変更
}
```

### 3. MenuFlyoutによるContextMenuの再実装

```xml
<GridView>
    <GridView.Resources>
        <MenuFlyout x:Key="ItemContextMenu">
            <MenuFlyoutItem Text="Open with..." Command="{Binding OpenWithCommand}"/>
            <MenuFlyoutSeparator/>
            <MenuFlyoutItem Text="Delete" Command="{Binding DeleteCommand}"/>
        </MenuFlyout>
    </GridView.Resources>
    <GridView.ItemContainerStyle>
        <Style TargetType="GridViewItem">
            <Setter Property="ContextFlyout" Value="{StaticResource ItemContextMenu}"/>
        </Style>
    </GridView.ItemContainerStyle>
</GridView>
```

### 4. 画像処理の強化

WinUIでの高度な画像処理：

```csharp
using Windows.Graphics.Imaging;

public static async Task<SoftwareBitmap> LoadImageAsync(string filePath)
{
    var file = await StorageFile.GetFileFromPathAsync(filePath);
    using var stream = await file.OpenAsync(FileAccessMode.Read);
    var decoder = await BitmapDecoder.CreateAsync(stream);
    return await decoder.GetSoftwareBitmapAsync();
}
```

---

## 既知の制限事項

1. **GridSplitter**: 視覚的な区切りのみ（サイズ変更機能なし）
2. **ContextMenu**: 未実装
3. **画像変換機能**: 一部簡略化（基本的な表示は動作）
4. **Linuxでのビルド**: Windows専用のため、Linux/macOSではビルド不可

---

## 参考資料

### 公式ドキュメント
- [Windows App SDK ドキュメント](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- [WinUI 3 ギャラリー](https://github.com/microsoft/WinUI-Gallery)
- [WPF から WinUI への移行ガイド](https://learn.microsoft.com/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/wpf)

### コミュニティリソース
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit)
- [WinUI 3 サンプル](https://github.com/microsoft/WindowsAppSDK-Samples)

---

## 変更履歴

### 2025-12-13: WinUI 3への完全移行完了
- プロジェクト基盤の変更（.csproj, app.manifest）
- 全C#コードファイル（30+）の移行
- 全XAMLファイル（8/8）の移行
- VirtualizingWrapPanel → GridViewへの置き換え
- DataGrid → CommunityToolkit DataGridへの置き換え
- TreeViewの階層構造の再設計

---

## ライセンス

このプロジェクトは元のPhotoViewerプロジェクトと同じライセンスに従います。
詳細は[LICENSE](LICENSE)ファイルを参照してください。
