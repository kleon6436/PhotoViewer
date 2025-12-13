# PhotoViewer ビルド手順書

## Windows環境でのビルド

### 必要な環境

1. **オペレーティングシステム**
   - Windows 11（推奨）
   - Windows 10 バージョン 1809（ビルド 17763）以降

2. **開発ツール**
   - Visual Studio 2022（バージョン 17.0以降）
   - 必須ワークロード:
     - ✅ .NET デスクトップ開発
     - ✅ C++ によるデスクトップ開発
     - ✅ ユニバーサル Windows プラットフォーム開発

3. **SDK**
   - Windows App SDK 1.6（Visual Studioと共にインストール）
   - Windows SDK 10.0.19041.0以降

### ビルド手順（Visual Studio使用）

#### ステップ1: ソリューションを開く

```
PhotoViewer.sln をVisual Studio 2022で開きます
```

#### ステップ2: ビルド構成を設定

1. ソリューション構成: `Release` または `Debug`
2. ソリューションプラットフォーム: `x64`

![ビルド構成](https://user-images.githubusercontent.com/placeholder/build-config.png)

#### ステップ3: NuGetパッケージの復元

Visual Studioが自動的にNuGetパッケージを復元します。
手動で実行する場合:

```
ソリューションを右クリック → NuGetパッケージの復元
```

#### ステップ4: ソリューションのビルド

```
メニュー: ビルド → ソリューションのビルド
ショートカット: Ctrl+Shift+B
```

#### ステップ5: 実行

```
F5キーまたはメニューから「デバッグ → デバッグの開始」
```

### ビルド手順（コマンドライン使用）

#### 前提条件

Developer Command Prompt for Visual Studio 2022を起動してください。

#### コマンド

```powershell
# プロジェクトディレクトリに移動
cd C:\path\to\PhotoViewer

# NuGetパッケージの復元
dotnet restore PhotoViewer.sln

# ソリューション全体をビルド（C++プロジェクトを含む）
msbuild PhotoViewer.sln /p:Configuration=Release /p:Platform=x64

# または、特定のプロジェクトのみビルド
msbuild PhotoViewer\PhotoViewer.csproj /p:Configuration=Release /p:Platform=x64

# 実行
.\x64\Release\PhotoViewer.exe
```

### ビルド成果物

ビルドが成功すると、以下の場所に実行ファイルが生成されます:

```
x64\Release\PhotoViewer.exe
x64\Release\ImageController.dll
x64\Release\ImageControllerCLI.dll
```

---

## トラブルシューティング

### エラー: "プロジェクト 'ImageControllerCLI' を読み込めませんでした"

**原因**: C++ワークロードがインストールされていません

**解決策**:
1. Visual Studio Installerを起動
2. 「変更」をクリック
3. 「C++ によるデスクトップ開発」にチェック
4. インストールを実行

### エラー: "NETSDK1100: To build a project targeting Windows on this operating system"

**原因**: Linux/macOS上でWindowsプロジェクトをビルドしようとしています

**解決策**:
このプロジェクトはWindows専用です。Windows環境でビルドしてください。

### エラー: "パッケージ 'Microsoft.WindowsAppSDK' が見つかりません"

**原因**: NuGetパッケージが復元されていません

**解決策**:
```powershell
dotnet restore PhotoViewer.sln
```

または、Visual Studioで:
```
ツール → NuGet パッケージ マネージャー → ソリューションのNuGetパッケージの管理
```

### エラー: "CommunityToolkit.WinUI.Controls.DataGrid が見つかりません"

**原因**: CommunityToolkitパッケージが復元されていません

**解決策**:
```powershell
# NuGetパッケージキャッシュのクリア
dotnet nuget locals all --clear

# パッケージの再復元
dotnet restore PhotoViewer.sln
```

### 警告: "NU1503: プロジェクトをスキップしています"

**原因**: C++プロジェクト（ImageController.vcxproj, ImageControllerCLI.vcxproj）はdotnet restoreではサポートされていません

**解決策**:
これは警告であり、エラーではありません。Visual StudioまたはMSBuildを使用してビルドすれば問題ありません。

### ビルドエラー: "リソースファイルが見つかりません"

**原因**: 画像リソースファイルが正しい場所にありません

**解決策**:
以下のファイルが存在することを確認してください:
```
PhotoViewer\Images\PhotoExifViewerIcon.ico
PhotoViewer\Images\bluetooth.png
PhotoViewer\Images\edit.png
PhotoViewer\Images\folder.png
PhotoViewer\Images\reload.png
PhotoViewer\Images\settings.png
PhotoViewer\Images\SplashScreen.png
PhotoViewer\Images\PlaceholderImage_100x100.png
```

---

## クリーンビルド

問題が解決しない場合は、クリーンビルドを試してください:

### Visual Studioでのクリーンビルド

```
1. メニュー: ビルド → ソリューションのクリーン
2. メニュー: ビルド → ソリューションのリビルド
```

### コマンドラインでのクリーンビルド

```powershell
# クリーン
msbuild PhotoViewer.sln /t:Clean /p:Configuration=Release /p:Platform=x64

# リビルド
msbuild PhotoViewer.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64
```

---

## デバッグビルド vs リリースビルド

### デバッグビルド（Debug）

```
- シンボル情報が含まれる
- 最適化なし
- デバッグしやすい
- ファイルサイズが大きい
```

### リリースビルド（Release）

```
- 最適化あり
- シンボル情報なし（設定による）
- パフォーマンスが良い
- ファイルサイズが小さい
```

---

## 配布用パッケージの作成

### 方法1: フォルダ発行

```powershell
# .NET 8.0向けのセルフコンテナ発行
dotnet publish PhotoViewer\PhotoViewer.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false
```

発行先: `PhotoViewer\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`

### 方法2: MSIX パッケージ（推奨）

1. Visual Studioで「パッケージ作成ウィザード」を使用
2. プロジェクトを右クリック → 発行 → MSIXパッケージの作成

MSIXパッケージの利点:
- ✅ Windows Storeで配布可能
- ✅ 自動更新サポート
- ✅ クリーンなインストール/アンインストール
- ✅ サンドボックス実行

---

## 開発時のヒント

### Hot Reload（ホットリロード）

WinUI 3アプリケーションは.NET Hot Reloadをサポートしています:

```
1. デバッグモードでアプリを起動（F5）
2. XAMLまたはC#コードを編集
3. Ctrl+Sで保存
4. アプリが自動的に更新される
```

### XAMLデザイナー

Visual Studio 2022のXAMLデザイナーでWinUI 3のプレビューが可能です:

```
1. .xamlファイルを開く
2. デザインビューとコードビューの分割表示
3. リアルタイムプレビュー
```

### パフォーマンスプロファイリング

```
メニュー: デバッグ → パフォーマンス プロファイラー
- CPU使用率
- メモリ使用量
- .NETオブジェクト割り当て
```

---

## CI/CDでのビルド

### GitHub Actions サンプル

```yaml
name: Build PhotoViewer

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Restore NuGet packages
      run: dotnet restore PhotoViewer.sln
    
    - name: Build
      run: msbuild PhotoViewer.sln /p:Configuration=Release /p:Platform=x64
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: PhotoViewer
        path: x64/Release/
```

---

## サポート

ビルドに関する問題が発生した場合:

1. このドキュメントのトラブルシューティングセクションを確認
2. README_WINUI_MIGRATION.mdで移行の詳細を確認
3. GitHubのIssuesで報告

---

## 関連ドキュメント

- [README_WINUI_MIGRATION.md](README_WINUI_MIGRATION.md) - WinUI 3への移行の詳細
- [README.md](README.md) - プロジェクト概要
