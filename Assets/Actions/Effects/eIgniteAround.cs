using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者の上下左右4マスに炎上効果を付与する効果。
    /// </summary>
    public sealed class eIgniteAround : ActionEffect
    {
        private static readonly Vector2Int[] AroundOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private readonly eIgnite ignite;

        public eIgniteAround(int duration, int damage)
        {
            ignite = new eIgnite(duration, damage);
        }

        public override bool CanExecute(ActionContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                if (context.MoveService.GetCellAt(context.User.Position + offset) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Execute(ActionContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                var targetPosition = context.User.Position + offset;
                var actionContext = new ActionContext(context.MoveService, context.User, targetPosition);
                if (ignite.CanExecute(actionContext))
                {
                    ignite.Execute(actionContext);
                }
            }
        }
    }
}
