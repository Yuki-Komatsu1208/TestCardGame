using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者を指定方向へ移動させる効果。
    /// </summary>
    public sealed class eMove : ActionEffect, IActionPreviewProvider
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
            return actualStep > 0 && context.MoveService.GetCellAt(userPos + dir)?.CanMove == true;
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

            context.MoveService.Move(context.User.ID, dir * actualStep);
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

        /// <summary>実行時と同じ方向へ、移動可能な範囲までのセルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            if (context?.User == null || context.MoveService == null) return ActionPreviewResult.None;
            Vector2Int offset = context.CandidatePosition - context.User.Position;
            if (offset == Vector2Int.zero) return new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
            Vector2Int direction = GetMoveDirection(offset);
            int requested = Mathf.Min(Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)), step);
            var cells = new List<Vector2Int>();
            for (int i = 1; i <= requested; i++)
            {
                Vector2Int position = context.User.Position + direction * i;
                if (context.MoveService.GetCellAt(position)?.CanMove != true) break;
                cells.Add(position);
            }
            return new ActionPreviewResult(true, cells, System.Array.Empty<Vector2Int>(), cells);
        }
    }
}
