using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;

namespace TestCardGame.Run
{
    /// <summary>
    /// ラン中に継続するプレイヤーの状態。
    /// </summary>
    [Serializable]
    public class RunState
    {
        public List<CardBase> playerDeck = new();
        public int currentStageIndex;
        public int currentHp;
    }
}
