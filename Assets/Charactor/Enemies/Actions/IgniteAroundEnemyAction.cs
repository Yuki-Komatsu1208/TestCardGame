using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Actions
{
    /// <summary>
    /// 自分の上下左右4マスに炎上効果を付与する敵行動。
    /// </summary>
    public sealed class IgniteAroundEnemyAction : EnemyAction
    {
        private static readonly Vector2Int[] AroundOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public IgniteAroundEnemyAction(int duration, int damage)
            : base(new List<ActionEffect> { new eIgnite(duration, damage) })
        {
        }

        /// <summary>
        /// 盤面内に炎上対象マスが1つでもあれば実行可能。
        /// </summary>
        public override bool CanExecute(EnemyTurnContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                if (context.MoveService.GetCellAt(context.Enemy.Position + offset) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 周囲4マスそれぞれにActionContextを作り、炎上効果を発動する。
        /// </summary>
        public override void Execute(EnemyTurnContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                var targetPosition = context.Enemy.Position + offset;
                if (context.MoveService.GetCellAt(targetPosition) == null)
                {
                    continue;
                }

                var actionContext = new ActionContext(context.MoveService, context.Enemy, targetPosition);
                foreach (var effect in Effects)
                {
                    effect.Execute(actionContext);
                }
            }
        }

        protected override Vector2Int GetTargetPosition(EnemyTurnContext context)
        {
            return context.Enemy.Position;
        }
    }
}
