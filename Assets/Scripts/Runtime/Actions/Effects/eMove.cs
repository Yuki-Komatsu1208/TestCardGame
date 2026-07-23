using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者を指定方向へ移動させる効果。
    /// </summary>
    public sealed class eMove : ActionEffect
    {
        /// <summary>
        /// 移動距離。
        /// </summary>
        private readonly int step;
        private readonly bool moveAwayFromTarget;

        /// <summary>
        /// 移動距離を指定して効果を作成する。
        /// </summary>
        public eMove(int step, bool moveAwayFromTarget = false)
        {
            this.step = step;
            this.moveAwayFromTarget = moveAwayFromTarget;
        }

        /// <summary>
        /// 指定セルへ向かう移動が可能か判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int diff = context.TargetPosition - userPos;
            if (diff == Vector2Int.zero) return false;

            Vector2Int dir = GetMoveDirection(diff);
            int distance = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? Mathf.Abs(diff.x) : Mathf.Abs(diff.y);
            int availableStep = context.StatusEffectService?.GetAdjustedMoveStep(context.User, step) ?? step;
            if (availableStep <= 0) return false;
            int actualStep = Mathf.Min(distance, availableStep);
            Vector2Int destination = userPos + dir * actualStep;
            return context.MoveService.GetCellAt(destination)?.CanMove == true;
        }

        /// <summary>
        /// 指定セル方向へ、効果の歩数分だけ使用者を移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;

            Vector2Int userPos = context.User.Position;
            Vector2Int diff = context.TargetPosition - userPos;
            Vector2Int dir = GetMoveDirection(diff);
            int distance = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? Mathf.Abs(diff.x) : Mathf.Abs(diff.y);
            int availableStep = context.StatusEffectService?.GetAdjustedMoveStep(context.User, step) ?? step;
            if (availableStep <= 0)
            {
                Debug.Log($"{context.User.Name}は凍傷で動けない。");
                return;
            }

            int actualStep = Mathf.Min(distance, availableStep);

            context.MoveService.RequestMoveRelative(context.User.ID, dir * actualStep);
        }

        /// <summary>
        /// 指定ベクトルを上下左右の単位方向に正規化する。
        /// </summary>
        private static Vector2Int Normalize(Vector2Int d)
            => Mathf.Abs(d.x) >= Mathf.Abs(d.y)
                ? new Vector2Int(d.x == 0 ? 0 : (d.x > 0 ? 1 : -1), 0)
                : new Vector2Int(0, d.y > 0 ? 1 : -1);

        private Vector2Int GetMoveDirection(Vector2Int targetOffset)
        {
            var direction = Normalize(targetOffset);
            return moveAwayFromTarget ? -direction : direction;
        }
    }
}
