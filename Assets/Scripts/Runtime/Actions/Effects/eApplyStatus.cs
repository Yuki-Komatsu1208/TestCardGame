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

        /// <summary>
        /// 付与する状態異常、持続ターン、補助値を指定して効果を作成する。
        /// </summary>
        public eApplyStatus(StatusEffectSO statusEffect, int duration, int value = 0)
        {
            this.statusEffect = statusEffect;
            this.duration = duration;
            this.value = value;
        }

        /// <summary>
        /// 対象マスが盤面上に存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return statusEffect != null && cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象マスのユニットへ状態異常を付与する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (statusEffect == null)
            {
                Debug.LogWarning("状態異常付与効果の状態異常定義が設定されていません。");
                return;
            }

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
