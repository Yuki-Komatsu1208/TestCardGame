# PLAN 06: 敵の次行動と対象範囲の予告

## 目的

各敵の頭上へ次行動アイコンを常時表示し、敵選択時には予告対象範囲を赤枠表示する。既存の行動ルーティンを維持したまま、次行動の種別、実行予定Effect、対象座標、影響範囲を副作用なしで問い合わせられるようにする。

予告は盤面状態が更新されるたびに再計算し、特にカードプレイ完了を必須トリガーとする。

## 現状

- `EnemyDefinitionSO` は `BehaviorPattern` と `EnemyActionPlanConfig` のリストを持ち、各Configから実行時 `ActionEffect` を生成する。
- `EnemyUnit` は `FirstExecutable` または `SequentialLoop` で行動する。`SequentialLoop` は `TryExecute` の試行前に `nextActionIndex` を進めるため、同じロジックを予告に流用すると予告だけでルーティンが進む危険がある。
- `FirstExecutable` は実行時点の `CanExecute` が真となる最初のPlanを選ぶ。
- `EnemyActionPlan.TryResolveTarget` はprivateで、Selfなら敵座標、Targetならプレイヤーの現在座標を返す。副作用なしの解決APIはない。
- `IEnemy` は `ExecuteTurn` しか公開せず、次行動問い合わせ契約を持たない。
- `ActionEffect` は表示分類を持たない。UIが `ePositionAttack` 等の型名を見て剣／渦巻／疑問符を推測するとADR-017に反する。
- 範囲Effectの影響セル計算は各 `Execute` 内部に分散しており、共通Preview APIはない。
- `CellBuilder` は敵ごとに `UnitView` を複製するが、頭上アイコン用の事前配置領域や敵クリック通知はない。
- `GameController` は移動完了イベントを公開するが、カードプレイ、召喚、死亡、状態異常変化、ターン遷移を統合した盤面状態更新イベントはない。
- 現在の敵ターンは同期的に全敵を順次実行するため、予告表示更新のタイミングを処理完了点へ明示する必要がある。

## 対象

- 攻撃＝剣、状態異常＝渦巻、その他＝疑問符の3分類。
- 行動表示分類を敵行動定義／バックエンドから明示的に渡す仕組み。
- 次に実行されるPlanと対象座標を、副作用なしで解決する問い合わせAPI。
- PLAN 05のAction Previewを使った影響セル集合の取得。
- 各敵の頭上に事前配置した意図アイコン領域。
- 敵クリックによる選択／解除と、選択敵の予告範囲表示。
- カードプレイ完了、移動、召喚、死亡、状態異常変化、ターン遷移での再計算。
- 行動不能、実行可能Planなし、撃破済み敵の表示状態。
- 実行直前の行動と直前予告が一致することの検証。

## 対象外

- 敵の行動順、ルーティン、対象選択、射程、ダメージ、状態異常ルールの変更。
- 新しい敵行動タイプや固有アイコンの追加。初期版はADR-017の3分類だけとする。
- プレイヤーに次行動を隠す敵、ランダム予告、フェイント等の新ゲーム仕様。
- 赤枠の描画実装そのもの（PLAN 05の `IBoardOverlayView` を使用）。
- 敵HUDのHP／スタン／状態異常（PLAN 04）。
- 意図アイコン画像の最終アート制作。必要なSprite差し替え枠までをUI実装対象とする。

## 責務分離／API案

### 定義側の表示分類

```csharp
public enum EnemyIntentKind
{
    Attack,
    StatusEffect,
    Other
}

[Serializable]
public struct EnemyActionPlanConfig
{
    public EnemyTargetKind targetKind;
    public EnemyIntentKind intentKind;
    public ActionEffectSO effect;
    public bool useCustomParameters;
    public ActionEffectParameters parameters;
}
```

- `intentKind` はEffect型から推測せず、敵行動のデータ定義で明示する。
- 既存Enemy assetへ値を移行する際、効果名だけで機械的に確定せず、実際の行動内容を確認して開発者レビューを通す。
- アイコンSpriteは共通UIテーマまたは `EnemyIntentIconSetSO` へ割り当て、敵assetごとに重複保持しない。

### 予告モデル

```csharp
public sealed class EnemyIntentPreview
{
    public UnitID EnemyId { get; }
    public EnemyIntentKind Kind { get; }
    public ActionEffect Action { get; }
    public Vector2Int TargetPosition { get; }
    public IReadOnlyList<Vector2Int> AffectedCells { get; }
    public bool CanExecute { get; }
    public string UnavailableReason { get; }
}
```

- Viewへは原則として `ActionEffect` 自体を渡さず、アイコン種別、対象座標、セル集合、利用可否だけを渡す。上記 `Action` はバックエンド内部モデルにだけ保持してよい。
- プレイヤー向けViewは内部型名やパラメータを表示しない。
- `UnavailableReason` はデバッグ／ログ用であり、初期版の頭上表示はアイコン非表示または疑問符表示のどちらかを仕様確認して決める。独断でプレイヤー情報を増やさない。

### 敵側の問い合わせ契約

```csharp
public interface IEnemy
{
    bool TryGetNextIntent(EnemyTurnContext context, out EnemyIntentPreview preview);
    void ExecuteTurn(EnemyTurnContext context);
}
```

- `EnemyActionPlan.TryPreview` は対象解決、`CanExecute`、影響範囲計算だけを行い、`Execute` を呼ばない。
- `SequentialLoop` ではローカル変数の候補indexを進めて検索し、`nextActionIndex` は変更しない。
- 実行時は予告結果をキャッシュして強制実行するのではなく、同じ選択関数を実行直前にも用いる。盤面変更後の古い予告が実行されないようにする。
- `FirstExecutable` と `SequentialLoop` の候補選択ロジックを純粋な共通関数へ寄せ、PreviewとExecuteの分岐差を防ぐ。
- 乱数を使う行動が将来追加された場合、乱数結果の予約仕様が別途必要になる。本PLANでは既存の決定的ルーティンだけを対象とし、乱数状態を消費しない。

### Presenter／View

```csharp
public interface IEnemyIntentView
{
    event Action<UnitID> EnemySelected;
    void Bind(UnitID enemyId, EnemyIntentKind kind, bool visible);
    void SetSelected(bool selected);
}

public sealed class EnemyIntentPresenter
{
    public void RefreshAll();
    public void SelectEnemy(UnitID? enemyId);
}
```

- Presenterは生存敵のPreviewを再計算し、UnitIDごとのViewへ渡す。
- ViewはSprite切替、表示／非表示、選択枠、クリック通知だけを担当する。
- 選択中の敵に空間範囲がある場合、PLAN 05の `IBoardOverlayView.Show(EnemyIntentRange, cells)` を呼ぶ。
- 非選択時は意図アイコンだけを表示し、赤枠は表示しない。
- カードドラッグ中はPLAN 05のプレイヤー範囲を優先し、ドラッグ終了後に選択敵の範囲を再表示する。

## 具体的な実装手順

1. 全 `EnemyDefinitionSO` assetの `behaviorPattern`、actionPlans、対象種別、Effectを一覧化し、攻撃／状態異常／その他の分類表を開発者レビュー用に作る。
2. `EnemyIntentKind` を定義し、`EnemyActionPlanConfig` へ明示フィールドを追加する。既存assetの分類値を設定し、未設定値をAttackへ誤フォールバックさせない。
3. `EnemyActionPlan` の対象解決を副作用なしの内部APIへ切り出し、実行とPreviewの双方から利用する。
4. `EnemyUnit` の候補Plan選択を、現在indexを引数に候補と「実行成功時の次index」を返す純粋ロジックへ切り出す。Preview時は戻り値を保存せず、Execute成功時だけ `nextActionIndex` を更新する。
5. `FirstExecutable`、`SequentialLoop`、行動不能、Planなし、全Plan実行不能の各ケースで `TryGetNextIntent` を実装する。
6. PLAN 05の `IActionPreviewProvider` を使用して対象セル集合を取得する。空間Preview非対応の行動はアイコンのみ表示し、赤枠は空集合とする。
7. `EnemyIntentPreview` からView専用モデルへ変換するPresenterを追加する。Presenterは `EnemyUnit` やEffectの内部型をUIへ漏らさない。
8. 敵View Prefab／テンプレートに頭上アイコンImage、クリック受付領域、選択枠を事前配置する。`CellBuilder` は用意済みコンポーネントを参照し、`AddComponent` で補わない。
9. 攻撃、状態異常、その他のSpriteセットを共通設定へ割り当てる。素材が未完成でも、Editorで3種を判別できる仮アイコンを置き、後からSprite差し替え可能にする。
10. 敵クリックで選択をトグルし、別の敵をクリックした場合は選択を移す。盤面背景クリックまたは撃破で選択解除する。
11. `GameController` または専用BattleState通知からPresenterへ更新を送る。カードプレイ完了は必須とし、移動完了、召喚完了、死亡反映後、状態異常変化完了、ターン遷移完了でも再計算する。
12. 一つの操作で複数イベントが出る場合、最終状態で一度更新する仕組みを用意する。中間状態の誤った予告が一瞬表示されないようにする。
13. 敵ターン開始直前に再問い合わせし、直前表示と実行候補が一致することを開発用検証ログまたはテストで確認する。不一致時にゲームルールを変えず、原因を日本語エラーとして報告する。
14. 敵死亡、敵召喚、ステージ切替、Run終了時にViewとOverlayのキャッシュを破棄する。

## 依存関係

- ADR-017およびEnemy ADR-001の「対象指定はEnemyActionPlanに閉じ込める」方針に依存する。
- 範囲取得と赤枠表示はPLAN 05のPreview契約と `IBoardOverlayView` に依存する。
- 敵頭上のView配置と同時表示数はPLAN 04の敵HUD／Battle Scene構造と調整する。
- `EnemyDefinitionSO` assetを変更するため、敵データを編集する他エージェントと同時作業しない。
- 状態異常変更イベントが不足する場合、`StatusEffectService` またはGameControllerの処理完了通知追加が必要となる。

## 並列実装の可否

- 行動選択の純粋化とEditModeテストは、View実装と並列可能。
- アイコンView／Presenterは、`EnemyIntentPreview` とOverlay API確定後に並列可能。
- `EnemyDefinitionSO.cs` とEnemy asset群は一人の担当へ集約する。複数エージェントによる同一asset編集を避ける。
- `EnemyActionPlan.cs` と `EnemyUnit.cs` は選択ロジックが密接なため同一担当が編集する。
- `BattleScene.unity`、敵View Prefab、`CellBuilder.cs` はPLAN 04／05と競合するため、Scene統合担当を一人に固定する。

## 受入条件

- 生存している各敵の頭上に次行動アイコンが常時1個表示される。
- 攻撃は剣、状態異常は渦巻、その他は疑問符となり、UIがEffect具象型から分類を推測しない。
- 非選択時はアイコンだけが表示され、敵選択時だけ空間対象範囲が赤枠表示される。
- 敵選択の切替、解除、撃破、ステージ終了で古い赤枠が残らない。
- Preview問い合わせを何回行っても `nextActionIndex`、HP、位置、状態異常、盤面、乱数状態が変化しない。
- `SequentialLoop` のPreviewを複数回呼んだ後でも、実際に実行される行動順が呼ぶ前と同じである。
- カードプレイ完了後に全生存敵の次行動と対象座標が再計算される。
- 移動、召喚、死亡、状態異常変化、ターン遷移後にも予告が更新される。
- 実行直前の盤面状態が変わっていない場合、表示された次行動と実際の実行行動が一致する。
- 空間範囲を持たない行動でもアイコンは表示でき、赤枠は表示しない。
- 敵が行動不能または実行可能Planなしの場合の表示が、実装前に開発者確認した方針どおりになる。

## テスト計画

### EditMode

- `Self` と `Target` の対象座標解決。
- `FirstExecutable` が最初の実行可能Planを選ぶこと。
- `SequentialLoop` が現在indexから最初の実行可能Planを選び、Previewではindexを変更しないこと。
- Execute成功時だけ次indexが進み、失敗Planの扱いが現行挙動と一致すること。
- Previewを1回／100回呼んだ場合で、その後の実行結果が同じこと。
- 行動不能、Planなし、全Plan不可の戻り値。
- `EnemyIntentKind` から表示アイコン種別への変換。
- Preview前後の敵・盤面状態スナップショットが一致すること。

### PlayMode

- 複数敵それぞれに正しいアイコンが表示されること。
- 敵A選択から敵B選択へ切り替えると、赤枠と選択枠がBへ移ること。
- カード使用、プレイヤー移動、敵移動、召喚、撃破、状態異常付与、ターン終了後にアイコン／範囲が更新されること。
- カードドラッグ中はプレイヤー範囲が優先され、終了後に敵予告範囲が復元されること。
- 敵ターンで予告どおりのPlanが実行されること。

### データ検証

- 全EnemyDefinition assetの全actionPlanに `intentKind` が設定済みであること。
- アイコンセットに3種のSpriteが設定されていること。
- 敵Viewに意図Image、クリック領域、UnitIDバインド先が設定されていること。

## 実装前に開発者確認が必要な点

ADR-017だけでは、以下の表示は確定していない。実装担当が独断で補完しない。

- 睡眠等で「次のターンに行動不能」と判定される敵の頭上を、アイコン非表示にするか疑問符にするか。
- 現在の盤面では全Planが実行不能だが、盤面変化後には実行可能になり得る敵の表示を、非表示にするか疑問符にするか。
- `eTelegraph` のようにメッセージだけを出す補助Effectを単独行動として「その他」にするか、次の実害行動を見せる設計にするか。後者は行動予告ではなく新しいゲーム仕様になり得る。

これらは実装開始時に現行敵assetの該当有無を提示し、該当する場合だけ開発者へ選択を求める。

## 想定対象ファイル

### 変更

- `Assets/Code/Definitions/Character/Enemies/EnemyDefinitionSO.cs`
- `Assets/Code/Runtime/Character/Enemies/IEnemy.cs`
- `Assets/Code/Runtime/Character/Enemies/EnemyActionPlan.cs`
- `Assets/Code/Runtime/Character/Enemies/EnemyTurnContext.cs`（Preview用読み取り情報が不足する場合）
- `Assets/Code/Runtime/Character/Enemies/Units/EnemyUnit.cs`
- `Assets/Code/Runtime/Controller/GameController.cs`
- `Assets/Code/Runtime/BoardManage/CellBuilder.cs`
- `Assets/Code/Runtime/Character/UnitView.cs` または敵専用View
- `Assets/GameData/Character/Enemies/*.asset`
- `Assets/GameData/Scenes/BattleScene.unity`

### 新規候補

- `Assets/Code/Runtime/Character/Enemies/EnemyIntentKind.cs`
- `Assets/Code/Runtime/Character/Enemies/EnemyIntentPreview.cs`
- `Assets/Code/Runtime/UIs/Battle/EnemyIntentPresenter.cs`
- `Assets/Code/Runtime/UIs/Battle/EnemyIntentView.cs`
- `Assets/Code/Definitions/UIs/EnemyIntentIconSetSO.cs`
- `Assets/GameData/UI/EnemyIntentIconSet.asset`
- `Assets/GameData/Prefabs/UI/Battle/EnemyIntentView.prefab`
- `Assets/UI/Art/Icons/EnemyIntent/` 配下の剣・渦巻・疑問符Sprite
- `Assets/Code/Editor/UI/EnemyIntentDataValidator.cs`
- `Assets/Code/Tests/EditMode/Enemies/EnemyIntentPreviewTests.cs`
- `Assets/Code/Tests/PlayMode/UI/EnemyIntentPlayModeTests.cs`

Action Preview関連ファイルはPLAN 05側を再利用し、類似の範囲計算APIを重複作成しない。
