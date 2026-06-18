using System;
using TestCardGame.Cards.Core;

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

        /// <summary>
        /// カード定義とレベルを指定してデッキ項目を作成する。
        /// </summary>
        public DeckCardEntry(CardDefinitionSO card, int level)
        {
            this.card = card;
            this.level = level;
        }
    }
}
