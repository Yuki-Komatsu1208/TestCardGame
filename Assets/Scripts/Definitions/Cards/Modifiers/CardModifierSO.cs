using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    /// <summary>
    /// カードに付与するModifierのScriptableObject定義。
    /// </summary>
    public abstract class CardModifierSO : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private CardModifierTiming timing;

        public string DisplayName => displayName;
        public string Description => description;
        public CardModifierTiming Timing => timing;

        public abstract CardModifier CreateRuntimeModifier();
    }
}
