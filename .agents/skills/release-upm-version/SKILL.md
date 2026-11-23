---
name: release-upm-version
description: >-
  com.axs.core の .meta ファイル監査、package.json のバージョン更新、ドキュメント同期、および Git リリースタグ作成を行う手順書。
  ユーザーから AXS.Core のリリース、バージョンアップ、タグ付け、公開を求められた際に使用する。
---

# `AXS.Core` (`com.axs.core`) 新バージョンリリース手順

`com.axs.core` UPMパッケージの新バージョンをリリース・Pushする際は、以下のチェックリストに従うこと。

## ステップ1: `Runtime/` 配下の `.meta` ファイル監査

`Runtime/`（および `Editor/` が存在する場合はその配下）のすべてのファイルとフォルダに対応する `.meta` ファイルが存在し、孤立した `.meta` ファイルがないことを確認する。

リポジトリルート（`d:\work_d\AXS.Core\AXS.Core`）で以下の PowerShell コマンドを実行してチェックする：

```powershell
Get-ChildItem -Path Runtime -Recurse | Where-Object { $_.Extension -ne '.meta' -and -not (Test-Path "$($_.FullName).meta") } | Select-Object FullName
```

- もし1件でも出力された場合は、先に `.meta` ファイルを生成（またはサンドボックスUnityプロジェクトを開いて自動生成）してから次へ進むこと。

## ステップ2: セマンティックバージョンの決定と `package.json` の更新

[package.json](../../../package.json) の `version` をセマンティックバージョニング（`MAJOR.MINOR.PATCH`）に従って更新する：
- **PATCH** (`1.0.x`): 公開APIの変更を伴わないバグ修正や内部改善。
- **MINOR** (`1.x.0`): 後方互換性を保った新機能、ユーティリティ、コンポーネントの追加。
- **MAJOR** (`x.0.0`): 既存の公開APIの破壊的変更、または対応Unityバージョンの引き上げ。

必要に応じて [README.md](../../../README.md) 内の導入URL例のバージョン表記も更新する。

## ステップ3: Gitコミット・タグ付け・Push

1. 変更差分とステータスを確認する：
   ```powershell
   git status
   ```
2. 変更をステージングしてコミットする：
   ```powershell
   git add .
   git commit -m "release: vX.Y.Z - <変更内容の要約>"
   ```
3. Gitタグを作成し、ブランチとタグの両方をリモート（`origin`）へPushする：
   ```powershell
   git tag vX.Y.Z
   git push origin main
   git push origin vX.Y.Z
   ```
