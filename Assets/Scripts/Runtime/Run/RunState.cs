using System;
using System.Collections.Generic;

namespace TestCardGame.Run
{
    [Serializable]
    public class RunState
    {
        public List<DeckCardEntry> playerDeck = new();
        public int currentStageIndex;
        public int currentHp;
    }
}