using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 対象に睡眠状態異常を付与する効果。
    /// </summary>
    public sealed class eSleep : ActionEffect
    {
        private readonly int duration;

        public eSleep(int duration = 1)
        {
            this.duration = duration;
        }

        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell != null && cell.Occupant != null)
            {
                context.StatusEffectService?.ApplySleep(cell.Occupant, duration);
                Debug.Log($"{cell.Occupant.Name}に睡眠状態異常（{duration}ターン）を付与しました。");
            }
            else
            {
                Debug.LogWarning("睡眠効果の対象マスにユニットが存在しません。");
            }
        }
    }
}
