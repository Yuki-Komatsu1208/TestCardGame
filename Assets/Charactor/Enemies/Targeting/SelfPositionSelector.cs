using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Targeting
{
    /// <summary>
    /// 敵自身の現在位置を対象にする。
    /// </summary>
    public sealed class SelfPositionSelector : IEnemyTargetSelector
    {
        public bool TrySelectTarget(EnemyTurnContext context, out Vector2Int targetPosition)
        {
            targetPosition = context.Enemy.Position;
            return true;
        }
    }
}
