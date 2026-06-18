using TestCardGame.Actions.Core;
using TestCardGame.Definitions.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class eApplyStatus : ActionEffect
    {
        private readonly StatusEffectSO statusEffect;
        private readonly int duration;
        private readonly int value;

        public eApplyStatus(StatusEffectSO statusEffect, int duration, int value = 0)
        {
            this.statusEffect = statusEffect;
            this.duration = duration;
            this.value = value;
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
                context.StatusEffectService?.ApplyStatus(cell.Occupant, statusEffect, duration, value);
                Debug.Log($"{cell.Occupant.Name}に状態異常 {statusEffect.DisplayName}（{duration}ターン, 値: {value}）を付与しました。");
            }
            else
            {
                Debug.LogWarning("状態異常付与効果の対象マスにユニットが存在しません。");
            }
        }
    }
}