using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Targeting
{
    /// <summary>
    /// 敵がActionを実行するときの対象座標を決めるインターフェース。
    /// </summary>
    public interface IEnemyTargetSelector
    {
        bool TrySelectTarget(EnemyTurnContext context, out Vector2Int targetPosition);
    }
}
