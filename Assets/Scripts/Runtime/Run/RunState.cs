using System;
using System.Collections.Generic;

namespace TestCardGame.Run
{
    /// <summary>
    /// ラン中に継続するプレイヤーの状態。
    /// </summary>
    [Serializable]
    public class RunState
    {
        public List<DeckCardEntry> playerDeck = new();
        public int currentStageIndex;
        public int currentHp;
    }
}
