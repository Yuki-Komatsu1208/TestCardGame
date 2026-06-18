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

        /// <summary>
        /// 睡眠の持続ターンを指定して効果を作成する。
        /// </summary>
        public eSleep(int duration = 1)
        {
            this.duration = duration;
        }

        /// <summary>
        /// 対象マスが盤面上に存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        /// <summary>
        /// 対象マスのユニットへ睡眠状態を付与する。
        /// </summary>
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
