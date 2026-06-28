# ADR-002: 状態管理はC#コードに集約し、Viewは表示だけを担当する

## ステータス
**承認済 (Accepted)**

## コンテキスト (背景)
カード、報酬、戦闘開始、手札表示の実装が進むにつれて、同じ「プレイヤーが持っているカード状態」を複数の場所で再構築する問題が出てきました。

特にカード周りでは、以下のような二重管理が発生していました。

* `RunState.playerDeck` が `DeckCardEntry` としてカード定義、レベル、Modifier定義を持つ。
* 戦闘開始時に `DeckCardEntry` から `CardBase` を生成する。
* 報酬選択UIでも `DeckCardEntry` から表示用の `CardBase` を一時生成する。
* レベルアップ報酬は `DeckCardEntry.level++` で適用され、`CardBase.ChangeLevel()` を経由していなかった。

この構造では、どこが正しいカード状態なのかが分かりにくくなります。
また、Viewが表示のためにドメインオブジェクトを生成したり、報酬適用後の状態を組み立てたりすると、Viewがロジックを持ち始めます。

本プロジェクトでは、UnityのScriptableObjectアセットはあくまで定義データとして扱い、プレイ中の状態はC#コード上のモデルに集約します。
Viewはそのモデルを画面に表示するだけにします。

## 決定事項 (Decision)
以下のアーキテクチャ方針を採用します。

* ベースとなる不変データはUnityアセットで管理する。
* プレイ中の状態管理はすべてC#コード上のランタイムモデルで行う。
* Viewはランタイムモデルを参照して表示するだけにし、ゲーム状態を生成・変更・補正しない。
* 報酬、レベルアップ、Enchant付与などの状態変更は、Viewではなくドメインモデルまたはサービス/コントローラー側で行う。
* 戦闘中状態とRun中状態を同じモデルで扱う場合、戦闘開始・終了などの境界で一時状態を明示的にリセットする。

カードについては、以下を標準構造とします。

* `CardDefinitionSO`
  * Unityアセット。
  * カード名、説明、レベル別コスト、基本クールタイム、基本Effect定義を持つ。
  * プレイ中の状態を持たない。

* `CardModifierSO`
  * Unityアセット。
  * Enchant/Modifierの定義を持つ。
  * プレイ中の付与状態そのものではない。

* `CardBase`
  * C#ランタイムモデル。
  * 現在の `Level`、生成済み `Effects`、付与済み `EnchantDefinitions`、runtime `Enchants`、`RemainingCooldown` を持つ。
  * `ChangeLevel()`、`AddEnchant()`、`StartCooldown()`、`TickCooldownAtTurnEnd()` など、カード状態の変更ルールを持つ。

* `RunState.playerDeck`
  * `List<CardBase>` とする。
  * Run中のプレイヤーカード状態の正とする。

* `PlayerUnit.Cards`
  * 戦闘中に使用するカード一覧。
  * Run中は `RunState.playerDeck` のカード状態を参照する。

* `CardView` / `HandView` / `CardListSelectionOverlay`
  * `CardBase` を受け取って表示する。
  * `CardBase` を新規生成して状態を再構築しない。
  * レベルアップやEnchant付与を直接実行しない。

報酬については、`RewardChoice` が適用可否、プレビュー生成、実適用のルールを持ちます。

* `RewardChoice.CanApplyTo(CardBase card)`
* `RewardChoice.CreatePreview(CardBase card)`
* `RewardChoice.ApplyTo(CardBase card)`

Viewはこれらの結果を表示するだけにします。

## 実装方針メモ
状態管理の責務は以下のように分けます。

```text
Asset Definition
  CardDefinitionSO
  CardModifierSO

Runtime State / Domain Model
  RunState
  CardBase
  PlayerUnit
  EnemyUnit
  Board / Cell
  StatusEffectCollection

Application / Flow Control
  RunController
  GameController
  TurnService
  CardPlayService
  UnitMoveService

View
  CardView
  HandView
  BattleUI
  CardListSelectionOverlay
  UnitView
```

Viewが許可される責務は以下に限定します。

* モデルの値をテキスト、画像、色、レイアウトへ反映する。
* ユーザー入力をコントローラー/サービスへ通知する。
* 表示用の一時プレビューを受け取り、それを表示する。

Viewが持たない責務は以下です。

* カード状態の生成。
* レベルアップ処理。
* Enchant/Modifier付与処理。
* クールタイム、HP、マナ、状態異常などのゲーム状態変更。
* アセット定義からランタイム状態を再構築する処理。

## 帰結 (Consequences)

### メリット (Pros)
* 状態の正がC#ランタイムモデルに集約され、同じカード状態をRun、戦闘、報酬UIで共有できます。
* Viewがロジックを持たないため、表示変更とゲームルール変更を分離できます。
* レベルアップやEnchant付与のルールが `CardBase` / `RewardChoice` に集まり、報酬UIと実適用で挙動がズレにくくなります。
* Unityアセットは定義データとして扱えるため、プレイ中状態がアセットに混ざりません。
* 将来的に保存/ロードを入れる場合も、保存対象はランタイムモデルの永続状態と明確に整理できます。

### デメリット / 今後の課題 (Cons / Future Work)
* `CardBase` がRun中の永続状態と戦闘中の一時状態を両方持つため、境界でのリセットを忘れると残クールタイムなどが持ち越される可能性があります。
* Viewが入力通知を持つことは許可しますが、入力後の状態変更は必ずコントローラー/サービス/モデルに委譲する必要があります。
* `CardBase` をRunStateに直接保持する方針は、将来的なセーブデータ永続化時にDTOへの変換層が必要になる可能性があります。
* 既存ドキュメントや古いADRには、旧設計の記述が残っている場合があります。方針が衝突する場合は本ADRを優先し、必要に応じて別ADRで更新履歴を残します。
