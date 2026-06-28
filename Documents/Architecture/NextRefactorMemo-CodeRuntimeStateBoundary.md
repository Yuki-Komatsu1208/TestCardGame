# 次回リファクタメモ: C#ランタイム状態管理とUnity境界の整理

作成日: 2026-06-28

## 前提方針

このメモは `Documents/ADRs/Architecture/ADR-002-code-owned-state-and-view-only-presentation.md` を前提にする。

* UnityのScriptableObject/アセットは、Unity上での編集、調整、データ生成を簡単にするための定義データとして扱う。
* ゲーム中の状態管理、判定、データ処理、ルール適用はC#ランタイム側で行う。
* ViewはランタイムモデルまたはViewModelを表示し、ユーザー入力をコントローラーへ通知するだけにする。
* Viewはゲーム状態を生成、補正、変更しない。
* 例外は、ステージ配置などUnityエディタ上で編集する方が明らかに自然な定義データに限る。ただし、その場合もプレイ中状態はC#ランタイムモデルに変換して扱う。

## 今回の監査範囲

主に以下を確認した。

* Enemy定義、Enemyランタイム、Enemy行動
* ActionEffect定義、ActionEffectランタイム
* StatusEffect定義、StatusEffectランタイム
* CardModifier定義、CardModifierランタイム
* Battle/Run系Controller、Service
* Board/Cell/Unit
* BattleUI、CardView、HandView、CardListSelectionOverlayなどのView

カード本体については前回までの整理で `RunState.playerDeck` を `List<CardBase>` に寄せ、カード状態の正をC#ランタイムモデルへ移した。今回の主目的は、カード以外にも同じ方針から外れている箇所がないかを見ること。

## 結論

カード以外にも、設計方針から外れている箇所が残っている。

特に優先度が高いのは以下。

1. `StatusEffectSO` がゲームロジックを持っている。
2. `EnemyActionPlan` と一部 `ActionEffect` が `GameController.Instance` に直接依存している。
3. `CellBuilder` がランタイムモデル生成とView生成を同時に担当している。
4. `BattleUI` / `CardListSelectionOverlay` / `CardView` が表示以外の判断やランタイム生成に踏み込んでいる。
5. `GameController` がモデル操作、View操作、戦闘フロー、生成処理をまとめて抱えている。

次回以降は、以下の優先順で分割すると安全。

## 優先度A: 先に直すべき責務違反

### 1. StatusEffectSOからゲームロジックを外す

対象:

* `Assets/Scripts/Definitions/StatusEffects/StatusEffectSO.cs`
* `Assets/Scripts/Definitions/StatusEffects/BurnStatusEffectSO.cs`
* `Assets/Scripts/Definitions/StatusEffects/PoisonStatusEffectSO.cs`
* `Assets/Scripts/Definitions/StatusEffects/ShieldStatusEffectSO.cs`
* `Assets/Scripts/Definitions/StatusEffects/SleepStatusEffectSO.cs`
* `Assets/Scripts/Definitions/StatusEffects/WeakStatusEffectSO.cs`
* `Assets/Scripts/Runtime/StatusEffects/StatusEffectInstance.cs`
* `Assets/Scripts/Runtime/StatusEffects/StatusEffectCollection.cs`
* `Assets/Scripts/Runtime/Controller/Services/StatusEffectService.cs`
* `Assets/Scripts/Runtime/Controller/Services/DamageService.cs`

現状の問題:

* `StatusEffectSO` と派生SOが `OnTurnStart`、`OnTurnEnd`、`CanAct`、`Merge` などのゲーム処理を持っている。
* SOが定義データではなく、状態異常の挙動そのものになっている。
* `DamageService` が `ShieldStatusEffectSO` や `WeakStatusEffectSO` の型、またはEffectIdに依存してダメージ計算している。

望ましい形:

* `StatusEffectSO` はID、表示名、説明、デフォルト値、アイコンなどの定義だけを持つ。
* 状態異常の挙動はC#ランタイム側の `StatusEffectBehavior` か `StatusEffectRule` に移す。
* `StatusEffectInstance` は「どの状態異常か」「残りターン」「スタック量」などのランタイム状態を持つ。
* `StatusEffectService` がターン開始、ターン終了、行動可否、マージ、ダメージ補正を処理する。
* `DamageService` はSO型ではなく、ランタイム側の状態種別またはルール結果に依存する。

推奨作業:

1. `StatusEffectId` または状態異常種別VOを作る。
2. `StatusEffectSO` を定義データに縮小する。
3. 現在SOにある `OnTurnStart` / `OnTurnEnd` / `CanAct` / `Merge` をランタイムサービスへ移す。
4. `DamageService` のShield/Weak参照をSO型依存から外す。
5. 既存アセットはIDと初期値を保持するだけにする。

### 2. GameController.Instance依存をランタイムロジックから外す

対象:

* `Assets/Scripts/Runtime/Actions/Effects/eSummonEnemy.cs`
* `Assets/Scripts/Runtime/Character/Enemies/EnemyActionPlan.cs`
* `Assets/Scripts/Runtime/Controller/GameController.cs`

現状の問題:

* `eSummonEnemy` が `GameController.Instance.SpawnEnemyDuringBattle(...)` を直接呼ぶ。
* `EnemyActionPlan.TryExecute` が `GameController.Instance.PlayUnitAttackAnimation(...)` を直接呼ぶ。
* ランタイムロジックがMonoBehaviourのSingletonに依存しており、View境界が崩れている。

望ましい形:

* 召喚は `ActionContext` に含めたランタイム用Spawnサービス、または `BattleSpawnService` へ依存する。
* 敵攻撃アニメーションはドメインロジックから直接再生しない。
* `EnemyActionPlan` は「行動が成功した」という結果またはイベントを返す。
* `GameController` やView層がイベントを受けてアニメーションを再生する。

推奨作業:

1. `ActionContext` に召喚用インターフェースを追加するか、`BattleRuntimeContext` を作る。
2. `eSummonEnemy` はそのインターフェースだけを呼ぶ。
3. `EnemyActionPlan.TryExecute` の戻り値を単純なboolから行動結果へ拡張する。
4. `GameController.PlayUnitAttackAnimation` はView同期側から呼ばれるようにする。

### 3. CellBuilderをModel生成とView生成に分ける

対象:

* `Assets/Scripts/Runtime/BoardManage/CellBuilder.cs`
* `Assets/Scripts/Runtime/Controller/GameController.cs`
* `Assets/Scripts/Definitions/Stage/StageDefinitionSO.cs`
* `Assets/Scripts/Definitions/Character/Enemies/EnemySpawnDefinition.cs`

現状の問題:

* `CellBuilder` が `Board`、`PlayerUnit`、`EnemyUnit` などのランタイムモデルを作る。
* 同じクラスがセルPrefab、UnitView、Text、Image、色などのViewも生成・設定する。
* ステージ定義のアセット編集は許容できるが、戦闘中の正となる状態とView構築が密結合している。

望ましい形:

* `StageDefinitionSO` はステージ編集用の定義データとして維持する。
* `BattleModelFactory` または `StageRuntimeBuilder` が、定義から `Board`、`PlayerUnit`、`EnemyUnit` を生成する。
* `BattleViewBuilder` または `BoardViewFactory` が、生成済みモデルを受け取ってViewを作る。
* View側はモデルのIDや座標を参照して表示を同期する。

推奨作業:

1. `CellBuilder` からランタイムモデル生成部分を抽出する。
2. `CellBuilder` からView生成部分を抽出する。
3. `GameController` は両者を順番に呼ぶだけにする。
4. 戦闘中の敵追加も同じ生成経路を使う。

## 優先度B: Viewを表示専念へ寄せる

### 4. BattleUIの報酬判断とフロー制御を外す

対象:

* `Assets/Scripts/Runtime/Controller/BattleUI.cs`
* `Assets/Scripts/Runtime/Controller/RunController.cs`
* `Assets/Scripts/Runtime/Run/RewardChoice.cs`

現状の問題:

* `BattleUI` が報酬ボタン生成、報酬種別判定、ヒール報酬とカード選択報酬の分岐を持っている。
* UIが `RunController.ChooseHealReward` や `ChooseDeckCardForReward` を直接選んでいる。
* 表示だけではなく、報酬選択フローの一部を持っている。

望ましい形:

* `RunController` または `RewardSelectionController` が報酬選択の入口を管理する。
* UIは `RewardChoiceViewModel` の一覧を表示し、押された選択IDを通知する。
* カード対象選択が必要な場合も、Controller側が次に表示するViewModelを決める。

推奨作業:

1. `RewardChoiceViewModel` を作る。
2. `BattleUI.ShowRewards` はViewModelを表示するだけにする。
3. 報酬クリック時は `rewardId` またはindexだけを通知する。
4. 報酬適用とカード選択遷移はController側へ寄せる。

### 5. CardListSelectionOverlayのプレビュー生成を外す

対象:

* `Assets/Scripts/Runtime/UIs/CardListSelectionOverlay.cs`
* `Assets/Scripts/Runtime/Cards/Views/CardView.cs`
* `Assets/Scripts/Runtime/Run/RewardChoice.cs`

現状の問題:

* `CardListSelectionOverlay` が `RewardChoice.CanApplyTo` と `CreatePreview` を呼んでいる。
* Overlayが `FindAnyObjectByType` やreflectionで `HandView.cardPrefab` を探している。
* UIが報酬プレビュー生成とカード選択可否判定に踏み込んでいる。

望ましい形:

* Overlayは `CardSelectionItemViewModel` の一覧を表示するだけにする。
* Controllerが選択可能/不可、プレビュー表示用カード、適用後テキストを事前に作る。
* Prefab参照はInspectorで明示的に渡すか、専用ViewFactoryで管理する。

推奨作業:

1. `CardSelectionItemViewModel` を作る。
2. `RunController` 側で候補リストとプレビューを作る。
3. Overlayから `RewardChoice` 依存を外す。
4. Overlayからreflectionを削除する。

### 6. CardViewから入力配線とGameController探索を外す

対象:

* `Assets/Scripts/Runtime/Cards/Views/CardView.cs`
* `Assets/Scripts/Runtime/UIs/MoveCardDragHandler.cs`
* `Assets/Scripts/Runtime/Cards/Views/HandView.cs`

現状の問題:

* `CardView.Bind` が表示更新だけでなく、`MoveCardDragHandler` へカード、移動量、閾値、`GameController` を渡している。
* Viewが `FindAnyObjectByType<GameController>()` を通じて入力先を探している。

望ましい形:

* `CardView` はカードの表示だけを行う。
* 入力受付は専用InputViewまたはHandPresenterが設定する。
* `MoveCardDragHandler` は入力イベントを外へ通知するだけにし、使用カードや移動の判断はControllerへ送る。

推奨作業:

1. `CardView.Bind` からController探索とDragHandler設定を外す。
2. `HandView` または `HandPresenter` が入力コールバックを注入する。
3. `MoveCardDragHandler` は `OnDropped(screenPosition)` のような通知に限定する。

## 優先度C: Controller/Serviceの整理

### 7. GameControllerをアプリケーション制御に縮小する

対象:

* `Assets/Scripts/Runtime/Controller/GameController.cs`

現状の問題:

* 戦闘開始、ターン進行、カード使用、敵召喚、死亡処理、View同期、アニメーション再生を持っている。
* `Instance` がランタイムロジックから参照されることで依存方向が逆転している。

望ましい形:

* `GameController` はUnityシーンの入口として、サービスを組み立ててフローを進める。
* ドメイン処理はService/Modelへ移す。
* View操作はViewSync/Presenterへ移す。
* ランタイムロジックから `GameController.Instance` を呼ばない。

推奨作業:

1. 戦闘状態操作を `BattleService` または `BattleRuntime` に抽出する。
2. View同期を `BattleViewPresenter` / `BattleViewSync` に抽出する。
3. 死亡処理をモデル側処理とView側非表示処理に分ける。
4. `GameController.Instance` 参照を段階的に削除する。

### 8. CardPlayServiceを入力座標変換とカード実行に分ける

対象:

* `Assets/Scripts/Runtime/Controller/Services/CardPlayService.cs`
* `Assets/Scripts/Runtime/Controller/Services/BoardTargetingService.cs`

現状の問題:

* `CardPlayService` が画面座標からセルを探す処理と、カード効果実行をまとめて扱っている。
* 画面座標はView/Input境界の情報で、カード実行サービスの中に入るとテストしにくい。

望ましい形:

* `BoardTargetingService` はInput/View Adapterとして画面座標からセル座標を返す。
* `CardPlayService` はカード、使用者、対象セル座標または対象Cellを受け取り、ドメイン処理だけを行う。

推奨作業:

1. `TryUseCardAtScreenPosition` 系をController/Input Adapter側へ寄せる。
2. `CardPlayService` は `TryUseCard(card, actor, targetCell)` の形を中心にする。
3. UI/DragHandlerは画面座標を通知するだけにする。

## 優先度D: 小さな整理候補

### 9. ActionEffectSOのConfigure系メソッドを見直す

対象例:

* `KnockbackEffectSO.Configure`
* `PoisonSpreadEffectSO.Configure`
* `PositionAttackEffectSO.Configure`
* `SummonEnemyEffectSO.Configure`
* `StatusEffectSO.Configure`

現状の問題:

* SOに実行時から呼べるmutatorがある。
* アセット定義がランタイム処理中に変更可能に見える。

望ましい形:

* SOは基本的に不変の定義データとして扱う。
* エディタ用の補助設定が必要ならEditorコードに寄せる。
* ランタイムで派生値が必要なら、SOを変更せずruntime DTOへ変換する。

### 10. ランダムとボードサイズの扱いをサービス化する

対象例:

* `Assets/Scripts/Runtime/Actions/Effects/eForcedMove.cs`
* `Assets/Scripts/Runtime/Actions/Effects/eTeleportTarget.cs`
* `Assets/Scripts/Runtime/Actions/Effects/eSummonEnemy.cs`

現状の問題:

* `UnityEngine.Random` をランタイム効果内で直接使っている。
* `eTeleportTarget` に `0..10` のようなボード範囲の直書きがある。

望ましい形:

* ランダムは `IRandomService` のようなランタイムサービス経由にする。
* ボードサイズや有効セル探索は `Board` または `BoardQueryService` に寄せる。

### 11. Cellの表示用テキストを分離する

対象:

* `Assets/Scripts/Runtime/BoardManage/Cell.cs`
* Tooltip系View

現状の問題:

* `Cell` が `TooltipText` のような表示用文字列を持っている。

望ましい形:

* `Cell` は状態だけを持つ。
* Tooltip表示文字列はViewModelまたはFormatterが作る。

### 12. PlayerUnit/EnemyUnitの定義変換をFactoryへ寄せる

対象:

* `Assets/Scripts/Runtime/Character/Player/PlayerUnit.cs`
* `Assets/Scripts/Runtime/Character/Enemies/Units/EnemyUnit.cs`

現状の問題:

* UnitコンストラクタがSO定義からランタイム状態を作っている。
* 今は許容範囲だが、生成ルールが増えるとEntityが初期化責務を持ちすぎる。

望ましい形:

* `PlayerRuntimeFactory` / `EnemyRuntimeFactory` がSO定義からランタイムUnitを作る。
* Unit自体は状態と基本操作を持つ。

## 許容する例外

以下はUnity側に残してよい。ただし、プレイ中状態はC#ランタイムへ変換して扱う。

* `StageDefinitionSO` によるステージ配置編集
* `EnemyDefinitionSO` による敵パラメータ、行動定義の編集
* `CardDefinitionSO` によるカード基本データの編集
* `ActionEffectSO` による効果パラメータの編集
* `CardModifierSO` によるEnchant/Modifier定義の編集
* Editor拡張、CustomDrawer、AssetDatabaseを使った編集補助

## 次回の推奨作業順

1. `StatusEffectSO` のロジック剥がし
2. `DamageService` / `StatusEffectService` のSO依存解消
3. `GameController.Instance` を `eSummonEnemy` と `EnemyActionPlan` から除去
4. `CellBuilder` を `StageRuntimeBuilder` と `BattleViewBuilder` に分割
5. `BattleUI` の報酬フローをController/ViewModelへ移動
6. `CardListSelectionOverlay` のプレビュー生成とreflectionを削除
7. `CardView` の入力配線をHand/Presenterへ移動
8. `GameController` を薄いシーン入口へ縮小

## 判断保留

以下は実装前に改めて確認する。

* 状態異常IDを文字列で扱うか、enum/VOで扱うか。
* Enemy行動結果イベントをC# eventで出すか、結果オブジェクトでControllerへ返すか。
* Stage生成をどこまでUnity編集例外として残すか。
* ViewModel層を明示的なクラスとして作るか、まずはDTOで軽く始めるか。

