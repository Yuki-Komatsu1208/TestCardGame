using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    /// <summary>
    /// 魔力集中Modifierの定義アセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewFocusChannelingModifier", menuName = "Card Game/Card Modifiers/Focus Channeling")]
    public sealed class FocusChannelingModifierSO : CardModifierSO
    {
        [SerializeField, Min(1)] private int focusAmount = 1;

        /// <summary>
        /// 実行時の魔力集中Modifierを作る。
        /// </summary>
        public override CardModifier CreateRuntimeModifier()
        {
            return new FocusChannelingModifier(focusAmount);
        }
    }
}
