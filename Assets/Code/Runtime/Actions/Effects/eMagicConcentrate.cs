using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 精神統一の効果。
    /// 使用者自身に集中（Focus）を一定スタック追加する。
    /// </summary>
    public sealed class eMagicConcentrate : ActionEffect
    {
        private readonly int focusAmount;

        /// <summary>
        /// 獲得する集中量を初期化する。
        /// </summary>
        public eMagicConcentrate(int focusAmount)
        {
            this.focusAmount = focusAmount;
        }

        /// <summary>
        /// 使用者が存在するか確認する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            return context.User != null;
        }

        /// <summary>
        /// 使用者へ集中を追加する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;

            IUnit user = context.User;
            MagicFocusHelper.AddFocus(user, focusAmount);
            int currentFocus = MagicFocusHelper.GetFocusCount(user);
            Debug.Log($"{user.Name}は精神統一を行い、集中スタックを {focusAmount} 獲得した。 (現在の集中: {currentFocus})");
        }
    }
}
