using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Actions
{
    /// <summary>
    /// 対象へ近づく敵行動。
    /// </summary>
    public sealed class MoveTowardTargetEnemyAction : EnemyAction
    {
        public MoveTowardTargetEnemyAction(int step)
            : base(new List<ActionEffect> { new eMove(step) })
        {
        }

        public override bool CanExecute(EnemyTurnContext context)
        {
            return context.Target.Position != context.Enemy.Position;
        }

        protected override Vector2Int GetTargetPosition(EnemyTurnContext context)
        {
            return context.Target.Position;
        }
    }
}
