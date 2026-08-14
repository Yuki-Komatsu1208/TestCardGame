using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Run
{
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Card Game/Shop Catalog")]
    public class ShopCatalogSO : ScriptableObject
    {
        [Min(1)] public int referenceWeight = 10;
        [Min(0)] public int keystonePrice = 60;
        public List<KeystoneDefinition> keystones = new();
        public List<ShopCardEntry> cardPool = new();
    }

    [Serializable]
    public class ShopCardEntry
    {
        public CardDefinitionSO card;
        [Min(1)] public int baseWeight = 1;
        [Min(0)] public int standardPrice = 20;
    }
}
