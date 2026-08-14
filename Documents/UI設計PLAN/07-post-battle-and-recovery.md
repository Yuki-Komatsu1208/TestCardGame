# PLAN 07: 戦闘後・死亡・致命的エラー復旧

## 目的

通常報酬、遠征ボス後の選択、OverHunt継続判断、RUN勝利、死亡、新規RUN開始、致命的エラー復旧を、事前構築UIと表示専用データへ移行する。

## 現状

* `BattleUI` が報酬パネルとRUN終了パネルを実行時生成している。
* `BattleUI` が報酬種別を判定し、回復、レベルアップ、MOD付与の処理入口を選んでいる。
* `RunController` が戦闘結果を受け、報酬、帰還、OverHunt、次ステージ開始を分岐している。
* 敗北時はRUNを `Failed` にするが、新しいRUNを開始してOutpostへ戻る正式な導線はない。
* 致命的エラーを一元的に新規RUNへ復旧させる仕組みと、シーン間で上部メッセージを引き渡す仕組みはない。

## 対象

* 事前構築された報酬3枠
* レベルアップ・MOD付与のカード選択とOK／NO確認
* 遠征ボス後の帰還／OverHunt選択
* OverHunt戦後の帰還／続行選択と保留ゴールド表示
* RUN勝利画面
* 死亡画面、ランダム短文ボタン、新規RUN開始
* 致命的エラーの一元復旧とOutpostメッセージ
* ゲーム内上部ログへの結果通知

## 対象外

* 報酬抽選確率、ゴールド量、OverHunt係数などのバランス変更
* 新しい報酬種別、アイテム、工房、クエストの実装
* 敵AIや盤面対象表示の変更
* セーブデータやRUN途中再開の新設

## 設計方針

### 表示モデル

Viewへゲームオブジェクトやサービスを直接渡さず、用途ごとの表示モデルを用意する。

* `RewardChoiceViewModel`
  * 選択ID
  * タイトル、説明
  * 表示種別（LevelUp、Heal、Modifier）
  * 選択可否と不可理由
* `RouteChoiceViewModel`
  * 帰還／続行の操作ID
  * 現在HP
  * OverHunt深度
  * 保留ゴールド
  * 死亡時RUN失敗の警告
* `RunEndViewModel`
  * 勝利／死亡
  * 主メッセージ
  * ボタン文言
* `RecoveryNotice`
  * Outpostで表示するプレイヤー向け文言
  * 表示種別（通常、警告、致命的）

Viewは選択IDをPresenterへ返すだけとし、報酬適用やRUN遷移を判断しない。

### 死亡フロー

1. 戦闘敗北を確定し、現在RUNを `Failed` にする。
2. 死亡画面へ `致命的な傷を負った` と表示する。
3. `死亡した`、`目の前が暗くなる`、`立ち上がれないようだ……` から、注入可能な乱数サービスで1つ選ぶ。
4. 表示中はボタン文言を固定する。
5. ボタン押下後、旧RUNを破棄して同じ `RunDefinitionSO` から新しいRUNを開始する。
6. `新たな傭兵がやってきた` をシーン間通知として登録する。
7. OutpostSceneへ遷移し、上部メッセージ領域で通知を1回表示する。

### 致命的エラー復旧

* 復旧窓口を1つに集約する。各Viewが個別にScene遷移しない。
* 復旧開始時に多重実行防止フラグを立てる。
* 開発者向け詳細は `Debug.LogException` または日本語コンテキスト付きエラーログへ出す。
* 現在RUNを破棄し、既知の `RunDefinitionSO` から新しいRUNを開始する。
* `致命的なエラーが発生しました` を黄／琥珀色の通知として登録する。
* Outpostへ遷移し、通知を1回表示した後に消費する。
* 新規RUN生成自体が失敗した場合は再帰的に復旧せず、入力を止めた最小エラー画面へ留める。

`RunSession` に「新しいRUNを明示的に開始する」「シーン間通知を1回だけ保持・取得する」ためのAPIを追加する案を第一候補とする。具体的な型名は実装時に既存責務と合わせる。

## 実装手順

1. `BattleUI`、`RunController`、`RewardController`、`RunProgressService` の現在のイベントと状態遷移をテストで固定する。
2. 報酬、ルート選択、RUN終了の表示モデルを純C#型として定義する。
3. 報酬適用と画面分岐をViewからPresenter／Controllerへ移す。
4. BattleSceneへ報酬3枠、ルート選択、勝利、死亡の各パネルを事前配置する。
5. 報酬種別に応じて、黄／金、緑、赤の枠を割り当てる。
6. PLAN 03のカード一覧を、レベルアップ・MOD付与対象選択へ接続する。
7. OK／NO確認後にだけ強化を確定する。回復は即時適用する。
8. OverHunt画面へ深度、保留ゴールド、死亡リスクを表示する。
9. 新規RUN開始APIとシーン間通知を追加し、死亡フローへ接続する。
10. 致命的エラー復旧窓口と多重実行防止を追加する。
11. Outpost上部メッセージ領域で、死亡後と致命的エラー後の通知を消費する。
12. 新経路の受入確認後、`BattleUI` の報酬・終了パネル生成処理をPLAN 08の削除対象として記録する。

## 依存関係

* PLAN 01の共通メッセージ領域とテーマ
* PLAN 02のOutpost上部メッセージ表示
* PLAN 03の戦闘外カード選択と強化確認
* PLAN 04のBattle HUDと事前構築パネル領域

## 並列可否

* 表示モデルと純C#の死亡・復旧サービスは、Scene UI作成と並列化できる。
* `BattleUI`、`RunController`、`RewardController` は相互にイベント契約を共有するため、同時編集を避ける。
* BattleSceneを編集する担当はPLAN 04〜06の担当と排他的にする。

## 受入条件

* 通常報酬が常に3つの事前配置枠へ表示される。
* 回復は確認なしで適用される。
* レベルアップとMOD付与は対象カード選択とOK／NO確認を経る。
* 遠征ボス後に帰還／OverHuntを選べる。
* OverHunt後に保留ゴールドを確認して帰還／続行を選べる。
* 死亡画面へ指定メッセージとランダム短文ボタンが表示される。
* 死亡後は新しいRUNとしてOutpostへ戻り、`新たな傭兵がやってきた` が1回表示される。
* 致命的エラー後は新しいRUNとしてOutpostへ戻り、指定警告が1回表示される。
* 同じ致命的エラーからScene遷移や新規RUN開始が多重実行されない。
* Viewが報酬やRUN状態を直接変更しない。

## 検証

### EditModeテスト

* 報酬種別から表示色種別への変換
* 死亡ボタン文言が候補内から選ばれ、画面表示中は変化しないこと
* シーン間通知が1回だけ消費されること
* 致命的復旧の多重実行防止
* 新規RUNでHP、ゴールド、デッキ、OverHunt状態が初期化されること

### PlayModeテスト

* 回復、レベルアップ、MOD付与の3報酬フロー
* ボス帰還とOverHunt進入
* OverHunt帰還と続行
* 通常死亡とOverHunt死亡からの新規RUN
* 必須参照欠落を模した致命的エラーからOutpostへの復旧
* 1280 x 720と1920 x 1080で各モーダルが画面外へ出ないこと

## 主な対象ファイル

* `Assets/Code/Runtime/Controller/BattleUI.cs`
* `Assets/Code/Runtime/Controller/RunController.cs`
* `Assets/Code/Runtime/Controller/RewardController.cs`
* `Assets/Code/Runtime/Run/RunSession.cs`
* `Assets/Code/Runtime/Run/RunLifecycleService.cs`
* `Assets/Code/Runtime/Run/RunProgressService.cs`
* `Assets/GameData/Scenes/BattleScene.unity`
* `Assets/GameData/Scenes/OutpostScene.unity`
* 新規の報酬・ルート選択・RUN終了・復旧用View／Presenter／表示モデル
* 対応するEditMode／PlayModeテスト
