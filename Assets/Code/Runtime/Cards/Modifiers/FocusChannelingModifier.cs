using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用時、追加で集中を一定数獲得するModifier。
    /// </summary>
    public sealed class FocusChannelingModifier : CardModifier
    {
        private readonly int focusAmount;

        /// <summary>
        /// カード使用後に追加する集中量を初期化する。
        /// </summary>
        public FocusChannelingModifier(int focusAmount)
        {
            this.focusAmount = Mathf.Max(1, focusAmount);
        }

        /// <summary>
        /// カード使用後にプレイヤーへ集中を追加する。
        /// </summary>
        public override void OnAfterCardUse(CardModifierContext context)
        {
            if (context?.Player == null) return;

            MagicFocusHelper.AddFocus(context.Player, focusAmount);
            int currentFocus = MagicFocusHelper.GetFocusCount(context.Player);
            Debug.Log($"[魔力集中Mod] カード使用に伴い、追加で集中を {focusAmount} 獲得した。 (現在の集中: {currentFocus})");
        }
    }
}
