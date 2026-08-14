# PLAN 05: 盤面自動フィットと対象範囲表示

## 目的

ステージの `boardSize` に応じてUI盤面全体を所定の表示領域へ収め、カードドラッグ中や対象確認時に効果範囲を赤枠で明示する。現行の移動可能マス表示とセル効果表示を保ちつつ、盤面モデル、対象範囲計算、表示レイヤーを分離する。

本PLANではCameraを引く方式を採用せず、ADR-017の決定どおり `RectTransform` で構築された盤面を表示領域内へフィットさせる。

## 現状

- `CellBuilder` は `stageDef.boardSize` の数だけ `Cell.prefab` を実行時生成し、座標と `RectTransform` の辞書を作る。セルは可変盤面の反復要素であり、ADR-017が事前配置を要求する画面UIスロットとは役割が異なるが、生成責務とレイアウト責務が同じクラスに混在している。
- `CellBuilder` はプレイヤー／敵Viewも複製し、場合によっては `AddComponent<UnitView>` する。盤面フィットとは別課題だが、事前構築方針に反する箇所として後続整理が必要。
- `BattleScene` の `Board` にはGrid系コンポーネントがあるが、基準Canvasは800 x 600で、任意の `boardSize` を表示領域へ収める明示的計算はない。
- `BoardTargetingService` は画面座標に最も近いセルを返すだけで、画面外ドロップの拒否、カードの合法対象一覧、効果範囲の取得は扱わない。
- `BoardVisualService` は炎上セルをオレンジ、通常セルを白へ直接上書きする。移動可能、攻撃範囲、敵予告など複数の表示状態を合成する仕組みがない。
- `MoveCardDragHandler` はカード本体の位置を動かし、PointerUp時に即座にカード使用を要求する。ドラッグ開始時の縮小、候補セル変更時のプレビュー、終了時のクリアはない。
- 各 `ActionEffect` は `CanExecute` と `Execute` を持つが、副作用なしに影響セル集合を返す共通契約を持たない。範囲計算は `eAreaAttack`、`eLineAttack`、`eIgniteAround` 等の実行処理内部に分散している。
- `GameController.RefreshBattleViews` は死亡処理、セル状態同期、HUD更新をまとめて呼ぶが、「盤面状態が更新された」ことを外部Viewへ通知する汎用イベントはない。

## 対象

- `boardSize`、表示領域、セル間隔から正方形セルサイズと盤面Contentサイズを計算する自動フィット。
- 盤面の縦横中央配置と、プレイヤー／敵Viewのセル追従維持。
- 現行の移動可能マス表示を壊さない表示レイヤー設計。
- 攻撃、状態異常、その他の空間効果が持つ対象セルを赤枠で表示する共通API。
- カードドラッグ開始時のカード縮小、ドラッグ位置に対応する対象候補・影響範囲の更新、終了／キャンセル時の復元と赤枠クリア。
- 範囲を持たない効果では枠を表示しないこと。
- 敵意図表示（PLAN 06）から同じ赤枠レイヤーを利用できるようにすること。
- 盤面状態変更の通知と、範囲プレビュー再計算の更新契機。

## 対象外

- 盤面サイズそのもの、セル座標系、カードの射程・ダメージ・効果ルールの変更。
- Camera移動、ズーム、盤面スクロールの導入。自動フィット後も判別不能になる盤面サイズが正式仕様になった場合に別途判断する。
- 敵の次行動選択アルゴリズムとアイコン分類（PLAN 06）。
- 手札の50枠、重なり、横スクロール（PLAN 04）。
- 新しいカード効果や敵行動の追加。
- `CellBuilder` 全体の生成方式変更。セル数上限や敵View上限は本PLANで独断決定しない。

## 実装前に確認する境界

盤面サイズにはカード一覧の50枚のような正式な最大値がまだない。盤面セルまで固定上限数を事前配置するか、サイズ可変の盤面だけは完成済み `Cell` Prefabの実行時配置を許可するかを、このPLANへ着手する前に開発者が決定する。

* 固定配置を選ぶ場合は、最大盤面サイズを仕様化し、その数のセルスロットを事前配置して余剰を非表示にする。
* Prefab配置を例外許可する場合も、セル内部のImageやOverlayをコードで生成せず、完成済みPrefabを配置するだけに限定する。
* どちらの場合も盤面モデル、セル座標、表示サイズの判断はView生成処理と分離する。

## 責務分離／API案

### 盤面フィット

```csharp
public readonly struct BoardLayoutResult
{
    public float CellSize { get; }
    public Vector2 ContentSize { get; }
}

public static class BoardLayoutCalculator
{
    public static BoardLayoutResult Calculate(
        Vector2 viewportSize,
        Vector2Int boardSize,
        float spacing,
        Vector4 padding,
        float maximumCellSize);
}
```

- 計算はUnity Scene状態から分離してEditModeテスト可能にする。
- `BoardView` は計算結果を `GridLayoutGroup.cellSize` とContentの `sizeDelta` へ反映するだけとする。
- `CellBuilder` は盤面モデルとセル対応辞書を構築し、フィット値の判断をしない。
- 盤面表示領域のRectはPLAN 04のHUD／手札領域を除いた中央領域とする。

### 範囲問い合わせ

Actionの型名をUIが判定する方式は採らない。次のいずれかの明示契約をEffect側に追加する。

```csharp
public interface IActionPreviewProvider
{
    ActionPreviewResult GetPreview(ActionPreviewContext context);
}

public sealed class ActionPreviewResult
{
    public bool HasSpatialTarget { get; }
    public IReadOnlyList<Vector2Int> SelectableCells { get; }
    public IReadOnlyList<Vector2Int> AffectedCells { get; }
}
```

- `ActionPreviewContext` は盤面、使用者、現在の候補座標、状態異常等、計算に必要な読み取り専用情報を持つ。
- `GetPreview` はHP、位置、カード、行動ルーティン、乱数状態を変更しない。
- `SelectableCells` はカードを置ける候補、`AffectedCells` は選択中候補から実際に影響するセルとして区別する。
- プレビュー非対応／空間対象なしは空集合を返す。UI側でEffect具象型を `switch` しない。
- 実処理とプレビューの範囲計算が二重実装にならないよう、各Effectの純粋なセル列挙関数を `Execute` とPreviewから共用する。

### 表示レイヤー

```csharp
public enum CellOverlayKind
{
    MoveCandidate,
    PlayerActionRange,
    EnemyIntentRange
}

public interface IBoardOverlayView
{
    void Show(CellOverlayKind kind, IReadOnlyCollection<Vector2Int> cells);
    void Clear(CellOverlayKind kind);
    void ClearAllTransient();
}
```

- 基礎セル色／炎上等の永続表示と、移動候補／カード範囲／敵予告の一時枠を別Imageで表示する。
- 攻撃・状態異常・その他の範囲効果はADR-017どおり赤枠を使用する。移動可能表示は既存の視認性を維持する別レイヤーとする。
- プレイヤーカードのプレビューと敵予告が競合する場合、ドラッグ中のプレイヤープレビューを優先し、終了後に選択中敵の予告を復元する。優先順位はPresenterで管理する。

### ドラッグ入力

```csharp
public interface ICardDragPreviewController
{
    void BeginPreview(int cardIndex);
    void UpdatePreview(int cardIndex, Vector2 screenPosition);
    bool Commit(int cardIndex, Vector2 screenPosition);
    void Cancel(int cardIndex);
}
```

- `MoveCardDragHandler` はPointerイベントを通知し、対象計算やカード使用可否を判断しない。
- `BoardTargetingService` は最近傍だけでなく、ポインターが盤面Viewport内か、該当セルRect内かを判定するAPIを持つ。
- ドラッグカードの縮小率は手札表示設定から受け、ドラッグ終了・キャンセル・無効化時に必ず元へ戻す。

## 具体的な実装手順

1. 既存StageDefinitionの `boardSize` 最小・最大、セルPrefab寸法、GridLayout設定、HUD／手札が使う予約領域を調査し、表示領域Rectを確定する。正式な最大盤面サイズが未定なら上限は決めず、既存データ範囲でテストする。
2. 純粋な `BoardLayoutCalculator` を実装し、縦横それぞれの利用可能サイズから小さいセルサイズを採る。0以下の盤面、負の余白、極小Viewportを明示的に拒否する。
3. `BoardView` を追加し、ステージ初期化後とViewport寸法変更時にセルサイズ、間隔、Content寸法、中央配置を更新する。毎フレーム計算せず、初期化／解像度変更時だけ更新する。
4. `CellBuilder` から盤面レイアウト判断を切り離す。セル生成後に `BoardView.Bind(boardSize, cellRects)` を呼び、セル辞書の座標対応を維持する。
5. `Cell.prefab` に基礎表示、永続セル効果、移動候補枠、プレイヤー範囲枠、敵予告枠の事前配置Imageを用意する。実行時 `AddComponent` は使わない。
6. `BoardVisualService` の直接色上書きを、セルの永続状態表示へ限定する。複数Overlayが互いの色を消さないよう `CellView` または `BoardOverlayView` へ責務を移す。
7. ActionEffect群を分類せず、空間プレビューが必要な既存Effectごとに `IActionPreviewProvider` を実装する。対象セル列挙を純粋関数へ抽出し、`Execute` とPreviewで共有する。
8. 複数Effectを持つカードでは、各Effectの `SelectableCells` と `AffectedCells` の合成規則を既存カード対象ルールに合わせる。規則がコードから一意に判断できなければ勝手に共通部分／和集合を選ばず、開発者へ確認する。
9. `BoardTargetingService` に盤面内判定とセル特定APIを追加する。Viewport外を最近傍セルへ吸着して誤発動しないようにする。
10. `CardDragPreviewPresenter` を追加し、ドラッグ開始でカードを縮小、候補セル変更時にPreviewを再計算、PointerUpで既存 `UseCardAtDropScreenPosition` を呼び、成功・失敗に関係なくOverlayとカードTransformを復元する。
11. `MoveCardDragHandler` を薄い入力Viewに変更し、`FindAnyObjectByType` の通常経路とカードルール判断を除く。PLAN 04担当と編集責任を調整する。
12. `GameController` に盤面変更通知を追加する。最低限、カードプレイ完了、移動完了、召喚、死亡処理、状態異常変化、ターン遷移後に一度発火する。1操作中の中間状態で過剰発火しないよう、処理完了点へ集約する。
13. 敵選択による予告範囲表示の入口を `IBoardOverlayView` へ用意し、PLAN 06から `EnemyIntentRange` を渡せる状態にする。
14. 解像度3種と既存盤面サイズで、全セルがViewport内に収まること、入力座標とセル座標が一致することを確認する。

## 依存関係

- ADR-017と `StageDefinitionSO.boardSize` に依存する。
- 盤面Viewportの確定はPLAN 04のBattle常設レイアウトに依存する。
- カードの表示縮小・復元と手札レイアウト再計算はPLAN 04の手札View APIに依存する。
- 戦闘中カード選択モード中のドラッグ可否は共通カード選択PLANに依存する。
- 敵予告はPLAN 06が作る予告モデルを受け、本PLANのOverlayを再利用する。
- 各Effectの表示分類や説明文ではなく、空間範囲だけを扱う。意図種別はPLAN 06側の責務とする。

## 並列実装の可否

- `BoardLayoutCalculator` とテストはScene変更なしで先行・並列実装可能。
- Effectごとの純粋な範囲列挙は、担当Effectファイルを分割すれば並列可能。ただし共通インターフェースと合成規則を先に固定する。
- `BattleScene.unity`、`Cell.prefab`、`MoveCardDragHandler.cs` は競合しやすいため、各ファイルの編集担当を一人に固定する。
- PLAN 06は予告問い合わせモデルを並列実装可能だが、`IBoardOverlayView` の契約を先に共有する。
- PLAN 04の手札フォーカスと本PLANのドラッグ縮小が同じTransformへ作用するため、状態遷移表を合意してから結合する。

## 受入条件

- 既存の全StageDefinitionで盤面全体が指定Viewport内へ収まり、セルは正方形を維持する。
- 1280 x 720、1920 x 1080、2560 x 1440で盤面がHUDと手札へ致命的に重ならない。
- プレイヤー／敵Viewがフィット後の正しいセルへ追従する。
- 炎上等のセル状態、移動可能表示、プレイヤー範囲、敵予告範囲が別レイヤーとして共存し、一方の更新が他方を消さない。
- 空間効果カードをドラッグするとカードが縮小し、現在候補の影響セルが赤枠表示される。
- 盤面外、無効対象、キャンセル、カード使用失敗のいずれでもカード表示とOverlayが確実に元へ戻る。
- 空間対象を持たないカードでは赤枠が表示されない。
- プレビュー問い合わせによってHP、位置、状態異常、カード状態、敵ルーティン、乱数状態が変化しない。
- UIはEffectの具象型名から範囲を推測しない。
- カードプレイ完了を含む盤面変更後に、表示と選択中予告を再計算できるイベントが発火する。

## テスト計画

### EditMode

- 5 x 5、6 x 6、横長、縦長の盤面と複数Viewport寸法に対するセルサイズ／Content寸法計算。
- 余白・間隔込みで全セルがViewport内となる境界値テスト。
- 0以下の盤面サイズと極小Viewportのエラー処理。
- 各Preview対応Effectについて、既知の使用者座標・対象座標から期待するセル集合を返すこと。
- Preview前後でモデル状態が変わらないこと。
- Overlay優先順位とClear時に他レイヤーが維持されること。

### PlayMode

- ステージ切替時に盤面サイズが変わり、ユニットと入力判定が新しいセルへ一致すること。
- ドラッグ中に隣接セルへ移動したとき赤枠が更新されること。
- 盤面外ドロップでカードが使用されず、表示が復元されること。
- ドラッグ中にモーダル／ターン遷移で操作不能になった場合もPreviewが残らないこと。
- 移動、召喚、死亡、状態異常変化後に盤面変更通知が発火すること。

### 手動確認

- 赤枠が炎上色やユニット画像の上でも判別できること。
- 縮小したドラッグカードが対象セルを隠しすぎないこと。
- 最小想定解像度でセルが小さすぎる場合は数値を勝手に決めず、スクロール／ズームへ移行する判断材料を開発者へ提示する。

## 想定対象ファイル

### 変更

- `Assets/GameData/Scenes/BattleScene.unity`
- `Assets/GameData/Prefabs/Cell.prefab`
- `Assets/Code/Runtime/BoardManage/CellBuilder.cs`
- `Assets/Code/Runtime/Controller/GameController.cs`
- `Assets/Code/Runtime/Controller/Services/BoardTargetingService.cs`
- `Assets/Code/Runtime/Controller/Services/BoardVisualService.cs`
- `Assets/Code/Runtime/UIs/MoveCardDragHandler.cs`
- 空間プレビュー対象となる `Assets/Code/Runtime/Actions/Effects/eXxx.cs` 群

### 新規候補

- `Assets/Code/Runtime/UIs/Battle/BoardView.cs`
- `Assets/Code/Runtime/UIs/Battle/CellView.cs`
- `Assets/Code/Runtime/UIs/Battle/BoardOverlayView.cs`
- `Assets/Code/Runtime/UIs/Battle/CardDragPreviewPresenter.cs`
- `Assets/Code/Runtime/UIs/Battle/BoardLayoutCalculator.cs`
- `Assets/Code/Runtime/Actions/Preview/IActionPreviewProvider.cs`
- `Assets/Code/Runtime/Actions/Preview/ActionPreviewContext.cs`
- `Assets/Code/Runtime/Actions/Preview/ActionPreviewResult.cs`
- `Assets/Code/Tests/EditMode/UI/BoardLayoutCalculatorTests.cs`
- `Assets/Code/Tests/EditMode/Actions/ActionPreviewTests.cs`
- `Assets/Code/Tests/PlayMode/UI/BoardTargetPreviewPlayModeTests.cs`

ActionEffect共通契約の追加は戦闘ロジックへ広く影響するため、実装開始時にAPI案を開発者へ提示して承認を得る。既存効果の挙動を変更する修正は本PLANの範囲外とする。
