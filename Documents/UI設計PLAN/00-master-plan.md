# UI設計PLAN 全体索引

## 目的

ADR-017を、独立して実装・検証できる作業単位へ分割する。
各PLANは実装順序を示すものであり、この文書群の作成時点ではコード、Scene、Prefabを変更しない。

## PLAN一覧

| 順序 | PLAN | 主な成果物 |
|---|---|---|
| 1 | [01 共通UI基盤](01-common-ui-foundation.md) | 解像度、テーマ、上部ステータス、メッセージ、設定画面 |
| 2 | [02 Outpost UI](02-outpost-ui.md) | 施設選択、宿屋、ショップ、準備中施設、出発 |
| 3 | [03 カード一覧と選択](03-card-list-and-selection.md) | 50枠一覧、詳細、3モード、複数選択、強化確認 |
| 4 | [04 Battle HUDと手札](04-battle-hud-and-hand.md) | 常設HUD、50枠手札、重なり、フォーカス、スクロール |
| 5 | [05 盤面フィットと対象表示](05-board-fit-and-targeting.md) | 可変盤面フィット、移動・効果範囲、ドラッグ表示 |
| 6 | [06 敵行動予告](06-enemy-intent-preview.md) | 副作用のない次行動照会、アイコン、対象範囲再計算 |
| 7 | [07 戦闘後・死亡・復旧](07-post-battle-and-recovery.md) | 報酬、OverHunt、死亡、新規RUN、致命的エラー復旧 |
| 8 | [08 旧UI生成撤去と検証](08-runtime-ui-removal-and-validation.md) | 旧生成コード削除、参照検証、総合回帰確認 |

## 依存関係

```text
01 共通UI基盤
├─ 02 Outpost UI
├─ 03 カード一覧と選択
└─ 04 Battle HUDと手札
   └─ 05 盤面フィットと対象表示
      └─ 06 敵行動予告

03 カード一覧と選択 ─┐
04 Battle HUDと手札 ─┴─ 07 戦闘後・死亡・復旧

02〜07完了 ───────── 08 旧UI生成撤去と検証
```

## 並列実装方針

共通UI基盤の契約と共通Prefabが確定した後、Outpost、カード一覧、Battle HUDは並列化できる。

### 並列化できる作業

* OutpostSceneとOutpost専用Presenter/Viewの実装
* 共通カード一覧Prefabとカード選択バックエンドの実装
* BattleSceneのHUD・手札Viewの実装
* UI用オリジナル画像、枠、札、アイコンの制作。ただし01で寸法とスライス規則を決めた後に開始する
* 純C#のViewModel、選択要求、敵意図問い合わせの単体テスト

### 直列にする作業

* 同じScene、Prefab、`.meta` を複数エージェントが同時編集する作業
* `BattleUI`、`RunController`、`GameController` のイベント契約変更
* カード選択APIの移行と、それを利用する各Effectの変更
* 旧実行時生成コードの削除

### 所有権の目安

| 作業領域 | 排他的に扱う主ファイル・Asset |
|---|---|
| Outpost担当 | `OutpostScene.unity`、Outpost専用Prefab、`OutpostSceneController` |
| カード一覧担当 | カード一覧Prefab、カード詳細Prefab、`CardListSelectionOverlay`、選択要求型 |
| Battle HUD担当 | `BattleScene.unity`、HUD・手札Prefab、`BattleUI`、`HandView` |
| 盤面担当 | Cell/Board View、`CellBuilder`、対象表示サービス |
| 敵意図担当 | Enemyの予告モデル、`EnemyActionPlan`、`EnemyUnit`、意図表示View |
| フロー担当 | `RunController`、`RewardController`、死亡・復旧サービス |

共有ファイルを変更する必要が生じた場合は、先に契約を文書化し、所有担当へ変更を集約する。

## 該当工程で残る確認事項

PLAN作成によって判明した未確定点をここへ集約する。該当機能へ着手する前に確認し、無関係なPLANは停止させない。

| PLAN | 確認事項 |
|---|---|
| 01 | 音量をマスターのみとするかBGM／SEへ分けるか、画面設定の項目、設定保存方式 |
| 01 | 上部ログの履歴数、表示秒数、自動スクロールなどの初期調整値 |
| 03 | カード固有のIntrinsic Modifierを詳細パネルのMOD一覧へ含めるか |
| 04 | 敵HUDを事前配置するための最大同時出現数。召喚を含めた上限が必要 |
| 05 | 盤面セルも固定上限数を事前配置するか、完成済みCell Prefabの実行時配置を可変盤面の例外として許可するか |
| 05 | 複数Effectカードの範囲を一意に合成できない場合の表示規則 |
| 06 | 行動不能、全Plan実行不能の敵で意図アイコンを非表示にするか疑問符にするか |
| 06 | `eTelegraph` を「その他」の次行動として見せるか、その先の実害行動を見せるか |

手札のスクロール切替は既定20枚で実装し、実画面確認後に設定値だけを調整するため、実装開始を止める未確定事項には含めない。

## 全体完了条件

* OutpostSceneとBattleSceneの主要UIをPlay Modeへ入らず確認できる。
* UI構造を作るための実行時 `new GameObject`、`AddComponent` が残っていない。
* ADR-017で定めた固定スロット数がEditor検証される。
* Viewはゲーム状態を直接変更せず、入力イベントとIDだけを外部へ通知する。
* 1280 x 720、1920 x 1080、2560 x 1440で主要情報が欠けない。
* 最大50枚のカード、ショップ10商品、報酬3択、最大数の敵を含む画面で操作できる。
* 通常戦闘、ボス帰還、OverHunt、死亡、致命的エラー復旧を一連で確認できる。
* 既存のゲームルール、カード効果、ショップ抽選、報酬適用に回帰がない。

## オーケストレーション時の確認手順

1. 各担当は対象PLANの「対象外」を守る。
2. 変更開始前に対象Scene・Prefabの所有者が重複していないことを確認する。
3. 純C#契約を先に実装・テストし、その後Viewを接続する。
4. 各PLANの受入条件を満たした時点で横断レビューする。
5. 08までは旧UI生成経路を一括削除しない。
6. 08で旧経路を削除し、全画面を通した回帰確認を行う。
