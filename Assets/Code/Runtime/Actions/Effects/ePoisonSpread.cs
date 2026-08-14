using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using System.Collections.Generic;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定地点とその上下左右へ毒をばらまく効果。
    /// </summary>
    public sealed class ePoisonSpread : ActionEffect, IActionPreviewProvider
    {
        private readonly StatusEffectId poisonEffect;
        private readonly int duration;
        private readonly int value;

        /// <summary>
        /// 毒定義、持続、値を指定して作成する。
        /// </summary>
        public ePoisonSpread(StatusEffectId poisonEffect, int duration, int value)
        {
            this.poisonEffect = poisonEffect;
            this.duration = duration;
            this.value = value;
        }

        /// <summary>
        /// 対象地点と隣接4方向のユニットへ毒を付与する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (poisonEffect == StatusEffectId.None) return;

            Vector2Int targetPos = context.TargetPosition;
            Vector2Int[] coordinates = {
                targetPos,
                targetPos + Vector2Int.up,
                targetPos + Vector2Int.down,
                targetPos + Vector2Int.left,
                targetPos + Vector2Int.right
            };

            foreach (var pos in coordinates)
            {
                var cell = context.MoveService.GetCellAt(pos);
                if (cell != null && cell.Occupant != null)
                {
                    context.StatusEffectService?.ApplyStatus(cell.Occupant, poisonEffect, duration, value);
                }
            }
        }

        /// <summary>指定地点と上下左右の盤面内セルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            if (context?.MoveService?.GetCellAt(context.CandidatePosition) == null) return new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
            Vector2Int[] candidates = { context.CandidatePosition, context.CandidatePosition + Vector2Int.up, context.CandidatePosition + Vector2Int.down, context.CandidatePosition + Vector2Int.left, context.CandidatePosition + Vector2Int.right };
            var cells = new List<Vector2Int>();
            foreach (Vector2Int position in candidates) if (context.MoveService.GetCellAt(position) != null) cells.Add(position);
            return new ActionPreviewResult(true, new[] { context.CandidatePosition }, cells);
        }
    }
}
