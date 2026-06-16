using System;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 対象に隣接していれば攻撃し、離れていれば1マス近づく行動。
    /// </summary>
    public sealed class ChaseAndAttackAlgorithm : DefaultEnemyAlgorithm
    {
        private readonly int attackDamage;

        public ChaseAndAttackAlgorithm(int attackDamage)
        {
            if (attackDamage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackDamage), "攻撃ダメージは0以上である必要があります。");
            }

            this.attackDamage = attackDamage;
        }

        public override void Execute(EnemyTurnContext context)
        {
            Vector2Int diff = context.Target.Position - context.Enemy.Position;
            int distance = Mathf.Abs(diff.x) + Mathf.Abs(diff.y);

            if (distance == 1)
            {
                Attack(context);
                return;
            }

            if (diff == Vector2Int.zero)
            {
                return;
            }

            MoveTowardTarget(context, diff);
        }

        private void Attack(EnemyTurnContext context)
        {
            context.Target.Hp.TakeDamage(attackDamage);
            Debug.Log($"{context.Enemy.Name} attacked {context.Target.Name} for {attackDamage} damage! {context.Target.Name} HP: {context.Target.Hp.CurrentValue}");
        }

        private void MoveTowardTarget(EnemyTurnContext context, Vector2Int diff)
        {
            Vector2Int primary = GetPrimaryDirection(diff);
            if (context.MoveService.RequestMoveRelative(context.Enemy.ID, primary))
            {
                return;
            }

            Vector2Int secondary = GetSecondaryDirection(diff);
            if (secondary != Vector2Int.zero)
            {
                context.MoveService.RequestMoveRelative(context.Enemy.ID, secondary);
            }
        }

        private static Vector2Int GetPrimaryDirection(Vector2Int diff)
        {
            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            {
                return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
            }

            return new Vector2Int(0, diff.y > 0 ? 1 : -1);
        }

        private static Vector2Int GetSecondaryDirection(Vector2Int diff)
        {
            if (diff.x == 0 || diff.y == 0)
            {
                return Vector2Int.zero;
            }

            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            {
                return new Vector2Int(0, diff.y > 0 ? 1 : -1);
            }

            return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
        }
    }
}
