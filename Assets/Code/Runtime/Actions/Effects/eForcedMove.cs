using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>カード定義の方向へ対象を強制移動させる。</summary>
    public sealed class eForcedMove : ActionEffect, IActionPreviewProvider
    {
        private readonly int distance;
        private readonly ForceMoveDirection direction;
        private readonly int baseStunDamage;

        public eForcedMove(int distance, ForceMoveDirection direction, int baseStunDamage)
        {
            this.distance = Mathf.Max(1, distance);
            this.direction = direction;
            this.baseStunDamage = Mathf.Max(0, baseStunDamage);
        }
        /// <summary>
        /// 対象マスにユニットがいるか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象の周囲から移動先を選び、強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("強制移動効果: 対象マスにユニットがいません。");
                return;
            }

            context.MoveService.ForceMove(context.User, cell.Occupant, distance, direction, baseStunDamage);
        }

        /// <summary>強制移動を受ける対象セルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            bool valid = context != null && CanExecute(context.ToActionContext());
            return valid ? new ActionPreviewResult(true, new[] { context.CandidatePosition }, new[] { context.CandidatePosition })
                : new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
        }
    }
}
