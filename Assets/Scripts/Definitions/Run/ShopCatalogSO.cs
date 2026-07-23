using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Core.Modifiers;
using UnityEngine;

namespace TestCardGame.Run
{
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Card Game/Shop Catalog")]
    public class ShopCatalogSO : ScriptableObject
    {
        [Tooltip("開始時のショップでだけ表示する。価格は通常 0G にする。")]
        public List<ShopOffer> keystoneOffers = new();
        public List<ShopOffer> cardOffers = new();
        public List<ShopOffer> itemOffers = new();
        public List<ShopOffer> modifierOffers = new();
    }

    [Serializable]
    public class ShopOffer
    {
        public string displayName;
        [TextArea] public string description;
        [Min(0)] public int price;
        public CardDefinitionSO card;
        public CardModifierSO modifier;

        public string DisplayName => !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : card != null ? card.cardName : modifier != null ? modifier.DisplayName : "未設定の商品";
    }
}
