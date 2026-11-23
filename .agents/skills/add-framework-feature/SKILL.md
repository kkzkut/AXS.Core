---
name: add-framework-feature
description: >-
  AXS.Core (com.axs.core) UPMパッケージに新しいクラス、コンポーネント、または機能モジュールを追加する際の手順書。
  ユーティリティ、フレームワーク機能、モバイルプラットフォームアダプターの実装を求められた際に使用する。
---

# `AXS.Core` (`com.axs.core`) への新機能追加手順

`AXS.Core` に新しいスクリプト、モジュール、機能を追加する際は、以下のステップに従って作業を進めること。

## ステップ1: 配置レイヤーと名前空間の選定

`AGENTS.md` の3層アーキテクチャ原則に従い、追加する機能の配置先を決定する。

1. **Layer 0 — `Runtime/Utilities/<Category>/`**
   - **アセンブリ**: `AXS.Core.Utilities`
   - **名前空間**: `AXS.Core.Utilities`
   - **用途**: **外部パッケージ依存ゼロ**で動作する純粋な計算処理、C# / 基本Unity型の拡張メソッド、データ構造、疎結合メッセージングなど。
2. **Layer 1 — `Runtime/Framework/<Module>/`**
   - **アセンブリ**: `AXS.Core`
   - **名前空間**: `AXS.Core.<Module>`（例: `AXS.Core.Save`, `AXS.Core.UI`, `AXS.Core.Audio`）
   - **用途**: ゲームジャンルを問わないランタイムシステム、マネージャー、オブジェクトプール、抽象ローダーなど。`UniTask`, `Addressables`, `UnityEngine.UI`, `AXS.Core.Utilities` を利用可能。具体的な見た目のPrefabやOS固有処理は含めないこと。
3. **Layer 2 — `Runtime/Platform/Mobile/`（または新規プラットフォームフォルダ）**
   - **アセンブリ**: `AXS.Platform.Mobile`
   - **名前空間**: `AXS.Platform.Mobile`
   - **用途**: モバイル端末・OS固有機能のアダプター（SafeArea、戻るキー、Haptics振動、ストアレビュー、ATT、通知など）。

## ステップ2: コードの実装

`AGENTS.md` の規約に準拠して実装する：
- `GetInstanceID()` は使わず、`Object.GetEntityId()`（`EntityId`）を使用する。
- `FindObjectOfType` / `FindFirstObjectByType` は使わず、`Object.FindAnyObjectByType<T>()` を使用する。
- 非同期メソッドは `UniTask` を使い、末尾に `CancellationToken cancellationToken = default` を受け取る。
- 公開クラスやメソッドには日本語のXMLドキュメントコメント（`/// <summary>`）を記述する。

## ステップ3: 新規フォルダ・スクリプトの `.meta` ファイル生成

`Runtime/` 配下のすべてのフォルダおよび `.cs` ファイルには、プロジェクト間でGUIDを固定するための `.meta` ファイルが必須である。

- **新規フォルダ `Runtime/.../<FolderName>` を作成した場合**（Unity Editor未起動時）：
  `Runtime/.../<FolderName>.meta` を以下の形式で作成する：
  ```yaml
  fileFormatVersion: 2
  guid: <ハイフン無し32文字の小文字16進数GUID>
  folderAsset: yes
  DefaultImporter:
    externalObjects: {}
    userData: 
    assetBundleName: 
    assetBundleVariant: 
  ```
- **新規C#スクリプト `<ScriptName>.cs` を作成した場合**（Unity Editor未起動時）：
  `<ScriptName>.cs.meta` を以下の形式で作成する：
  ```yaml
  fileFormatVersion: 2
  guid: <ハイフン無し32文字の小文字16進数GUID>
  MonoImporter:
    externalObjects: {}
    serializedVersion: 2
    defaultReferences: []
    executionOrder: 0
    icon: {instanceID: 0}
    userData: 
    assetBundleName: 
    assetBundleVariant: 
  ```
  *(※新しい32文字の16進数GUIDは PowerShell の `[guid]::NewGuid().ToString("N")` で生成すること)*

## ステップ4: コンパイル検証（サンドボックス環境が利用可能な場合）

検証用サンドボックスプロジェクト（`D:\work_d\UnityProject\AXS-Core-Sandbox`）がこのリポジトリを参照しており、`.csproj` が生成されている場合は：
1. `D:\work_d\UnityProject\AXS-Core-Sandbox` 内の対象 `.csproj` に対して `dotnet build` を実行し、0エラー・0警告でビルドが通ることを確認する。

## ステップ5: `README.md` の更新

[README.md](../../../README.md) を更新する：
1. **「2. ディレクトリ構成とファイル一覧」** のツリーに新規ファイルを追加する。
2. **「4. 各機能の詳細と実装サンプル」** の該当レイヤー項目に、機能の目的と簡潔な使用コード例を追記する。
