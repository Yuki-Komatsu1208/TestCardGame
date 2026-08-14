# PLAN 08: 旧実行時UI生成の撤去とEditor検証

## 目的

PLAN 01〜07で置き換えた旧UI生成経路を安全に削除し、Scene・Prefabの参照漏れと固定スロット数をEditor上で検出できる状態にする。

## 現状

* `OutpostSceneController` がOutpost画面、ショップ、商品、テキストを実行時生成している。
* `BattleUI` がHUDの不足時フォールバック、報酬パネル、RUN終了パネル、選択ボタンを実行時生成している。
* `HandView` がカードPrefabを必要数生成し、既存Viewを破棄している。
* `CardListSelectionOverlay` がカードViewを生成し、reflectionやシーン探索へ依存する箇所を持つ。
* `CellBuilder` が盤面モデル生成と盤面View生成の両方を担当している。
* 動的生成が欠けたInspector参照を隠すフォールバックとして働いており、単純削除すると黒画面や操作不能を起こす可能性がある。

## 対象

* 置換済みの画面構築コードとフォールバック生成の削除
* `FindAnyObjectByType`、reflection、暗黙のPrefab探索の通常経路からの撤去
* 固定スロット数と必須参照を検証するEditorツール
* Scene、Prefab、Controllerの参照整合性確認
* UIリファクタ全体の回帰テスト

## 対象外

* UIと無関係なゲームロジックのリファクタ
* 未実装の工房、クエスト、アイテム効果
* アセットフォルダ全体の再編成
* 設計済みでない新しい画面や演出の追加

## 削除方針

* PLAN単位で新経路の受入条件を満たすまで、対応する旧経路を削除しない。
* 新旧経路を長期間併存させず、PLAN 08で利用箇所を一括検索して整理する。
* 欠落参照時にUIを生成して継続するフォールバックは廃止し、日本語の検証エラーとしてPlay開始前に検出する。
* 実行時のUI構造生成と、ゲームモデル生成を区別する。許可する動的処理が残る場合は、理由と対象を検証コードの除外規則へ明記する。
* `Destroy` と `Instantiate` の単純な文字列全削除は行わず、UI表示構造を作り直す用途だけを対象にする。

## Editor検証項目

### 共通

* 基準Canvas、CanvasScaler、EventSystem、Cameraの存在
* 必須View、Presenter、ボタン、テキスト、Image、ScrollRectの参照
* 参照先がScene内または正しいPrefab内に属すること

### 固定スロット

* 共通カード一覧が50枠であること
* 戦闘手札が50枠であること
* ショップカード商品が10枠であること
* キーストーン商品が3枠であること
* 通常報酬が3枠であること
* スロット配列にnullと重複参照がないこと

### 画面固有

* Outpostの宿屋、ショップ、工房、クエスト、出発パネルが存在すること
* 工房とクエストの準備中表示が存在すること
* BattleのHUD、上部ログ、手札、盤面、報酬、ルート選択、死亡パネルが存在すること
* カード一覧の6列Grid、縦ScrollRect、詳細パネル、選択数、OK／NO操作が存在すること
* 敵意図アイコンと対象範囲表示先が設定されていること

## 実装手順

1. PLAN 01〜07の受入結果と、置換対象になった旧メソッド一覧を収集する。
2. `new GameObject`、`AddComponent`、`Instantiate`、`Destroy`、`FindAnyObjectByType`、reflectionをUIコードから検索し、用途を分類する。
3. Outpostの旧 `CreatePanel`、`CreateText`、画面再構築経路を削除する。
4. Battleの不足時フォールバック生成、報酬生成、終了パネル生成、可変選択ボタン生成を削除する。
5. HandViewとカード一覧の生成・破棄を、固定スロットへのBind／Unbindへ置き換え済みであることを確認し、旧経路を削除する。
6. PLAN 05の決定に従って、CellBuilder内のView生成責務を撤去または許可済み経路へ限定する。
7. シーン探索とreflectionによる参照取得を、Inspector参照または明示的な依存注入へ置き換える。
8. UI構造とスロット数を検査するEditor Validatorを追加する。
9. Unityのメニューまたは保存・ビルド前検証からValidatorを実行可能にする。
10. EditMode、PlayMode、手動解像度確認を実施する。
11. 不要になった旧Prefab、未参照UI Asset、旧Scene内オブジェクトを参照検索後に整理する。削除は対象を明示して行う。

## 依存関係

* PLAN 01〜07がそれぞれ受入条件を満たしていること
* SceneとPrefabの最終的な所有担当が変更を完了していること
* 盤面セルViewの事前配置方針または許可済み動的処理がPLAN 05で確定していること

## 並列可否

* Validatorの純粋な検査ロジックは、各画面の最終調整と並列実装できる。
* 旧コード削除は共有ControllerとSceneへ広く触れるため、他エージェントの実装完了後に直列で行う。
* 未参照Asset削除は破壊的なので、参照監査と開発者確認後に別作業として行う。

## 受入条件

* OutpostとBattleの主要画面がEditor上で確認できる。
* UI構造生成を目的とした実行時 `new GameObject` と `AddComponent` がない。
* カード、商品、報酬の表示が固定スロットのBind／Unbindで動く。
* 必須参照やスロット数が不正な場合、Play前のEditor検証で日本語エラーが出る。
* 必須参照欠落を動的生成で黙って補わない。
* Outpost、カード一覧、Battle、報酬、OverHunt、死亡、致命的復旧が通して動く。
* 既存カード効果、ショップ抽選、報酬、RUN進行に回帰がない。

## 検証

### 静的検査

* UIコード内の生成・探索API利用箇所を一覧化し、許可理由のない箇所が0件であること
* Unity SceneとPrefabのMissing Script、Missing Reference確認
* GUIDと`.meta`の重複確認

### EditModeテスト

* 各固定スロット数の正常・不足・重複ケース
* 必須参照欠落時の日本語エラー
* 表示モデルをBind後、余剰スロットが非表示になること
* Unbind後に前データが残らないこと

### PlayModeテスト

* 新規RUN開始からOutpost、通常戦闘、報酬、次戦闘、ボス帰還
* OverHunt進入、保留ゴールド表示、帰還、死亡
* カード0枚、20枚、21枚、50枚の一覧と手札
* ショップ商品0件、10件、購入済み、ゴールド不足、キーストーン購入後
* 敵複数、盤面5 x 5、6 x 6、範囲表示、意図再計算
* 致命的エラーから新規RUNのOutpostへ復旧

### 手動表示確認

* 1280 x 720
* 1920 x 1080
* 2560 x 1440
* 通常DPIと高DPI
* カード50枚時の可読性とスクロール操作

## 主な対象ファイル

* `Assets/Code/Runtime/UIs/OutpostSceneController.cs`
* `Assets/Code/Runtime/Controller/BattleUI.cs`
* `Assets/Code/Runtime/Cards/Views/HandView.cs`
* `Assets/Code/Runtime/UIs/CardListSelectionOverlay.cs`
* `Assets/Code/Runtime/BoardManage/CellBuilder.cs`
* `Assets/Code/Runtime/Cards/Views/CardView.cs`
* `Assets/Code/Runtime/UIs/MoveCardDragHandler.cs`
* `Assets/GameData/Scenes/OutpostScene.unity`
* `Assets/GameData/Scenes/BattleScene.unity`
* `Assets/GameData/Prefabs/`
* 新規Editor Validatorと対応テスト
