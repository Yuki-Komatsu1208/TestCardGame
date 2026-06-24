using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Core.Modifiers;

namespace TestCardGame.Run
{
    /// <summary>
    /// ラン中のデッキに入っているカードとレベルの組み合わせ。
    /// </summary>
    [Serializable]
    public class DeckCardEntry
    {
        public CardDefinitionSO card;
        public int level;
        public List<CardModifierSO> modifiers = new();

        /// <summary>
        /// カード定義とレベルを指定してデッキ項目を作成する。
        /// </summary>
        public DeckCardEntry(CardDefinitionSO card, int level)
        {
            this.card = card;
            this.level = level;
            this.modifiers = new List<CardModifierSO>();
        }

        public DeckCardEntry(CardDefinitionSO card, int level, List<CardModifierSO> modifiers)
        {
            this.card = card;
            this.level = level;
            this.modifiers = modifiers ?? new List<CardModifierSO>();
        }
    }
}
