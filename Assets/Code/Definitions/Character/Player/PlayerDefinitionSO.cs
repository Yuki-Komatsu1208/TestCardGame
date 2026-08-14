using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Cards.Core;

namespace TestCardGame.Character.Player
{
    [CreateAssetMenu(fileName = "NewPlayerDefinition", menuName = "Card Game/Player Definition")]
    public class PlayerDefinitionSO : ScriptableObject
    {
        [Serializable]
        public class InitialCardEntry
        {
            public CardDefinitionSO card;
            [Range(1, 3)]
            public int level = 1;
        }

        public string playerName = "Player";
        public int maxHp = 100;
        [Min(1)] public int maxStunResist = 100;
        public int initialMana = 3;
        public int maxMana = 5;
        public int manaRecoveryPerTurn = 3;
        public Sprite playerSprite;
        public List<InitialCardEntry> initialCards;
    }
}
