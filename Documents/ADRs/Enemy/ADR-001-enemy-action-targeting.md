# ADR-001: 敵行動の対象指定は当面 EnemyActionPlan に閉じ込める

## ステータス
**承認済 (Accepted)**

## コンテキスト (背景)
カードと敵行動の処理を共通化するため、行動の実体は `ActionEffect` に寄せました。
`ActionEffect` は `CanExecute(ActionContext)` と `Execute(ActionContext)` を持ち、プレイヤーのカードと敵の行動の両方から利用できます。

一方で、敵はプレイヤーと違い、入力から対象を選ぶのではなくAI側で対象を決めます。
そのため、敵には「どのActionを使うか」と「どこを対象にするか」を組み合わせる仕組みが必要です。

当初は `IEnemyTargetSelector` と `SelfPositionSelector` / `TargetUnitPositionSelector` のようなクラスを用意しましたが、現状の対象指定は「自分自身」または「ターゲットユニット」の2種類だけです。
この段階でインターフェースと複数クラスに分けると、実装量に対して抽象化が重くなります。

## 決定事項 (Decision)
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

## 将来の移行方針
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

## 帰結 (Consequences)

### メリット (Pros)
* 現状の対象指定の少なさに対して、過度な抽象化を避けられます。
* `Slime` や `FireSlime` の行動定義が読みやすくなります。
* 新しいEnemyを追加するとき、`EnemyActionPlan.Self(...)` / `EnemyActionPlan.Target(...)` を使うだけで済みます。
* 対象指定が増えた場合の移行先も明確です。

### デメリット / 今後の課題 (Cons / Future Work)
* `TargetKind` が増えすぎると `EnemyActionPlan` が肥大化します。
* 複雑な対象指定、再利用したい対象指定、テストしたい対象指定が増えた場合は、`ITargetSelector` への分離を検討します。
* 移行する場合でも、敵クラス側の記述を簡潔に保つため、staticファクトリメソッドは維持します。
