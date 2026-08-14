using TestCardGame.Cards.Modifiers;
using System.Collections.Generic;
using TestCardGame.Run;
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
        [SerializeField] private List<BuildTag> buildTags = new();

        public string DisplayName => displayName;
        public string Description => description;
        public CardModifierTiming Timing => timing;
        public IReadOnlyList<BuildTag> BuildTags => buildTags ?? (IReadOnlyList<BuildTag>)System.Array.Empty<BuildTag>();

        public abstract CardModifier CreateRuntimeModifier();
    }
}
