# KcharyPhotoViewer
## 免責事項
このソフトは無保証・無責任です。以下の条件に同意していただける場合にのみ、このソフトをご利用いただくことができます。

- 作者は、このソフトによって発生した損害に関し、一切の責任を負わない。

- 作者は、このソフトのサポート ( 不具合修正・バージョンアップなど ) に関する一切の義務を負わない。

## アプリ概要
Windows用のKchary作成PhotoViewerです。技術力が上がってきたので、前回のものをリメイクしています。

Windows 10 1903以降の対応で、RawImageExtension、および、NikonのNEF Codecのインストールが必須である。https://downloadcenter.nikonimglib.com/ja/products/170/NEF_Codec.html

Exif情報と写真閲覧、Exif情報の削除などが行えます。

以前のものから機能を考え直して、作り直しました。

動画再生機能などは組み込んでいません。

![app screenshot](./Images/AppScreen.png)

## アプリの動作
Exif情報や写真の閲覧ができます。

Exif情報の削除し、ブログ向け、SNS向けにファイルサイズを変更して保存できます。
※ 私がよく使う3つのファイルサイズを選択できます。

設定画面で設定した他のアプリを起動することも可能です。

## ビルドについて
本アプリは、vcpkgにて、OpenCV、Librawの32bit,、64bitのライブラリがインストールされている環境でのみビルド可能です。

vcpkgは、以下のURLを参考にインストールしてください。

https://docs.microsoft.com/ja-jp/cpp/build/vcpkg?view=msvc-160

次に、以下のコマンドを打ち、OpenCVとLibrawのライブラリをビルドしてください。

'vcpkg install opencv:x64-windows opencv:x86-windows libraw:x64-windows libraw:x86-windows'

その後、以下のコマンドを打ち、Visual studioのプロジェクトにvcpkgを適用してください。

'vcpkg integrate install'


## 使用しているテクノロジ
- Prism

- Libraw

- OpenCV

## 機能
- 写真閲覧

- Exif情報の閲覧

- 連携アプリの起動（設定画面で設定が必要）

## クレジット
- kchary @kleon6436 (Author, Developer)

This software is released under the MIT License, see LICENSE.txt.
