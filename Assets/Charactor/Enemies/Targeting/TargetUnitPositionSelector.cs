using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Targeting
{
    /// <summary>
    /// 敵のターゲットユニットの現在位置を対象にする。
    /// </summary>
    public sealed class TargetUnitPositionSelector : IEnemyTargetSelector
    {
        public bool TrySelectTarget(EnemyTurnContext context, out Vector2Int targetPosition)
        {
            targetPosition = default;
            if (context.Target == null)
            {
                return false;
            }

            targetPosition = context.Target.Position;
            return true;
        }
    }
}
