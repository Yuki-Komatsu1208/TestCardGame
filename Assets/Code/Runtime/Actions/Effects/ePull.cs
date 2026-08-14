using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 対象ユニットを使用者の近くへ引き寄せる効果。
    /// </summary>
    public sealed class ePull : ActionEffect, IActionPreviewProvider
    {
        private readonly int distance;

        /// <summary>
        /// 引き寄せる距離を指定して効果を作成する。
        /// </summary>
        public ePull(int distance)
        {
            this.distance = distance;
        }

        /// <summary>
        /// 対象マスにユニットが存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象ユニットを使用者に近づく方向へ強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("引き寄せ効果: 対象マスに引き寄せるユニットがいません。");
                return;
            }

            context.MoveService.Pull(context.User, cell.Occupant, distance);
        }

        /// <summary>引き寄せ対象となるセルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            bool valid = context != null && CanExecute(context.ToActionContext());
            return valid ? new ActionPreviewResult(true, new[] { context.CandidatePosition }, new[] { context.CandidatePosition })
                : new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
        }
    }
}
