# UI設計PLAN 03: カード一覧・カード選択

## 1. 目的

ADR-017に従い、所持カードの閲覧、戦闘外の強化対象選択、戦闘中のカード効果対象選択を、用途ごとに明確に分離して実装する。

本PLANでは以下を達成対象とする。

* 共通カード一覧を横6列、最大50枠、縦スクロールで表示する。
* 一覧上で選んだカードの現在コスト、レベル、効果、全MODを右側の詳細パネルへ表示する。
* 閲覧、戦闘外選択、戦闘中選択の3モードを混同しない。
* 戦闘外では対象外カードをグレーアウトし、戦闘中では手札上の対象外カードを非表示にする。
* 最小選択数と最大選択数を要求ごとに指定できる、重複なしのトグル式複数選択へ対応する。
* 対象候補不足時は、選択画面を開く前に起点カードを使用不能と判定する。
* レベルアップとMOD付与だけは、対象選択後に「強化します」のOK／NO確認を挟む。
* Viewは表示と入力通知だけを担当し、選択可否、カード使用可否、強化適用はバックエンドへ委譲する。
* UIオブジェクトはSceneまたはPrefabへ事前配置し、実行時に生成・破棄しない。

## 2. ADR-017から確定している仕様

### 2.1 共通カード一覧

* RUN中の所持カード上限は50枚とする。
* 一覧用カードスロットを50枠事前配置する。
* 配置は横6列、縦スクロールとする。
* フィルター、検索、ソートは初期版の対象外とする。
* 未使用スロットは非表示にする。
* カードクリック時は、そのカードを選択表示し、右側へ詳細を表示する。
* 詳細には現在コスト、現在レベル、効果、付与済みMODを表示する。
* MODは複数ある場合も省略せず列挙する。

### 2.2 表示モード

| モード | 表示場所 | 対象外カード | 選択確定 |
| --- | --- | --- | --- |
| 閲覧 | Outpost／Battleの共通一覧 | 通常表示 | なし |
| 戦闘外選択 | 共通一覧 | グレーアウトし、理由を表示可能にする | OKで確定 |
| 戦闘中選択 | Battleの手札 | 非表示 | OKで確定 |

戦闘中選択は共通一覧を開く処理ではない。弾丸カード選択などでは、現在表示中の手札を選択モードへ切り替える。

### 2.3 複数選択

* 選択要求ごとに最小選択数と最大選択数を指定する。
* 同じカードインスタンスは1回だけ選択できる。
* 選択操作はトグル式とする。
* 選択順は結果へ影響しない。
* 選択数は `2 / 3` 形式で表示する。右辺は最大選択数とする。
* 選択数が最小未満の場合、OKは無効とする。
* 最大数へ達した後、未選択カードを追加選択することはできない。選択解除は可能とする。
* 候補数が最小選択数未満の場合、選択UIを開始しない。

### 2.4 強化確認

* レベルアップとMOD付与は、カードを選択後、OK／NO確認モーダルを表示する。
* 確認文言は「強化します」を基調とし、対象カードと強化内容を併記する。
* NOまたはキャンセルではカード状態を変更しない。
* 回復と購入では本確認モーダルを使用しない。

## 3. 現状調査

### 3.1 現在の一覧UI

`CardListSelectionOverlay` は、`Show` と `ShowCardOperation` により閲覧と選択を扱っている。ただし次の点がADR-017と一致しない。

* `PopulateDeckGrid` が既存要素を `Destroy` し、`CardView` を枚数分 `Instantiate` している。
* カードに `Button` がない場合、実行時に `AddComponent<Button>` している。
* `HandView` やシーン内 `CardView` を `FindAnyObjectByType` とreflectionで探索し、Prefab参照を補完している。
* 閲覧時の詳細表示と、報酬適用前の確認を同じ `confirmModal` で扱っている。
* 選択状態は単一の `selectedDeckIndex` だけで、複数選択に対応していない。
* 詳細表示はカードプレビュー中心で、右側へ常設する独立した詳細Viewになっていない。
* 選択不能カードは事前にグレーアウトされず、クリック後に警告を出す。

### 3.2 現在の戦闘中選択

`CardTargetSelectionService.RequestSelection` は、`CardListSelectionOverlay.Instance` または `FindAnyObjectByType` で一覧を探し、単一カードを選ばせる。

`eGunshot` と `eSuppressingShot` は、弾丸候補の存在を `CanExecute` で確認し、実行後に単一弾丸を選択して消費する。現在の候補存在確認は1枚選択に限れば正しいが、汎用的な最小選択数の概念はない。

### 3.3 現在の戦闘外選択と強化

`RewardChoice.CanApplyTo` がレベルアップ可否とMOD付与可否を判定し、`RewardController.ChooseDeckCardForReward` が対象カードへ変更を適用する。`CardListSelectionOverlay` は選択前後のカードを実行時生成し、確認モーダルに表示している。

`TownService.TryBuyLevelUp` と `TryBuyModifier` は対象カードをランダム選択する現行実装である。工房は今回「準備中」のため、本PLANでは工房機能を追加しない。将来、戦闘外カード選択を使う場合に同じ選択要求を利用できる形だけ用意する。

### 3.4 現在のカード表示

`CardView` は `CardBase` を直接保持し、`LateUpdate` ごとに表示を更新する。また、View内から `GameController` を探索してドラッグ処理を設定している。現在コスト算出に `GetCost(null)` を使用するため、プレイヤー状態に依存するコスト補正が将来追加された場合、正しい表示にならない可能性がある。

`HandView` は表示更新のたびに手札カードを全削除・再生成しており、戦闘中選択のフィルタ、トグル選択、選択数表示に必要な状態を保持できない。

## 4. 対象範囲

### 4.1 対象

* 共通カード一覧Prefabの事前構築
* 一覧用カードスロット50枠の事前配置
* 6列グリッドと縦スクロール
* 右側カード詳細パネル
* 閲覧モード
* 戦闘外の単一・複数選択モード
* Battle手札上の単一・複数選択モード
* 選択要求、候補判定、選択結果の表示専用モデルとバックエンドAPI
* 候補不足によるカード使用不能判定
* レベルアップ／MOD付与の強化確認モーダル
* 事前配置参照と50枠を検証するEditor検証
* 上記に対応するEditMode／PlayModeテスト

### 4.2 対象外

* カード検索、フィルター、ソート
* 50枚を超える所持カードへの対応
* デッキ編成、控えデッキ、カード所持上限そのもののゲームルール実装
* ショップ商品、報酬3択、戦闘手札の見た目の共通化
* 工房とクエストのバックエンド実装
* 新しいMOD削除ルールや強化費用の決定
* カード効果本文を機械的に再構成する仕組み
* 敵行動中にデッキ閲覧を許可するための非同期進行全体の改修
* UIテーマ画像、カードイラスト、フォント自体の制作

## 5. 責務分離案

### 5.1 バックエンド

バックエンドは次を担当する。

* 選択元カード列と、各カードを識別する選択トークンの作成
* 候補条件の評価
* 最小／最大選択数の検証
* 候補不足時の要求拒否
* カード効果の使用可否への候補数反映
* 確定結果を元のカードへ解決する処理
* 報酬または強化の適用
* キャンセル時に状態を変更しないことの保証

同名・同定義・同レベルのカードが複数存在し得るため、カード名や`CardDefinitionSO`を選択識別子にしない。初期実装では、要求作成時点の元リスト上のインデックスを不透明な選択トークンとして使う。確定前に、対象が同じリストの同じカードインスタンスであることを再検証する。

### 5.2 Presenter／Coordinator

PresenterまたはCoordinatorは次を担当する。

* バックエンドのカード情報を表示専用モデルへ変換する。
* 表示モードに応じて共通一覧または戦闘手札へ表示要求を渡す。
* Viewから通知されたトークンを選択集合へ反映する。
* `選択数 / 最大数` とOKボタンの活性状態を更新する。
* 右側詳細パネルへ現在選択／フォーカスカードを渡す。
* 強化対象確定後に確認モーダルを開く。
* モーダルのOK後だけ、Controllerまたは報酬サービスへ適用を依頼する。
* 排他的な選択要求の重複を拒否する。

### 5.3 View

Viewは次だけを担当する。

* 事前配置されたスロットへの表示モデル反映
* 未使用枠の表示／非表示
* 選択可能、グレーアウト、選択中、フォーカス中の視覚化
* クリック、OK、NO、キャンセル、スクロール入力の通知
* 詳細パネルと選択数の表示

Viewは`CardBase`、`RunState`、`RewardChoice`へ変更を加えない。`FindAnyObjectByType`、reflection、実行時`AddComponent`を通常経路にしない。必要な参照はInspectorから割り当てる。

## 6. 表示専用モデル案

型名は実装時に既存namespaceとの整合を確認して確定する。配置先候補は `Assets/Code/Runtime/UIs/Cards/` とする。

```csharp
public enum CardListMode
{
    Browse,
    OutOfBattleSelection
}

public readonly struct CardSelectionToken
{
    public int SourceIndex { get; }
}

public sealed class CardDisplayModel
{
    public CardSelectionToken Token { get; init; }
    public string Name { get; init; }
    public int CurrentLevel { get; init; }
    public int CurrentCost { get; init; }
    public int CurrentCooldown { get; init; }
    public string EffectDescription { get; init; }
    public IReadOnlyList<string> ModifierNames { get; init; }
    public bool IsSelectable { get; init; }
    public string DisabledReason { get; init; }
}

public sealed class CardSelectionRequest
{
    public string Title { get; init; }
    public IReadOnlyList<CardBase> SourceCards { get; init; }
    public Predicate<CardBase> IsCandidate { get; init; }
    public int MinimumSelectionCount { get; init; }
    public int MaximumSelectionCount { get; init; }
    public bool CanCancel { get; init; }
}

public sealed class CardSelectionResult
{
    public IReadOnlyList<CardSelectionToken> SelectedTokens { get; init; }
}
```

表示モデルにはUnity UIコンポーネントや変更可能なカード状態を持たせない。`CurrentCost`はPresenterが正しいプレイヤー文脈を渡して算出し、View内から`GetCost(null)`を呼ばない。

## 7. バックエンドAPI案

### 7.1 共通の候補検証

```csharp
public CardSelectionValidation Validate(CardSelectionRequest request);
public bool HasEnoughCandidates(CardSelectionRequest request);
```

検証内容は以下とする。

* `MinimumSelectionCount >= 0`
* `MaximumSelectionCount >= MinimumSelectionCount`
* `MaximumSelectionCount <= 50`
* 元リストがnullでない
* `IsCandidate`を満たす一意のカードインスタンス数が最小選択数以上

無効な要求は日本語の理由を返し、UIを開かない。通常の「対象カード不足」は例外ではなく、カード使用不能理由として扱う。

### 7.2 選択開始と完了

```csharp
public bool TryRequestSelection(
    CardSelectionRequest request,
    Action<CardSelectionResult> onConfirmed,
    Action onCancelled = null);
```

* 既に排他的な選択が進行中なら開始しない。
* 確定時に選択数、トークン重複、対象の現存、候補条件を再検証する。
* コールバックには順序の意味を持たせない。
* 1枚選択も `MinimumSelectionCount = 1`、`MaximumSelectionCount = 1` として同じAPIを使う。

### 7.3 戦闘中選択の表示先

`CardTargetSelectionService` は一覧Overlayを直接探索しない。Battle側から注入された選択Presenterまたは選択ポートを使用する。

```csharp
public interface ICombatCardSelectionPort
{
    bool TryBegin(
        CardSelectionRequest request,
        Action<CardSelectionResult> onConfirmed,
        Action onCancelled = null);
}
```

Battle実装は`HandView`を選択モードへ切り替え、対象外を非表示にする。これにより、弾丸選択時に別の50枠一覧を開かない。

### 7.4 カード効果側

`eGunshot`、`eSuppressingShot`の単一選択要求は次の形へ移行する。

* 最小1、最大1
* 候補条件は弾丸定義に含まれること
* `CanExecute`は候補数1以上を同じ候補判定サービスで確認する
* 確定結果のトークンから弾丸カードを再解決する
* 解決時点でもカードが手札に存在し、候補条件を満たすことを確認してから消費する

将来の複数カード消費効果は、同じ要求へカード固有のmin／maxを渡せる。min／maxの値を本PLANで新たに決めることはせず、各カード効果またはその`EffectSO`の既定パラメータから渡す。

### 7.5 戦闘外強化

戦闘外の選択Presenterは、全所持カードを50枠へ表示し、`RewardChoice.CanApplyTo`などバックエンドから渡された条件で対象外をグレーアウトする。

レベルアップ／MOD付与の処理順は次とする。

1. 強化選択要求を開始する。
2. 対象外カードをグレーアウトする。
3. 対象カードをトグル選択する。
4. 選択数条件を満たした場合だけ一覧側OKを有効化する。
5. 一覧側OKで選択を閉じず、強化確認モーダルを開く。
6. モーダルNOで一覧へ戻り、選択状態を維持する。
7. モーダルOKでバックエンドが再検証し、強化を適用する。
8. 適用成功後に一覧とモーダルを閉じ、報酬解決を通知する。

既存報酬は1枚選択だが、共通APIは複数選択を許容する。複数対象へ同一報酬を適用するゲームルールは未決定のため、既存のレベルアップ／MOD報酬はmin 1／max 1のままとする。

## 8. View構成案

### 8.1 共通カード一覧Prefab

`CardListSelectionOverlay.prefab` は少なくとも次の階層を持つ。

```text
CardListSelectionOverlay
├─ Blocker
├─ Header
│  ├─ TitleText
│  └─ CloseButton
├─ Body
│  ├─ CardScrollView
│  │  └─ Viewport
│  │     └─ GridContent
│  │        ├─ CardListSlot_00
│  │        ├─ ...
│  │        └─ CardListSlot_49
│  └─ CardDetailPanel
│     ├─ CardPreview
│     ├─ LevelText
│     ├─ CostText
│     ├─ EffectText
│     └─ ModifierListText
└─ Footer
   ├─ SelectionCountText
   ├─ CancelButton
   └─ OkButton
```

* `GridLayoutGroup.constraint`はFixed Column Count、`constraintCount = 6`とする。
* 50枠はPrefabに保存し、配列参照をInspectorへ明示的に設定する。
* 閲覧モードでは選択数とOKを非表示にする。
* 戦闘外選択モードでは選択数、OK、必要に応じてキャンセルを表示する。
* 詳細パネルは一覧と同時に表示し、確認モーダルとして扱わない。
* 初期フォーカスは先頭の表示カードとする。カードが0枚なら詳細を空状態にする。

### 8.2 一覧スロット

一覧用の`CardListSlotView`を設ける。戦闘手札用、ショップ用、報酬用Prefabは流用しない。

スロットが持つ状態は以下とする。

* 未使用
* 通常
* フォーカス
* 選択中
* 選択不能（グレーアウト）

クリック時は`CardSelectionToken`だけを通知する。グレーアウト中のクリックは選択を変更しないが、右詳細へ内容と選択不能理由を表示してよい。

### 8.3 詳細パネル

詳細パネルは次を表示する。

* カード名
* 現在レベル
* 現在コスト
* 現在クールダウン（既存カード表示と整合を取るため併記候補。ADR必須項目ではないため、最終表示有無は実装時に既存UI仕様へ合わせる）
* 現在レベルの効果説明
* 付与済みMOD全件
* MODなしの場合の「なし」
* 戦闘外選択で対象外の場合、その理由

カード固有のIntrinsic Modifierを「MOD」欄へ含めるかはADR-017で明記されていない。現行UIの`BuildModifierSummary`は`EnchantDefinitions`のみを表示しているため、初期実装は付与済みEnchantのみをMOD欄へ列挙し、Intrinsic Modifierの表示追加は別途開発者判断とする。

### 8.4 強化確認モーダル

強化確認モーダルは共通一覧の子または共通モーダルPrefabとして事前配置する。前後カードを実行時生成しない。必要であれば事前配置した「現在」と「強化後」の2つのプレビュー枠へ表示モデルを割り当てる。

* 主文言: `強化します`
* 対象カード名
* レベルアップの場合は現在レベルと強化後レベル
* MOD付与の場合は付与するMOD名
* OKボタン
* NOボタン

### 8.5 戦闘手札選択

戦闘手札の事前配置と重なり／スクロールの実装はBattle手札PLANの担当範囲とする。本PLAN側から必要とする連携口は以下である。

```csharp
void EnterSelectionMode(IReadOnlyList<CardDisplayModel> cards, int min, int max);
void UpdateSelection(IReadOnlyCollection<CardSelectionToken> selected);
void ExitSelectionMode();
event Action<CardSelectionToken> CardSelectionToggled;
event Action ConfirmRequested;
event Action CancelRequested;
```

選択中は対象外カードを非表示とし、対象カードだけで既存の手札レイアウトを再計算する。選択終了後は、元の全手札表示と通常操作を復元する。

## 9. 具体的な実装手順

### フェーズA: 選択契約と表示モデル

1. `CardListMode`、`CardSelectionToken`、`CardDisplayModel`、`CardSelectionRequest`、`CardSelectionResult`を追加する。
2. min／max、候補数、重複トークン、確定時再検証を担当する選択ロジックを、Unity UIに依存しないクラスとして追加する。
3. `CardBase`から表示モデルを構築するFactoryまたはPresenterを追加する。
4. 現在コスト算出に必要なPlayer文脈を呼び出し側から渡す。
5. 無効要求と候補不足について、日本語の検証結果を返せるようにする。

### フェーズB: 共通カード一覧Viewの事前構築

1. `CardListSelectionOverlay.prefab`を、一覧、右詳細、フッター、確認モーダルがEditor上で見える構成へ更新する。
2. 一覧用`CardListSlotView` Prefabを作成する。
3. 50個のスロットを`GridContent`へ事前配置し、6列の`GridLayoutGroup`を設定する。
4. 50個のスロット参照を一覧ViewへInspector設定する。
5. `CardDetailPanelView`を追加し、現在値とMOD一覧を表示できるようにする。
6. 強化確認用プレビュー枠とOK／NOを事前配置する。
7. `CardListSelectionOverlay`から`Instantiate`、`Destroy`、実行時`AddComponent`、reflectionによるPrefab取得を除去する。
8. PrefabをOutpostSceneとBattleSceneの双方へ配置し、各シーンのController／Presenter参照をInspector設定する。

### フェーズC: 閲覧モード

1. OutpostとBattleのデッキ確認ボタンから閲覧要求を出す。
2. 最大50枚を元リスト順で表示する。
3. 先頭カードを初期詳細として表示する。
4. クリックごとに右詳細を更新する。
5. 閲覧中はカード状態を変更する操作とOKボタンを表示しない。
6. Battleで別の排他的選択が進行中の場合は、閲覧要求を拒否または閲覧ボタンを無効化する。

### フェーズD: 戦闘外選択と強化確認

1. 戦闘外選択用Presenterを追加する。
2. 全所持カードを表示し、候補外スロットをグレーアウトする。
3. トグル選択、min／max、選択数表示、OK活性を実装する。
4. レベルアップ／MOD報酬から、min 1／max 1の選択要求を開始する。
5. 一覧OK後に強化確認モーダルを開く。
6. NO時は未適用で一覧へ戻す。
7. OK時に`RewardController`側で対象と`CanApplyTo`を再検証してから適用する。
8. 現在の`ChooseDeckCardForReward(int, RewardChoice)`は、新しい選択結果を受ける入口へ移行する。既存呼び出しが残る間は薄い互換ラッパーに限定する。

### フェーズE: 戦闘中選択

1. `CardTargetSelectionService`からSingleton／シーン探索と一覧Overlay依存を除き、`ICombatCardSelectionPort`をコンストラクタ注入する。
2. BattleのPresenterから`HandView`へ選択モードを通知する。
3. 対象外カードを非表示にし、対象カードだけを再レイアウトする。
4. 対象カードのトグル、選択数、OK活性を実装する。
5. 選択中は通常のドラッグ／カードプレイ入力を停止する。
6. 確定またはキャンセル後に、全カード表示と通常入力を復元する。
7. `eGunshot`と`eSuppressingShot`をmin 1／max 1の要求へ移行する。
8. 両Effectの`CanExecute`と実行開始が同じ候補条件を利用するよう統一する。
9. 将来の複数選択Effectがmin／maxを渡せることを、テスト用の要求で検証する。ゲーム用カードの新規仕様は追加しない。

### フェーズF: 検証と旧処理撤去

1. 50枠、重複参照、null参照、6列設定を検証するEditor検証を追加する。
2. 旧`selectedDeckIndex`、`currentChoice`中心のUI状態を撤去する。
3. 一覧と確認モーダルに残る実行時生成・破棄処理を撤去する。
4. `CardView.LateUpdate`依存を見直し、一覧スロットはイベントまたは明示的Bindで更新する。
5. Viewから`GameController`を探索する経路を、Inspector参照またはPresenterからの設定へ置き換える。
6. OutpostとBattleの両シーンで、Prefab参照と初期非表示状態を検証する。

## 10. 他PLANとの依存関係

### 10.1 先行依存

* 共通テーマ／基準解像度PLAN
  * 色、フォント、羊皮紙パネル、ボタン状態の見た目を利用する。
* 共通モーダル／入力排他方針
  * 強化確認と、他Overlayとの同時表示防止に必要となる。

### 10.2 後続または連携依存

* Outpost UI PLAN
  * デッキ確認ボタンと、将来の戦闘外選択開始口を接続する。
* Battle HUD／手札PLAN
  * 50枠の戦闘手札、20枚超の横スクロール、選択時フィルタ、通常入力停止を担当する。
* 戦闘後／報酬PLAN
  * レベルアップ／MOD報酬から戦闘外選択を開始し、確認後の解決を受け取る。
* 旧UI撤去PLAN
  * 全呼び出し移行後に、旧生成コードと探索フォールバックを削除する。

## 11. 並列作業可否

### 11.1 並列可能

* 表示モデルと選択検証ロジックの実装
* 共通一覧Prefabと一覧スロットの制作
* 詳細パネルと強化確認モーダルの制作
* EditModeテストの作成

これらは、先に`CardDisplayModel`と選択イベントの契約を固定すれば並列化できる。

### 11.2 同一ファイル競合のため直列推奨

* `CardListSelectionOverlay.cs`の置換と、Prefabのserialized参照更新
* `HandView.cs`の手札事前配置対応と、戦闘選択モード対応
* `BattleUI.cs`のデッキ閲覧接続と、報酬選択接続
* `CardTargetSelectionService.cs`のAPI変更と、`GameController.cs`の注入更新
* `RewardController.cs`の選択結果API変更と、報酬画面側の呼び出し更新

担当エージェントを分ける場合、同じUnity PrefabまたはSceneを複数エージェントが同時編集しない。

## 12. 受入条件

### 12.1 一覧と詳細

* Unity EditorのPrefab Modeで、50個の一覧スロット、6列グリッド、右詳細、フッター、確認モーダルを確認できる。
* 0～50枚のカードを、実行時`Instantiate`／`Destroy`なしで表示できる。
* 7枚目が2行目、49～50枚目が9行目に表示され、縦スクロールで到達できる。
* 未使用スロットは非表示で、レイアウト上の空枠として残らない。
* カードクリックで右詳細が更新される。
* 詳細へ現在コスト、現在レベル、効果、付与済みMOD全件が表示される。
* 同名カードが複数あっても、クリックした実体に対応する詳細と選択状態が表示される。

### 12.2 閲覧モード

* OutpostとBattleの両方から一覧を開ける。
* 閲覧中にカードのレベル、MOD、所持状態が変化しない。
* 閲覧モードでは選択確定用OKを表示しない。
* 排他的なカード選択中に、別の閲覧または選択を重ねて開始できない。

### 12.3 戦闘外選択

* 所持カードをすべて表示する。
* 条件外カードは非表示にせず、グレーアウトして選択できない。
* min／maxの範囲内で複数カードをトグル選択できる。
* 同じカードを二重選択できない。
* 選択数が `選択数 / 最大数` で更新される。
* min未満ではOKが無効、max到達後は追加選択不可、解除は可能である。

### 12.4 戦闘中選択

* 弾丸選択時に別一覧を開かず、手札上で候補カードだけが表示される。
* 選択中は通常のカードプレイとドラッグを行えない。
* min／maxを満たした時だけOKで確定できる。
* 確定後は選択したカードだけが効果解決へ渡される。
* キャンセル可能な要求では、キャンセル後に通常手札が復元され、カード状態が変化しない。
* 候補がmin未満の場合、起点カードは使用不能であり、選択UIは開かない。

### 12.5 強化確認

* レベルアップ／MOD付与の対象選択後に「強化します」のOK／NOモーダルが表示される。
* NOでは一覧へ戻り、カード状態と報酬状態が変化しない。
* OKでは確定直前の再検証後に1回だけ強化が適用される。
* 適用不能になっていた場合は強化せず、日本語メッセージを表示して安全に選択を終了または再選択させる。

### 12.6 アーキテクチャ

* Viewが`RunState`、カードレベル、MOD、報酬を直接変更しない。
* 一覧Viewと選択Serviceが`FindAnyObjectByType`やreflectionで通常参照を解決しない。
* 一覧、詳細、確認モーダルでUIオブジェクトを実行時生成・破棄しない。
* ショップ、報酬選択肢、戦闘手札、共通一覧は別View／Prefabのままである。

## 13. テスト計画

### 13.1 EditModeテスト

選択ロジックをUnity UIから分離し、以下をテストする。

* min 0／max 0の閲覧相当要求
* min 1／max 1の単一選択
* min 2／max 3の複数選択
* min未満では確定不可
* max超過を拒否
* 選択済みトークンの再トグルで解除
* 同じトークンを重複結果へ含めない
* 候補数がmin未満なら要求開始不可
* nullカードを候補として数えない
* 同じ定義の別カードインスタンスを別トークンとして扱う
* 確定前にカードが除去された場合、再検証で拒否
* 確定前に候補条件が変わった場合、再検証で拒否
* レベル3カードをレベルアップ候補外にする
* MOD報酬の適用可否を`RewardChoice.CanApplyTo`と一致させる

### 13.2 Editor／Prefab検証

* 一覧スロット参照がちょうど50件ある。
* null参照と同一スロットの重複登録がない。
* スロットが`GridContent`配下にある。
* GridLayoutGroupが6列固定である。
* 詳細欄、選択数、OK、キャンセル、確認モーダルの必須参照が設定されている。
* 一覧用スロットに戦闘用ドラッグ処理が付いていない。

### 13.3 PlayModeテスト

* 0、1、6、7、49、50枚の表示と未使用枠非表示
* 50枚時の最下段までのスクロール
* 閲覧時の詳細切替
* 複数MODの全件表示
* 戦闘外のグレーアウトと選択不能
* min 2／max 3の選択数、OK活性、max制限
* 戦闘中の候補外非表示と終了後復元
* 選択中の通常カードプレイ停止
* 弾丸カード0枚時にGunshot／SuppressingShotを使用できない
* 弾丸カード1枚以上で選択し、確定後に意図した1枚だけを消費
* レベルアップ／MOD付与でNOなら未適用、OKなら1回だけ適用
* OutpostとBattle双方からの閲覧

### 13.4 手動確認

* 1920×1080と1280×720で、6列と右詳細が重ならない。
* 通常文字が羊皮紙系パネル上で読み取れる。
* グレーアウト、フォーカス、選択中の差が色だけに依存せず判別できる。
* 長いカード名、長い効果文、MOD複数件でレイアウトが破綻しない。
* マウス操作でスクロール、選択、解除、詳細確認が迷わず行える。

## 14. 想定対象ファイル

実装時の候補であり、既存構成の確認後に最小限へ絞る。

### 14.1 更新候補

* `Assets/Code/Runtime/UIs/CardListSelectionOverlay.cs`
* `Assets/Code/Runtime/Controller/Services/CardTargetSelectionService.cs`
* `Assets/Code/Runtime/Cards/Views/HandView.cs`
* `Assets/Code/Runtime/Cards/Views/CardView.cs`
* `Assets/Code/Runtime/Controller/BattleUI.cs`
* `Assets/Code/Runtime/Controller/GameController.cs`
* `Assets/Code/Runtime/Controller/RewardController.cs`
* `Assets/Code/Runtime/Actions/Effects/eGunshot.cs`
* `Assets/Code/Runtime/Actions/Effects/eSuppressingShot.cs`
* `Assets/GameData/Prefabs/CardListSelectionOverlay.prefab`
* `Assets/GameData/Scenes/OutpostScene.unity`
* `Assets/GameData/Scenes/BattleScene.unity`

### 14.2 新規候補

* `Assets/Code/Runtime/UIs/Cards/CardListMode.cs`
* `Assets/Code/Runtime/UIs/Cards/CardSelectionToken.cs`
* `Assets/Code/Runtime/UIs/Cards/CardDisplayModel.cs`
* `Assets/Code/Runtime/UIs/Cards/CardSelectionRequest.cs`
* `Assets/Code/Runtime/UIs/Cards/CardSelectionResult.cs`
* `Assets/Code/Runtime/UIs/Cards/CardSelectionValidator.cs`
* `Assets/Code/Runtime/UIs/Cards/CardListPresenter.cs`
* `Assets/Code/Runtime/UIs/Cards/CardListSlotView.cs`
* `Assets/Code/Runtime/UIs/Cards/CardDetailPanelView.cs`
* `Assets/Code/Runtime/UIs/Cards/CardUpgradeConfirmView.cs`
* `Assets/Code/Runtime/UIs/Cards/ICombatCardSelectionPort.cs`
* `Assets/Code/Editor/UI/CardListSelectionOverlayValidator.cs`
* `Assets/GameData/Prefabs/UI/Cards/CardListSlot.prefab`
* テスト用asmdefと、上記ロジック／Viewに対応するEditMode・PlayModeテスト

## 15. 実装開始前に固定する境界

複数エージェントで並列実装する場合、最初に次だけを共有契約として固定する。

1. 選択トークンは要求元リストのインデックスを用い、確定時にカード実体を再検証する。
2. 閲覧／戦闘外選択は共通一覧、戦闘中選択は手札Viewを使用する。
3. 単一選択も複数選択APIのmin 1／max 1として扱う。
4. Viewへ渡すのは表示専用モデルと選択トークンであり、Viewからカード状態を変更しない。
5. 一覧50枠はPrefabへ事前配置し、実行時生成・破棄をしない。
6. 既存のレベルアップ／MOD報酬は単一対象のままにし、複数対象強化ルールを追加しない。
7. Intrinsic Modifierを詳細のMOD欄へ含めるかは未決定事項として勝手に変更しない。

