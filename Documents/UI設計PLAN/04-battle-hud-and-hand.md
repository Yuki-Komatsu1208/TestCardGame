# PLAN 04: Battle常設HUDと戦闘手札

## 目的

ADR-017に従い、`BattleScene` の常設HUDと最大50枚の戦闘手札をUnity Editor上で確認できる事前構築UIへ移行する。表示と入力だけをViewへ、戦闘状態の判断と表示モデル生成をPresenter／Controller側へ置き、現行の戦闘ルールは変更しない。

本PLANは実装手順を定めるものであり、レイアウト寸法や装飾素材の最終値は実画面確認後に開発者が決定する。

## 現状

- `BattleScene` の `CanvasScaler` は `800 x 600` 基準で、ADR-017の `1920 x 1080` と一致していない。
- `BattleUI` はHP、スタン耐性、状態異常、ターン、ステージ、ゴールド等を1個の `statusText` に連結している。敵単位の表示部品やゲージはない。
- `BattleUI.Initialize` は参照不足時に `GameObject`、`TextMeshProUGUI`、`Button` 等を実行時生成するフォールバックを持つ。報酬・終了・追加ボタンも一部動的生成する。
- `BattleUI` 自身が `GameController`、`RunController`、`RewardController` を探索し、表示文字列の構築、操作可否判定、イベント購読、報酬・Run遷移UIまで担っている。
- `HandView.ShowCards` は表示のたびに既存の子を破棄し、`CardView` を枚数分 `Instantiate` する。ADR-017の「50枠を事前配置」と一致しない。
- `HandView` はカード幅を合算して単純横並びにするため、重なり、20枚超の横スクロール、ホバー／選択カードの前面化・拡大に未対応。
- `CardView` は `LateUpdate` ごとに表示を更新し、その中で `FindAnyObjectByType<GameController>()` とドラッグ設定も繰り返す。
- `GameController.RefreshHandView` も毎回 `FindAnyObjectByType<HandView>()` を使用している。
- `BattleScene` には `BattleUIPanel`、`StatusText`、`HandPanel`、`EndTurnButton`、`CheckDeckButton` 等が存在するが、完成HUDとしての構造とInspector参照は不足している。
- 現行のターン情報は `TurnService.IsPlayerTurn` の真偽のみで、表示用の通算ターン番号は公開されていない。
- 戦闘ログの画面上部表示は別の共通UI計画に依存する。本PLANではログ領域を受け入れられる配置だけを確保し、ログ収集基盤は重複実装しない。

## 対象

- `BattleScene` の1920 x 1080基準レイアウト。
- プレイヤーHP、スタンゲージ、状態異常、マナ、ターン、遠征・ステージ、OverHunt深度・保留ゴールドの常設表示。
- 敵ごとのHP、スタンゲージ、状態異常を表示するHUD枠。最大同時出現数は既存ステージ定義を調査して必要数を事前配置し、不足時は明示的エラーとする。
- `ターン終了`、`デッキ確認`、右上設定ボタンへの配置導線。
- 最大50枚の事前配置手札スロット。
- 通常表示時の重なり、ポインターフォーカス／選択時の拡大と前面化。
- 既定20枚を超えた場合の横スクロール。閾値、重なり量、拡大率等はInspectorまたは表示設定アセットから調整可能にする。
- 既存のカード使用可否、クールダウン、カードドラッグ処理との接続。
- 参照不足と50枠不足を検出するEditor検証。

## 対象外

- カード一覧・詳細パネル・戦闘中複数選択の実装（共通カード一覧PLANの担当）。
- 盤面の自動フィット、赤枠、ドラッグ中の範囲計算（PLAN 05）。
- 敵の次行動アイコンと予告計算（PLAN 06）。
- 報酬、OverHunt選択、死亡画面の置換。
- ログメッセージの収集・配送基盤。上部ログ領域の最終実装は共通UI PLANへ従う。
- 戦闘ルール、最大所持カード数、デッキ編成ルールの変更。
- 実画面確認前の最終的なピクセル寸法・色・アニメーション時間の確定。

## 責務分離案

### 表示モデル

実際の型名は実装開始時に既存の共通表示モデル方針と合わせる。最低限、以下の読み取り専用データを用意する。

```csharp
public sealed class BattleHudViewModel
{
    public UnitHudViewModel Player { get; }
    public IReadOnlyList<UnitHudViewModel> Enemies { get; }
    public string TurnLabel { get; }
    public string ExpeditionLabel { get; }
    public string StageLabel { get; }
    public bool IsOverHunt { get; }
    public int OverHuntDepth { get; }
    public int PendingGold { get; }
    public bool CanEndTurn { get; }
    public bool CanOpenDeck { get; }
}

public sealed class UnitHudViewModel
{
    public UnitID UnitId { get; }
    public string Name { get; }
    public int CurrentHp { get; }
    public int MaxHp { get; }
    public int CurrentStun { get; }
    public int MaxStun { get; }
    public IReadOnlyList<StatusIconViewModel> Statuses { get; }
    public bool IsDefeated { get; }
}
```

- Presenterは `GameController`／`RunController` の状態を読み、ViewModelを組み立てる。
- Viewは数値、テキスト、ゲージ、色、表示／非表示を反映し、ゲーム状態を直接変更しない。
- `BattleHudView` は `EndTurnRequested` と `DeckRequested` のような入力イベントだけを公開する。
- `BattleUI` は段階的移行中の調停役に留め、最終的にHUD Viewへ表示モデルを渡す。報酬等の別責務は各専用Presenterへ移す計画と整合させる。
- OverHuntの赤強調は `IsOverHunt` を受け取ったViewが専用テキストへ適用する。通常HUD全体の色は変えない。

### 手札View API案

```csharp
public interface IBattleHandView
{
    event Action<int> CardPlayRequested;
    event Action<int> CardFocused;
    void Bind(IReadOnlyList<BattleHandCardViewModel> cards);
    void SetInteractionEnabled(bool enabled);
    void SetSelectionFilter(IReadOnlySet<int> visibleCardIndices);
    void ClearSelectionFilter();
}
```

- 50個の `BattleHandCardSlotView` をInspectorで保持し、`Bind` は先頭からデータを割り当てる。未使用枠は非表示にする。
- スロット識別にはカード名ではなく、バインド時の安定したカードインデックスまたはカードインスタンスIDを使う。
- Viewは `CardBase` を変更しない。既存移行中に `CardBase` を表示用に読む場合も、操作要求はID／インデックスだけを通知する。
- `CardView` の常時 `LateUpdate` は廃止し、カード状態変更またはHUD更新時に明示的に再バインドする。
- 戦闘中選択フィルターは共通カード一覧とは別物として、この50枠の表示／選択状態だけを切り替える。

### レイアウト設定案

`BattleHandLayoutSettingsSO` または `HandView` のSerializeFieldとして、少なくとも次を調整可能にする。

- `scrollThreshold`（初期値20）
- `maxSlots`（検証値50。ゲームルール値として変更しない）
- `cardOverlap`／`minimumVisibleWidth`
- `focusedScale`、`focusedYOffset`
- `sidePadding`、`bottomPadding`
- `scrollSensitivity`

SO化するかPrefab内SerializeFieldに留めるかは、共通テーマ／UI設定PLANの決定を優先する。

## 具体的な実装手順

1. `BattleScene` の現行参照、同時出現敵数、HUD更新契機、カード状態更新契機を一覧化し、別PLANの変更箇所と競合しないScene階層を確定する。
2. `CanvasScaler` をADR-017の基準解像度へ合わせ、上部HUD、ログ予約領域、中央盤面領域、下部手札領域、右側操作領域をScene上に事前配置する。
3. プレイヤーHUD Viewと敵HUD Slot ViewをPrefabまたはScene上に作る。HP／スタンは数値とゲージの双方、状態異常は事前配置アイコン枠を使用する。
4. 敵HUD枠数について既存ステージの最大同時出現数と召喚上限を確認する。有限上限が仕様化されていなければ、勝手に上限を確定せず開発者へ確認し、本PLANの実装をその箇所で止める。
5. `BattleHudViewModel` とPresenterを追加し、`BattleUI.Refresh` 内の文字列連結を表示モデル生成へ移す。既存 `RunStateChanged`、ステージ開始、カード解決、ターン終了後の更新からPresenterを呼ぶ。
6. `BattleHudView` はInspector参照だけを使って表示を更新し、`FindAnyObjectByType`、`GameObject.Find`、実行時 `AddComponent` を通常経路から除く。
7. 戦闘手札専用Prefabをカード一覧・ショップ・報酬とは別に作成し、`HandPanel/Viewport/Content` 配下へ50枠を事前配置する。
8. `HandView.ShowCards` の生成・破棄方式を、既存50枠へのBind／非表示方式へ置換する。50枚超は状態不整合として日本語エラーを出し、黙って切り捨てない。
9. 1～20枚ではContentを表示幅内に収め、カードを均等に少し重ねる。21～50枚ではContent幅を広げて横スクロールを有効にする。閾値はInspector／設定値で変更可能にする。
10. `BattleHandCardSlotView` にPointer Enter／Exit、選択、ドラッグ開始／終了を接続し、フォーカス枠を拡大・上方向移動・最前面化する。フォーカス解除時は元のSibling順と位置へ戻す。
11. `MoveCardDragHandler` はScene探索と位置の自己管理を減らし、手札ViewまたはPresenterから必要なController参照とカードIDを一度だけ設定する。カード使用成否とフィルター中の入力可否をPresenter側で判断する。
12. `GameController.RefreshHandView` のScene探索をInspector参照またはPresenter参照へ置換する。初期化、カードプレイ完了、遅延カード処理解決、クールダウン更新で明示更新する。
13. `BattleUI.Initialize` のHUD動的生成フォールバックを削除し、必須参照不足は日本語エラー＋初期化失敗として扱う。報酬／終了UIの動的生成削除は、それらの置換PLAN完了時に行う。
14. 50枠、必須テキスト／ゲージ、ボタン参照、ScrollRect設定をEditModeで検証するEditor検証を追加する。
15. 1280 x 720、1920 x 1080、2560 x 1440でGame View確認を行い、20枚前後の閾値は開発者の画面確認結果に応じて設定値だけ調整する。

## 依存関係

- ADR-017および共通テーマ／基準解像度PLANに依存する。
- 上部ログ領域と設定ボタンは共通UI PLANのPrefab／APIに依存する。
- デッキ確認ボタンの遷移先は共通カード一覧PLANに依存する。
- 戦闘中の対象カードだけを残すフィルターは共通選択フローと連携するが、表示先は本PLANの手札Viewである。
- 盤面表示領域の最終RectはPLAN 05と共同で固定する。
- 敵頭上の意図アイコンを置くスペースはPLAN 06と共有する。
- 報酬・死亡・OverHuntパネルは別PLANで置換されるため、本PLANで `BattleUI` 全体を削除しない。

## 並列実装の可否

- HUD ViewModel／Presenterと手札Viewは、同じ `BattleScene` を直接編集しなければ並列化可能。
- Scene／Prefab担当は一人に限定する。複数エージェントが `BattleScene.unity` を同時編集しない。
- PLAN 05は盤面領域と `MoveCardDragHandler` を触るため、API合意後に並列化し、`MoveCardDragHandler.cs` の編集担当を一方へ固定する。
- PLAN 06は敵モデル／予告Presenterを先行実装できるが、`UnitView`／敵HUD Prefabへのアイコン接続は本PLANのScene構造確定後に行う。
- 共通カード一覧PLANとカード表示モデルの命名・カード識別方式を先に合意する。

## 受入条件

- Edit Modeの `BattleScene` でHUD、操作ボタン、手札領域、50個の手札スロットを確認できる。
- HUD構築のための `new GameObject`、`AddComponent`、座標・色指定が実行時コードに残っていない。
- プレイヤーと生存敵のHP、スタン、状態異常、ターン、遠征、ステージ、OverHunt深度・保留ゴールドが正しい専用欄へ表示される。
- OverHunt中は該当テキストだけが赤系で強調される。
- 0、1、20、21、50枚で例外なく表示でき、未使用枠が非表示になる。
- 20枚以下は重なり表示、閾値超過時は横スクロールになり、カード名または識別に必要な部分が完全に隠れない。
- ポインターを合わせた／選択したカードが拡大・前面化し、解除後に正しいレイアウトへ戻る。
- 50枚超または参照不足は日本語の明示的エラーになり、実行時生成へフォールバックしない。
- 敵行動中など入力禁止状態ではカード使用とターン終了ができず、閲覧可否はADR-017の許容範囲に従う。
- 1280 x 720から2560 x 1440まで、HUD、盤面予約領域、手札が互いに致命的に重ならない。

## テスト計画

### EditMode

- 50個の手札スロットと必須Inspector参照が設定されていること。
- `BattleHudViewModel` のHP／スタン比率、状態異常、OverHunt表示値の変換テスト。
- 0、20、21、50枚に対するスクロール切替判定とContent幅の計算テスト。
- 51枚入力時に検証エラーとなること。
- 敵HUDの未使用スロットが非表示となり、撃破済み表示が指定どおりになること。

### PlayMode

- ステージ開始、カード使用、ターン終了、敵被弾、状態異常付与・終了、敵撃破ごとにHUDが更新されること。
- カードのクールダウン／コスト表示が変更契機で更新され、毎フレーム探索に依存しないこと。
- フォーカス中のカードが最前面になり、ドラッグ終了後にレイアウトへ戻ること。
- 20枚と21枚の境界でスクロール状態が切り替わること。
- デッキ確認や排他的モーダル表示中に戦闘入力が二重発火しないこと。

### 手動確認

- 3解像度で文字欠け、ゲージ欠け、カードの過剰な重なりを確認する。
- 開発者が20枚前後の視認性を確認し、`scrollThreshold` と重なり量を確定する。
- ダークファンタジー基調の背景上でも、羊皮紙系パネル上の黒文字が読めることを確認する。

## 想定対象ファイル

### 変更

- `Assets/GameData/Scenes/BattleScene.unity`
- `Assets/Code/Runtime/Controller/BattleUI.cs`
- `Assets/Code/Runtime/Controller/GameController.cs`
- `Assets/Code/Runtime/Cards/Views/HandView.cs`
- `Assets/Code/Runtime/Cards/Views/CardView.cs`
- `Assets/Code/Runtime/UIs/MoveCardDragHandler.cs`（PLAN 05との担当調整必須）

### 新規候補

- `Assets/Code/Runtime/UIs/Battle/BattleHudView.cs`
- `Assets/Code/Runtime/UIs/Battle/BattleHudPresenter.cs`
- `Assets/Code/Runtime/UIs/Battle/BattleHudViewModel.cs`
- `Assets/Code/Runtime/UIs/Battle/UnitHudSlotView.cs`
- `Assets/Code/Runtime/UIs/Battle/BattleHandCardSlotView.cs`
- `Assets/Code/Definitions/UIs/BattleHandLayoutSettingsSO.cs`（SO化を採用する場合）
- `Assets/GameData/Prefabs/UI/Battle/BattleHud.prefab`
- `Assets/GameData/Prefabs/UI/Battle/UnitHudSlot.prefab`
- `Assets/GameData/Prefabs/UI/Battle/BattleHandCardSlot.prefab`
- `Assets/GameData/UI/BattleHandLayoutSettings.asset`（SO化を採用する場合）
- `Assets/Code/Editor/UI/BattleSceneUIValidator.cs`
- `Assets/Code/Tests/EditMode/UI/BattleHudTests.cs`
- `Assets/Code/Tests/EditMode/UI/BattleHandLayoutTests.cs`
- `Assets/Code/Tests/PlayMode/UI/BattleHudPlayModeTests.cs`

ファイル名と分割単位は実装時に既存asmdefおよび共通UI PLANへ合わせる。ここにない戦闘ルールファイルへ変更が必要になった場合は、先に開発者へ範囲拡張を確認する。
