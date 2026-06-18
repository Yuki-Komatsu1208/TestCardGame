using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Rewards
{
    [Serializable]
    public class CardRewardEntry
    {
        public CardDefinitionSO card;
        public int weight = 1;
    }

    [CreateAssetMenu(fileName = "NewCardRewardPool", menuName = "Card Game/Rewards/Card Reward Pool")]
    public class CardRewardPoolSO : ScriptableObject
    {
        public List<CardRewardEntry> candidates;
    }
}