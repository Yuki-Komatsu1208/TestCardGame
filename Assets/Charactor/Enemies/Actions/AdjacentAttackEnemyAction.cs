using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Actions
{
    /// <summary>
    /// 隣接している対象を攻撃する敵行動。
    /// </summary>
    public sealed class AdjacentAttackEnemyAction : EnemyAction
    {
        public AdjacentAttackEnemyAction(int damage)
            : base(new List<ActionEffect> { new ePositionAttack(damage, 1) })
        {
        }

        public override bool CanExecute(EnemyTurnContext context)
        {
            Vector2Int diff = context.Target.Position - context.Enemy.Position;
            return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1;
        }

        protected override Vector2Int GetTargetPosition(EnemyTurnContext context)
        {
            return context.Target.Position;
        }
    }
}
