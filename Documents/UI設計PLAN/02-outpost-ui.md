# PLAN 02: Outpost UI

## 目的

`OutpostScene` のUIをScene／Prefabのシリアライズ済みデータとして事前構築し、Unity Editor上で施設選択、宿屋、ショップ、準備中画面を確認できるようにする。既存のRUN、宿屋、ショップ、出発ルールは維持し、Viewは表示と入力通知、バックエンドは利用可否と状態変更を担当する。

## 現状

- `OutpostScene` にあるのは空のCanvas、`OutpostSceneController`、`RunBootstrapController`、描画対象0の `Outpost Camera` だけである。EventSystemもSceneには保存されていない。
- `OutpostSceneController.Awake()` がCanvasとEventSystemを探索し、必要なUIをすべて実行時生成する。画面切替時はCanvas配下を `Destroy` して再構築する。
- 施設選択に宿屋、ショップ、工房、クエストカウンター、出発がある。工房とクエストはログ出力だけで、画面内に「準備中」は表示されない。
- 宿屋は `RunSession.TryUseInn`、カード購入は `TryBuyShopCard`、キーストーン購入は `TryBuyKeystone`、出発は `TryStartNextExpedition` を呼ぶ。既存サービスは主に `bool` を返すため、UIが失敗理由を安全に表示するための結果データが不足している。
- `ShopService.CardOfferCount` は10、重み付き・重複なし抽選、セール、滞在中の在庫固定、購入済み状態は実装済みである。
- `ShopCatalogSO` はカードプールとキーストーン定義を持つ。キーストーンはRUN中1個だけ購入できる。
- `RunState` からHP、スタン耐性、ゴールド、キーストーン、所持アイテム、遠征インデックス／ステージインデックス、OverHunt状態を取得できる。表示文言への変換層はまだない。
- ADR-015の「キーストーン購入後は購入済みだけを表示」は、後発のADR-017で「購入済みは売り切れ、残りは購入不能の札を表示して選択不能」に具体化されているため、UI表示はADR-017を優先する。

## 対象

- Outpost背景、上部ステータス／メッセージ領域、施設選択親画面。
- 宿屋パネル、ショップパネル、工房／クエスト共通の準備中パネル、各施設の戻る操作、出発操作。
- 事前配置したカード商品10枠とキーストーン商品3枠。
- 通常、セール、購入済み／売り切れ、購入不能、ゴールド不足の表示。
- 既存バックエンドとの接続、Outpost表示専用モデル、ユーザー操作の結果通知。
- 旧実行時UI生成のOutpost範囲の撤去。

## 対象外

- 工房のMOD購入／抽出、クエストの候補抽選／受託／進捗管理。今回は入口と「準備中」表示のみとする。
- ショップの汎用アイテム販売、商品数の追加、価格・抽選・セール確率のバランス変更。
- 購入前確認と回復前確認。ADR-017に従い追加しない。
- カード一覧／詳細の共通オーバーレイ。Outpostから開く導線は後続のカード一覧PLANと統合する。
- 死亡または致命的エラーを検出して新規RUNを始める処理本体。それらが発行した上部メッセージの表示受け口だけを用意する。

## 設計方針

1. SceneにOutpostの画面ルートを配置し、施設選択、宿屋、ショップ、準備中、設定の各パネルを子として保持する。実行時はactive切替とデータ反映だけを行う。
2. `OutpostSceneController` は「何を表示するか」と「ユーザー操作をどのサービスに渡すか」のオーケストレーションに絞る。パネル、商品枠、ステータスは専用Viewに分け、各Viewから商品インデック／キーストーンID／施設種別だけを通知する。
3. `RunState` や `ShopCatalogSO` をViewに直接解釈させない。ControllerまたはPresenterがOutpostステータス、宿屋利用可否、カード商品、キーストーン商品の表示専用データを組み立てる。
4. サービスは購入可否と失敗理由の正を持つ。現行の `bool` だけでゴールド不足、購入済み、不正インデック、定義欠落を判別できない場合は、既存の公開APIを壊さない最小の結果型または照会APIをバックエンド側に追加する。
5. カード商品枠は10、キーストーン商品枠は3をシリアライズ済み配列で受け取る。実行時の `Instantiate` は行わず、在庫数が枠数未満なら余剩枠を非表示にする。
6. 購入可能性は `interactable`、グレーダウン、札の組み合わせで示す。カード購入済みは「売り切れ」、購入したキーストーンは「売り切れ」、残り2枠は「購入不能」とする。単なるゴールド不足は商品の存在を隠さず、押下時または無効理由表示で上部へ `お金がたらないよ！` を通知する。
7. 宿屋は事前に現在HP／最大HP、回復後HP、必要ゴールド、利用可否、利用不能理由を表示する。操作時に確認モーダルを挟まず、成功後に表示モデルを再取得する。
8. 上部ステータスはHP、スタン耐性、ゴールド、購入済みキーストーン、所持アイテム、次の遠征名、遠征進行状況、OverHunt帰還の確定情報を受け取れる構造にする。情報が存在しない項目はダミー文字を出さず、事前配置済みの対応枠を非表示にする。
9. 施設パネルを閉じると施設選択画面へ戻る。工房とクエストは同じ準備中Viewを別の文言で使用できるが、どちらかを開いた状態もEditorで確認できるようにする。
10. 出発成功時だけ `BattleScene` へ遷移する。失敗時はScene遷移せず、バックエンドから受けた理由を上部メッセージに表示する。

## 具体的手順

1. `OutpostSceneController`、`RunLifecycleService`、`ShopService`、`RunState`、`ShopCatalogSO`、`OutpostEconomyConfigSO`、`RunDefinitionSO` の現行APIとテストを一覧化し、維持するゲームルールとUIに必要な追加照会を分ける。
2. Outpost表示専用モデルを定義する。最低限、上部ステータス、宿屋、10件のカード商品、3件のキーストーン商品、出発可否を別データにし、Viewに `RunState` の変更可能参照を渡さない。
3. 購入、宿屋、出発の操作結果を返す経路を整える。既存テストが `bool` APIに依存する場合は互換性を残し、新UIは理由付きAPIを使う。文言は日本語で、表示向けと開発向けエラーを分ける。
4. `Assets/GameData/Prefabs/` に施設ボタン、カード商品枠、キーストーン商品枠、売り切れ／購入不能札、宿屋、準備中の必要部品を事前構築する。ショップ商品はカード一覧やBattle手札のPrefabを流用しない。
5. `OutpostScene` にEventSystemを事前配置し、Canvas配下に背景、共通上部領域、施設選択ルート、宿屋ルート、ショップルート、準備中ルート、設定モーダルを事前配置する。各ルートの初期active状態を明示する。
6. カード商品枠を10個、キーストーン商品枠を3個ショップ内に事前配置し、順序とInspector配列が一致するよう名前を付ける。ダミー商品を入れ、Sceneビューで通常／セール／売り切れ／購入不能の概形を確認する。
7. Viewスクリプトにシリアライズ済み参照を割り当て、データ表示、ボタン無効化、札の表示、操作イベントだけを実装する。Viewに価格計算、ゴールド減算、デッキ追加、回復、遠征開始を記述しない。
8. `OutpostSceneController` からUI構築用の `ClearScreen`、`CreateBackground`、`CreatePanel`、`CreateText`、`CreateFacilityButton` と、それらに依存する再構築フローを撤去する。起動時はInspector参照の検査、在庫確保、表示モデル取得、初期画面表示の順に限定する。
9. 宿屋利用、カード購入、キーストーン購入の成功後はScene／パネルを作り直さず、現在パネルの表示モデルと上部ステータスを再反映する。失敗後はゲーム状態を変更せず、上部メッセージに理由を表示する。
10. 施設選択→各施設→戻る、出発成功→Battle、設定モーダル開閉の入力状態を検査する。非表示パネルのButtonが反応しないことも確認する。
11. EditModeで商品枠数、必須参照、EventSystem、CanvasScaler、初期active状態を検査し、PlayModeでサービス結果と表示状態の対応を検査する。

## 依存関係

- 先行: `01-common-ui-foundation.md` のCanvas基準、上部ステータス／メッセージView、設定モーダル、共通スタイル規約。
- 仕様: ADR-013（Outpost施設）、ADR-015（ショップ在庫とキーストーン）、ADR-017（事前構築UIと最新の表示仕様）、Architecture ADR-002（Viewの責務）。
- バックエンド: `RunSession`、`RunLifecycleService`、`ShopService`、`RunProgressService`、`RunState`、`ShopCatalogSO`、`OutpostEconomyConfigSO`、`RunDefinitionSO`。
- 後続: カード一覧PLANがOutpostの「デッキ確認」導線を接続する。死亡／致命的エラーPLANが新規RUN後メッセージを共通通知経路へ発行する。

## 並列可否

- 表示専用モデル／バックエンド照会API、Outpost固有Prefab素材、自動テストは、公開受け口と担当ファイルを先に固定すれば並列化できる。
- `OutpostScene.unity` と `OutpostSceneController.cs` は統合点のため、それぞれ一人の担当者だけが編集する。共通UI基盤の担当者と同時に `OutpostScene.unity` を変更しない。
- ビジュアル素材は、必要ピクセル寸法、9-slice境界、命名、インポート設定をOutpost統合担当者が先に提示すれば、アセット生成担当と並列化できる。
- 共通基盤の受け口が未確定の間は、Sceneへの統合を行わない。モックアップ作成と既存バックエンドテストは先行できる。

## 受入条件

- Play Modeに入らずに、`OutpostScene` の背景、上部領域、施設選択、宿屋、ショップ、準備中パネル、設定モーダルをSceneビューまたはPrefab Modeで確認できる。
- 起動時は必ず施設選択画面が開き、宿屋、ショップ、工房、クエストカウンター、出発が表示される。
- 工房とクエストカウンターを開くと画面内に「準備中」が表示され、戻る操作が機能する。バックエンドは追加されない。
- 宿屋は既存式による全回復と支払いを行い、確認モーダルを出さない。満タン／ゴールド不足時はボタンと理由が一致する。
- カード商品枠が正確に10、キーストーン商品枠が正確に3事前配置され、実行時にUIオブジェクトを追加／破棄しない。
- カード購入後は同じ在庫枠へ「売り切れ」札が出て、再購入できず、商品は補充／再抽選されない。
- キーストーン1個の購入後は購入枠が「売り切れ」、残り2枠が「購入不能」となり、どの枠も選択できない。
- ゴールド不足で購入できず、上部に `お金がたらないよ！` が表示される。その際に商品やRUN状態は変更されない。
- 購入、回復、画面切替後にCanvas配下全体の破棄と再生成を行わず、既存Viewが更新される。
- 上部にHP、スタン耐性、ゴールド、キーストーン、所持アイテム、次の遠征と進行、OverHunt帰還情報を受け取れる。値が存在する項目はバックエンド状態と一致する。
- 出発の成功時だけBattleSceneへ遷移し、実行中のRUN、デッキ、ゴールド、購入済みキーストーンが維持される。
- `OutpostSceneController` に `new GameObject`、UI構築目的の `AddComponent`、Canvas子要素の全破棄が残っていない。`FindAnyObjectByType` を通常のUI参照取得に使っていない。

## 検証

- EditMode: Sceneのルート、EventSystem、必須Inspector参照、商品10／3枠、各パネルの初期active状態、ボタンイベントの割当を検査する。
- 純C#: 在庫固定、重複なし最大10件、購入済み枠の再購入防止、キーストーン1個制限、宿屋の価格と全回復、失敗理由が状態変更と整合することを確認する。
- PlayMode: 施設選択から各パネルへの遷移、戻る、即時購入／回復、札の更新、ステータス更新、メッセージ、出発を操作する。
- 手動: EditorのSceneビューでレイアウトを確認し、Gameビューで `1280 x 720`、1920 x 1080、2560 x 1440、3840 x 2160を確認する。札と商品名／価格が重ならず、10商品が判別できることを確認する。
- 回帰: Outpost滞在中にショップを閉じて開き直しても在庫と購入済みが変わらず、出発後のBattle起動が維持されることを確認する。

## 主な対象ファイル

- `Assets/GameData/Scenes/OutpostScene.unity`
- `Assets/Code/Runtime/UIs/OutpostSceneController.cs`
- `Assets/Code/Runtime/UIs/`（Outpostのパネル／スロットViewと表示専用モデル）
- `Assets/GameData/Prefabs/`（Outpost固有の施設ボタン、商品枠、札、宿屋、準備中パネル）
- `Assets/UI/Art/`、`Assets/UI/Fonts/`（Outpost外観素材）
- `Assets/Code/Runtime/Run/RunSession.cs`
- `Assets/Code/Runtime/Run/RunLifecycleService.cs`
- `Assets/Code/Runtime/Run/ShopService.cs`
- `Assets/Code/Runtime/Run/RunState.cs`（新しいゲーム状態を追加せず、表示元の確認に使う）
- `Assets/Code/Definitions/Run/ShopCatalogSO.cs`
- `Assets/Code/Definitions/Run/OutpostEconomyConfigSO.cs`
- `Assets/Code/Definitions/Run/RunDefinitionSO.cs`
- `Assets/Code/Editor/`（OutpostScene／Prefab参照検証）
- 既存テスト配下または、プロジェクトの現行分類に合わせたOutpostの純C#／EditMode／PlayModeテスト
