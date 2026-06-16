# アーキテクチャ決定記録 (Architectural Decision Record - ADR)

## ADR-001: グリッド制ターンバトルおよびカード効果システムの基盤設計

### ステータス
**承認済 (Accepted)**

### コンテキスト (背景)
本プロジェクトは、1人プレイ用PvEグリッド制ターンバトルのカードゲームです。
既存のコードベースには、以下のMVP（Model-View-Presenter）ライクな優れたクラス設計がすでに構築されていました：
* `Board` / `Cell` による盤面データ管理
* `UnitMoveService` / `ViewMoveService` によるユニットの移動処理とRectTransformベースの見た目同期
* `CardBase` / `CardView` / `HandView` による手札・カード情報の表示
* `GameController` による全体の統括

この既存のクリーンな設計思想（データモデルと表示ビューの明確な分離）を壊すことなく、以下の機能を追加する必要がありました：
1. ターン制システム（プレイヤーのターン ⇄ 敵のターン）の構築
2. 1ターンにつき最大1枚のカード使用制限
3. カード使用の「マナ」リソース制限（1ターンに1回復、最大5、初期3）
4. 各種カード効果（移動・攻撃・炎上効果）の実装
5. カードレベル（1〜3）による効果のスケーリング
6. 敵の最小限の行動AI（プレイヤーに1マス近づき、隣接時は攻撃）
7. マス（グリッド）そのものに対する特殊効果（炎上効果）
8. エディタに依存しない、初心者にも理解しやすいUI/オブジェクトの動的生成

---

### 決定事項 (Decisions)

#### 1. カードクラスの具体化とスケーリング
* `CardBase` を継承する具体的なカードクラスとして `cAttack` (攻撃) と `cIgnite` (炎上) を作成しました。
* `CardLevel` に応じた効果のスケーリング（ダメージ、炎上持続ターン、最大射程など）を `CardBase` および各カードクラスのコンストラクタおよび `LevelUp` / `LevelDown` オーバーライドメソッド内に定義しました。
* `eMove` (移動エフェクト) を、単なる固定距離移動から「カードをドロップしたマスに向かって、現在のカードレベル（最大ステップ数）の範囲内で進む」ように改良しました。

#### 2. リソース（マナ）とステータスの拡張
* `PlayerUnit` クラスに `Mana` / `MaxMana` プロパティを追加しました。
* マナはターン開始時に自動的に `1` 回復し、カード使用時にコスト（一律 `1`）を消費します。

#### 3. 状態管理（ターン制とカード使用制限）の GameController への統合
* `GameController.cs` が、ターンの進行状態（`IsPlayerTurn`）および「現在のターンにすでにカードを使用済みか（`hasPlayedCardThisTurn`）」のフラグを管理する状態マシンとしての役割を持つようにしました。
* 手札ドラッグを担当する `MoveCardDragHandler.cs` から `GameController.UseCardAtDropScreenPosition` を呼び出し、ターン状態やリソース状況（マナ・使用枚数）のバリデーションを1箇所で集中して行う設計にしました。

#### 4. 敵（エネミー）のクローン配置と簡易AI
* シーン構築を簡単にするため、`CellBuilder.cs` 内で既存のプレイヤー画像 (`playerView`) を複製 (Instantiate) して `EnemyView` を自動生成し、色を赤色に変更することで、追加のAssetアサイン手順を不要にしました。
* 敵のAIロジックは `GameController.ExecuteEnemyTurn` 内にカプセル化し、プレイヤーへ1マス近づく移動、または隣接時の 10 ダメージ攻撃を行うよう実装しました。

#### 5. グリッド効果「炎上」の Cell へのカプセル化
* `Cell.cs` 自体に炎上の持続ターン（`FireTurns`）とダメージ量（`FireDamage`）の状態を保持させ、ターン終了時の解決フェーズで `TickFire` を実行して、そのセルに留まっているキャラクターにダメージを与えるようにしました。
* `GameController.SyncCellVisuals` を通して、炎上中のセル（RectTransform）の Image のカラーをオレンジ色に、非炎上セルを白色に更新する「ビューの同期処理」を実装しました。

#### 6. Dynamic BattleUI の導入
* 新しいUI部品をインスペクターでアタッチする煩雑さを避けるため、ゲーム開始時に `BattleUI.cs` というMonoBehaviourコンポーネントをGameControllerオブジェクトに動的にアタッチし、キャンバスを探索して「ターン情報テキスト」「マナ・HPインジケータ」「ターン終了ボタン」を完全にプログラムコードから自動生成・配置・同期するアプローチを採用しました。

---

### 帰結 (Consequences)

#### メリット (Pros)
* **既存アーキテクチャの尊重**: モデル（データ）とビュー（表現）が綺麗に分離された元の美しい設計パターンをそのまま引き継いでいます。
* **拡張性の維持**: 新たなカードクラス（`cHeal`、`cGuard`等）や、新たなマス効果（毒沼、トラップ、回復ゾーン等）を追加する際も、今回確立した `CardBase` / `Cell` の拡張パターンをなぞるだけで容易に拡張可能です。
* **セットアップの自動化**: `BattleUI` や `EnemyView` を動的に生成するため、Unityのインスペクターでのアタッチ漏れ等による NullReferenceException の発生を最小限に防ぎ、初心者でも「Playボタンを押せばすぐに動く」状態を担保できました。

#### デメリット / 今後の課題 (Cons / Future Work)
* **GameControllerの肥大化**: ターン進行、敵AI、カード使用バリデーション、マスの見た目の同期など、多くの仲介処理が `GameController` に集中しつつあります。今後さらに仕様が肥大化する場合は、ターン進行を担当する `TurnManager` などの個別サービスにロジックを切り出す（リファクタリングする）必要があります。
* **マスのビジュアル表現**: 現在、炎上マスはセルの背景色を「オレンジ色」に変えることで表現しています。将来的に複数の効果が重なる場合や、より豊かなゲーム体験を求める場合は、セル上にパーティクルや専用の追加スプライトを重ねて表示するビュー側の拡張が必要になります。

---

## ADR-002: 敵行動の対象指定は当面 EnemyActionPlan に閉じ込める

### ステータス
**承認済 (Accepted)**

### コンテキスト (背景)
カードと敵行動の処理を共通化するため、行動の実体は `ActionEffect` に寄せました。
`ActionEffect` は `CanExecute(ActionContext)` と `Execute(ActionContext)` を持ち、プレイヤーのカードと敵の行動の両方から利用できます。

一方で、敵はプレイヤーと違い、入力から対象を選ぶのではなくAI側で対象を決めます。
そのため、敵には「どのActionを使うか」と「どこを対象にするか」を組み合わせる仕組みが必要です。

当初は `IEnemyTargetSelector` と `SelfPositionSelector` / `TargetUnitPositionSelector` のようなクラスを用意しましたが、現状の対象指定は「自分自身」または「ターゲットユニット」の2種類だけです。
この段階でインターフェースと複数クラスに分けると、実装量に対して抽象化が重くなります。

### 決定事項 (Decision)
当面は `EnemyActionPlan` が対象指定方法を内部に持ちます。
利用側は以下のstaticメソッドで簡潔にPlanを作成します。

```csharp
EnemyActionPlan.Self(new eIgniteAround(2, 5))
EnemyActionPlan.Target(new eMove(1))
```

`Self` は敵自身の座標を対象にします。
`Target` は `EnemyTurnContext.Target` の現在座標を対象にします。

新しい対象指定が少数であれば、`EnemyActionPlan` 内の `TargetKind` と `TryResolveTarget` に追加します。
例えば「ターゲットの1マス手前」「ランダムな隣接マス」などが1〜2種類増える程度なら、この方式を継続します。

### 将来の移行方針
対象指定の種類が増え、`EnemyActionPlan.TryResolveTarget` のswitchが大きくなってきた場合は、再びTargetSelector方式へ移行します。

その場合は、以下のように `EnemyActionPlan` が `ActionEffect` と `ITargetSelector` を受け取る設計にします。

```csharp
public class EnemyActionPlan
{
    public ActionEffect Action { get; }
    public ITargetSelector TargetSelector { get; }

    public EnemyActionPlan(ActionEffect action, ITargetSelector targetSelector)
    {
        Action = action;
        TargetSelector = targetSelector;
    }
}
```

ただし、使う側の記述が重くならないように、staticメソッドは残します。

```csharp
EnemyActionPlan.Self(new eSleep())
EnemyActionPlan.Target(new eMove(1))
EnemyActionPlan.RandomAdjacent(new eIgnite(2, 5))
```

staticメソッドの内部で適切な `ITargetSelector` を生成することで、敵クラス側は簡単に書ける状態を維持します。

### 帰結 (Consequences)

#### メリット (Pros)
* 現状の対象指定の少なさに対して、過度な抽象化を避けられます。
* `Slime` や `FireSlime` の行動定義が読みやすくなります。
* 新しいEnemyを追加するとき、`EnemyActionPlan.Self(...)` / `EnemyActionPlan.Target(...)` を使うだけで済みます。
* 対象指定が増えた場合の移行先も明確です。

#### デメリット / 今後の課題 (Cons / Future Work)
* `TargetKind` が増えすぎると `EnemyActionPlan` が肥大化します。
* 複雑な対象指定、再利用したい対象指定、テストしたい対象指定が増えた場合は、`ITargetSelector` への分離を検討します。
* 移行する場合でも、敵クラス側の記述を簡潔に保つため、staticファクトリメソッドは維持します。
