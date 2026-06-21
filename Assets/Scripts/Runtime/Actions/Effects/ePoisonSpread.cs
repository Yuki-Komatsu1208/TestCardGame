using TestCardGame.Actions.Core;
using TestCardGame.Definitions.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定地点とその上下左右へ毒をばらまく効果。
    /// </summary>
    public sealed class ePoisonSpread : ActionEffect
    {
        private readonly StatusEffectSO poisonEffect;
        private readonly int duration;
        private readonly int value;

        /// <summary>
        /// 毒定義、持続、値を指定して作成する。
        /// </summary>
        public ePoisonSpread(StatusEffectSO poisonEffect, int duration, int value)
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
            if (poisonEffect == null) return;

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
    }
}
