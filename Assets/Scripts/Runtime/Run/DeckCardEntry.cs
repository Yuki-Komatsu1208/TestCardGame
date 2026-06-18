using System;
using TestCardGame.Cards.Core;

namespace TestCardGame.Run
{
    [Serializable]
    public class DeckCardEntry
    {
        public CardDefinitionSO card;
        public int level;

        public DeckCardEntry(CardDefinitionSO card, int level)
        {
            this.card = card;
            this.level = level;
        }
    }
}