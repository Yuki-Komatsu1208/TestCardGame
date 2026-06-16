using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Cards.Core;

namespace TestCardGame.Character.Player
{
    [CreateAssetMenu(fileName = "NewPlayerDefinition", menuName = "Card Game/Player Definition")]
    public class PlayerDefinitionSO : ScriptableObject
    {
        public string playerName = "Player";
        public int maxHp = 100;
        public int initialMana = 3;
        public int maxMana = 5;
        public Sprite playerSprite;
        public List<CardDefinitionSO> initialCards;
    }
}
