# AGENTS.md — `AXS.Core` (`com.axs.core`) AI開発ガイドライン

このリポジトリは **Unity 6 (6000.0+ / 6000.5+)** を対象としたスタンドアロンの **Unity Package Manager (UPM)** 汎用フレームワーク（`com.axs.core`）です。
このワークスペースで作業するすべてのAIエージェントは、以下のアーキテクチャ原則、コーディング規約、およびUPMパッケージ制約を厳守してください。

---

## 1. アーキテクチャと厳格なレイヤー境界

本パッケージは4層アーキテクチャのうち **Layer 0 〜 Layer 2** を実装しています。
依存関係は必ず単方向（`Layer 2 -> Layer 1 -> Layer 0`）を保ち、逆方向の参照や循環参照は一切禁止します。

| レイヤー | ディレクトリ | アセンブリ (`.asmdef`) | ルート名前空間 | 許可される依存関係・制約ルール |
| :--- | :--- | :--- | :--- | :--- |
| **Layer 0** | `Runtime/Utilities/` | `AXS.Core.Utilities` | `AXS.Core.Utilities` | **外部パッケージ依存ゼロ。** 標準C#および最小限の `UnityEngine` コアAPIのみ使用可。`UniTask`、`Addressables`、`UnityEngine.UI`、`InputSystem` 等への参照は厳禁。 |
| **Layer 1** | `Runtime/Framework/` | `AXS.Core` | `AXS.Core.<Module>` | `AXS.Core.Utilities`, `UniTask`, `UnityEngine.UI`, `Unity.Addressables` を参照。ジャンル不問のマネージャーや基底クラスを配置する。**具体的な見た目のPrefabや画像（それらは利用先プロジェクトのLayer 3に属する）、特定OS固有のコードは絶対に含めないこと。** |
| **Layer 2** | `Runtime/Platform/Mobile/` | `AXS.Platform.Mobile` | `AXS.Platform.Mobile` | `AXS.Core`, `AXS.Core.Utilities`, `UniTask`, `UnityEngine.UI`, `Unity.InputSystem` を参照。モバイル端末やOS固有の挙動（SafeArea、物理戻るキー、Haptics振動、OS権限など）を吸収し、Layer 1へ橋渡しする。 |

> **将来 Editor 拡張スクリプトを追加する場合:**  
> Editor専用スクリプトは `Editor/` フォルダを新設し、専用の `AXS.Core.Editor.asmdef`（`"includePlatforms": ["Editor"]`）を配置すること。`Runtime/` 配下に `#if UNITY_EDITOR` 無しでエディタコードを混在させないこと。

---

## 2. コーディング規約 & Unity 6000.5+ 互換性ルール

1. **Unity 6000.5+ 最新API準拠（最重要）**:
   - **`Object.GetInstanceID()` は使用禁止**（Unity 6000.5+ で廃止・エラー化）。オブジェクト識別子やDictionaryのキーには必ず **`Object.GetEntityId()`** および **`EntityId`** 構造体を使用すること。
   - **`Object.FindObjectOfType<T>()` および `Object.FindFirstObjectByType<T>()` は使用禁止**。必ず **`Object.FindAnyObjectByType<T>()`** を使用すること。
2. **非同期処理 (`UniTask`)**:
   - 標準の `Task` やコルーチン（`IEnumerator`）ではなく、`UniTask`（`Cysharp.Threading.Tasks`）を使用すること。
   - 公開する非同期メソッドは必ず末尾に `Async` を付け、最終引数に `CancellationToken cancellationToken = default` を受け取ること。
   - 内部で生成した `CancellationTokenSource` は中断時や `OnDestroy` 時に確実に `Cancel()` および `Dispose()` すること。
3. **TimeScale 非依存の徹底**:
   - UIアニメーション（`UIElement`）、BGMクロスフェード（`AudioManager`）、連打防止クールダウン（`DebounceUtility`）では、ゲームポーズ中（`Time.timeScale = 0`）でも停止しないよう必ず `Time.unscaledDeltaTime` または `Time.unscaledTime` を使用すること。
4. **Input System 両対応**:
   - Layer 2 でハードウェア入力を読み取る際は、`#if ENABLE_INPUT_SYSTEM ... #else ... #endif` を用いて新 Input System と旧 Input Manager の両方でコンパイル・動作するように実装すること。
5. **ゼロアロケーション & メモリ安全性**:
   - `Update()` 内や高頻度で呼ばれるパスでの LINQ 使用や不要なヒープアロケーション（ボクシング等）を避けること。
   - ロードした `Addressables` ハンドルは必ず追跡・解放すること（`AddressableScopeLoader` 参照）。

---

## 3. UPMパッケージ & `.meta` ファイル管理ルール

1. **`package.json` はリポジトリ直下に配置**:
   - `https://github.com/kkzkut/AXS.Core.git#vX.Y.Z` のURLで `?path=` 無しに直接インポートできるよう、`package.json` は常にリポジトリのルート直下に置くこと。
2. **`Runtime/`（および `Editor/`）配下の `.meta` ファイルは必須**:
   - Git経由でUPMパッケージを配布する際、Unityはコミットされた `.meta` ファイルのGUIDを使って全プロジェクト間のスクリプト参照を維持する。
   - `Runtime/` 内に新しい `.cs` ファイル、`.asmdef`、またはフォルダを作成した場合は、必ず有効な `.meta` ファイルを生成してコミットすること（手順は `add-framework-feature` スキルを参照）。
   - `Runtime/` 内のファイルやフォルダを削除・移動した場合は、対応する `.meta` ファイルも必ず同時に削除・移動すること。
3. **ドット始まりの隠しフォルダ（`.agents/`, `.git/`）**:
   - UnityのAsset Importerは `.` で始まるフォルダ（`.agents/` 等）を自動的に無視する。`.agents/` やその中身に対して `.meta` ファイルを作成しないこと。
4. **ドキュメントの同期**:
   - 公開クラスや機能を追加・変更・リネームした際は、必ず `README.md` のディレクトリ構成図および機能解説もあわせて更新すること。

---

## 4. 検証用サンドボックスプロジェクト

- **サンドボックスのパス**: `D:\work_d\UnityProject\AXS-Core-Sandbox`
- `AXS.Core` は純粋なUPMパッケージリポジトリであり単体では `ProjectSettings/` や `Library/` を持たないため、コンパイル検証（`dotnet build` または Unity Editor での確認）および PlayMode でのデモ動作検証は上記のサンドボックスプロジェクトを通じて行うこと。
