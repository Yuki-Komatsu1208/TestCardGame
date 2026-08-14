# PLAN 01: 共通UI基盤

## 目的

ADR-017の最初の実装単位として、OutpostとBattleの両方が依存する表示基準と共通UIの受け口を用意する。Unity EditorのSceneビューおよびPrefab Modeで完成時のレイアウトを確認でき、実行時コードは事前配置されたViewの文言・値・表示状態だけを更新する状態にする。

## 現状

- `OutpostScene` のCanvasは子要素を持たず、`OutpostSceneController` が実行時に背景、パネル、テキスト、ボタン、EventSystemを生成している。
- `OutpostScene` の `CanvasScaler` は `Constant Pixel Size`、参照解像度は `1280 x 720` で、ADR-017の `1920 x 1080` 基準と一致していない。
- `Outpost Camera` はDisplay 1用に存在するが、Culling Maskは0である。Overlay Canvasの前後関係に影響させない前提で維持できる。
- 画面上部の共通ステータス、メッセージ／ログ、歯車ボタン、設定モーダルは事前構築されていない。
- 音量と画面設定を保存・適用するプレイヤー向けサービスは現時点で見当たらない。`PerformanceSettings` はEditor内Play ModeのFPS／VSync用であり、設定画面のバックエンドには使用できない。
- `RunState` にはUIメッセージの待機列がなく、死亡や致命的エラー後の文言をシーン間でOutpostに引き渡す経路も未実装である。
- 利用可能なUI素材は `Assets/UI/` に分類されている。TextMesh ProのUnity標準リソースも同配下にあるが、プロジェクト固有の共通テーマ定義はない。

## 対象

- 16:9、参照解像度 `1920 x 1080`、最低想定 `1280 x 720` のCanvas拡縮基準。
- 羊皮紙色／淡い灰色の面と黒文字を基本とする、ダークファンタジーにコミカルさを加えた共通表示規約。
- 画面上部のステータス領域、メッセージ／ログ領域、右上の歯車ボタン。
- 音量、画面設定、閉じる操作を持つ設定モーダルの事前構築。
- OutpostとBattleから利用する共通View API、表示専用モデル、メッセージ通知経路。
- シーンやPrefabのInspector参照漏れを検出する最小限のEditor検証。

## 対象外

- Outpostの施設本体、カード一覧、Battle HUD、報酬、死亡画面の個別実装。
- フィルター、検索、ソート、ローカライズ、キーコンフィグ。
- 音源自体の追加、複数ミキサーグループのバランス設計、支持画面モード／解像度候補の仕様決定。
- 16:9以外の専用レイアウト。極端なアスペクト比では余白を許容し、別途ADRまたは開発者指示があるまで仕様を拡張しない。

## 設計方針

1. レイアウト、色、フォント、アイコン、トランジションはSceneまたはPrefabにシリアライズする。実行時の `new GameObject`、UI構築目的の `AddComponent`、座標／色指定は行わない。
2. 共通UIは表示と入力通知だけを担当する。ゲーム状態の変更、メッセージの意味判定、設定値の適用／保存はControllerまたは専用サービスに委譲する。
3. OutpostとBattleで表示項目が異なるため、上部ステータスは一つの巨大な共通Prefabに固定しない。共通の視覚規約と小さな部品を使い、各Scene側で必要なスロットを事前配置する。
4. メッセージは本文、種別（通常／警告／致命的エラー等）、表示時刻または順序を持つ表示専用データとし、Viewは種別に対応した事前設定色を選ぶだけにする。開発用詳細とスタックトレースはUnity Consoleに残す。
5. シーン間メッセージはゲーム進行状態として `RunState` に直接混ぜず、セッション寿命の通知経路で「次の画面が1回消費するメッセージ」として扱う案を実装前レビューに出す。実装場所は既存の `RunSession` の責務を肥大化させないことを条件に決める。
6. Inspector参照を正常経路とし、`FindAnyObjectByType` やreflectionを通常の参照解決に使わない。
7. `Assets/Code/`、`Assets/UI/`、`Assets/GameData/` の分類を守る。外観素材は `Assets/UI/`、PrefabとSceneは `Assets/GameData/`、C#型は `Assets/Code/` に置く。

## 実装前の確定項目

以下はADR-017だけでは値が決まっていない。実装者が独断で決めず、開発者に確認する。

- 音量はマスター1項目のみか、BGM／SEを分けるか。
- 画面設定で変更できる項目（フルスクリーンのみか、解像度／VSync等も含むか）。
- 設定の即時適用／決定ボタン方式と、次回起動への保存要否。
- メッセージ履歴の件数、表示秒数、自動スクロール、同じ文言をまとめるかどうか。初期値はInspectorで調整可能にするが、最終値は画面確認後に開発者が決める。
- 共通外観を個々のPrefabに直接保持するか、表示定義用ScriptableObjectを新設するか。新しいSOは調整範囲と更新責務を広げるため、明示承認なしに追加しない。

## 具体的手順

1. `OutpostScene` と `BattleScene`、既存UI PrefabのCanvas、CanvasScaler、ソーティング、EventSystem、入力方式を棚卸しし、共通化する設定とScene固有設定を表にする。
2. 基準Canvasを `Scale With Screen Size` / `1920 x 1080` にし、幅と高さのマッチ値は `1280 x 720`、1920 x 1080、2560 x 1440、3840 x 2160での見え方を比較して決める。
3. パネル、通常ボタン、無効ボタン、札、ゲージ、モーダル遮蔽面、メッセージ種別の見本をPrefab Modeで並べ、黒文字の背景コントラストとピクセル表現の輪郭を確認できるスタイル見本を作る。
4. 上部ステータス／メッセージ領域のViewを作る。固定ラベルはEditor上で見えるダミー文言を持たせ、実行時は表示専用データを `Render` 相当の単一受け口から反映する。
5. メッセージ通知の発行、履歴保持、次Sceneへの1回限りの引き渡し、Viewへの配信を分けた最小のインターフェース案を作り、責務と寿命をコードレビューで確定する。
6. 歯車ボタンと設定モーダルを事前配置する。モーダルは初期非表示とし、背後のレイキャストを遮断し、閉じる操作が必ず機能することを先に検証する。
7. 「実装前の確定項目」の回答後に、設定値の表示モデル、適用サービス、保存方法を実装し、Viewは変更要求だけを通知する。
8. Outpost用とBattle用のルートに共通部品を事前配置し、Inspectorから参照を割り当てる。他のPLANが作る各画面はこの受け口へ表示データを渡す。
9. EditModeでPrefab／Sceneの必須参照、上部領域、歯車ボタン、設定モーダルの存在を検査する。PlayModeで表示更新、モーダルの入力遮断、Scene遷移後のメッセージ消費を検査する。

## 依存関係

- 上位仕様: `Documents/ADRs/GameDesign/ADR-017-serialized-ui-and-screen-interaction-design.md`
- 責務分離: `Documents/ADRs/Architecture/ADR-002-code-owned-state-and-view-only-presentation.md`
- Scene状態引き継ぎ: `RunSession`、`RunLifecycleService`、`RunState`
- 後続PLAN: Outpost、カード一覧、Battle HUD、戦闘後画面は、共通のCanvas基準、上部領域、メッセージ通知、モーダル表示規約に依存する。

## 並列可否

- Canvas基準、表示専用モデル／通知経路、スタイル見本の作成は、受け口とアセット配置を先に合意すれば並列化できる。
- `OutpostScene`、`BattleScene`、共通Prefabを複数エージェントが同時編集しない。Scene YAMLとPrefabの競合を避けるため、担当ファイルで分ける。
- Outpost実装はCanvas基準と共通View APIが確定した後に統合する。ビジュアル素材の作成は、サイズ、9-slice境界、色パレットの受け渡しを固定すれば別エージェントで並列実施できる。

## 受入条件

- OutpostとBattleの対象Canvasが `1920 x 1080` 参照の `Scale With Screen Size` で、16:9の各検証解像度で重要操作が画面外に出ない。
- Editor上で上部ステータス／メッセージ領域、歯車ボタン、設定モーダルの位置と概形を確認できる。
- 通常文字は明るいパネル上の黒を基本とし、暗い背景へ黒文字を直接置いていない。警告／致命的エラーは黄／琥珀色で識別できる。
- Viewの更新でGameObjectやUI Componentを生成せず、事前配置された参照だけを更新する。
- 設定モーダル表示中は背後操作が発火せず、閉じた後に元の画面操作へ戻れる。
- シーンを跨ぐプレイヤー向けメッセージが次画面で1回表示され、再読み込みで意図せず再表示されない。
- 必須Inspector参照の欠落がEditor検証または明確な日本語エラーで検出される。
- 設定画面の実適用は「実装前の確定項目」が解消してから受入対象とする。未確定のままは外観と開閉のみで完了扱いにしない。

## 検証

- EditMode: Scene／Prefabの必須参照数、初期active状態、CanvasScaler設定、モーダルの遮蔽面、EventSystemの重複を自動検査する。
- PlayMode: ダミーのステータスモデルを渡して文言／ゲージ／アイコンが更新されること、設定モーダル開閉、入力遮断、メッセージの1回消費を確認する。
- 手動: Gameビューを `1280 x 720`、1920 x 1080、2560 x 1440、3840 x 2160で表示し、文字のつぶれ、ボタンの押下可否、上部領域と歯車の衝突、暗部と黒文字の衝突を確認する。
- 回帰: 既存のOutpost起動、Battle起動、Scene遷移が維持され、Unity ConsoleにNullReferenceExceptionや重複EventSystem警告が出ないことを確認する。

## 主な対象ファイル

- `Documents/ADRs/GameDesign/ADR-017-serialized-ui-and-screen-interaction-design.md`（参照、変更しない）
- `Assets/GameData/Scenes/OutpostScene.unity`
- `Assets/GameData/Scenes/BattleScene.unity`
- `Assets/GameData/Prefabs/`（共通パネル、メッセージ領域、設定モーダル等の新規Prefab）
- `Assets/UI/Art/`、`Assets/UI/Fonts/`（共通外観素材）
- `Assets/Code/Runtime/UIs/`（共通View、表示専用モデル、設定モーダルController）
- `Assets/Code/Runtime/Run/RunSession.cs`（通知経路との接続が必要な場合のみ。通知本体を安易に抱え込まない）
- `Assets/Code/Editor/`（Scene／Prefab参照検証）
- 既存テスト配下または、プロジェクトの現行分類に合わせたEditMode／PlayModeテストファイル
