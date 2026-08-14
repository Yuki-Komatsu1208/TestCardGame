using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>使用者を指定マスへ直接転移させる。移動不能なら失敗し、位置補正は行わない。</summary>
    public sealed class eTeleport : ActionEffect, IActionPreviewProvider
    {
        public override bool CanExecute(ActionContext context) => context.MoveService.GetCellAt(context.TargetPosition)?.CanMove == true;

        public override void Execute(ActionContext context)
        {
            if (CanExecute(context)) context.MoveService.Teleport(context.User.ID, context.TargetPosition);
        }

        /// <summary>移動可能な指定セルを転移先として返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            bool valid = context?.MoveService?.GetCellAt(context.CandidatePosition)?.CanMove == true;
            return valid ? new ActionPreviewResult(true, new[] { context.CandidatePosition }, System.Array.Empty<Vector2Int>(), new[] { context.CandidatePosition })
                : new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
        }
    }
}
