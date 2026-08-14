using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定座標の対象へスタンダメージだけを与える。
    /// </summary>
    public sealed class eStunDamage : ActionEffect, IActionPreviewProvider
    {
        private readonly int stunDamage;

        public eStunDamage(int stunDamage)
        {
            if (stunDamage < 0) throw new System.ArgumentOutOfRangeException(nameof(stunDamage));
            this.stunDamage = stunDamage;
        }

        public override bool CanExecute(ActionContext context)
        {
            if (context?.MoveService?.GetCellAt(context.TargetPosition) == null) return false;
            return true;
        }

        public override void Execute(ActionContext context)
        {
            if (stunDamage <= 0 || !CanExecute(context)) return;
            var target = context.MoveService.GetUnitAt(context.TargetPosition);
            if (target == null) return;

            context.StatusEffectService?.DealStunDamage(target, stunDamage);
        }

        /// <summary>スタンダメージの対象セルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            bool valid = context != null && CanExecute(context.ToActionContext());
            return valid ? new ActionPreviewResult(true, new[] { context.CandidatePosition }, new[] { context.CandidatePosition })
                : new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
        }
    }
}
