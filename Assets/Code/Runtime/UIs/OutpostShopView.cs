using System;
using System.Collections.Generic;
using TestCardGame.Run;
using UnityEngine;

namespace TestCardGame.UIs
{
    public sealed class OutpostShopView : MonoBehaviour
    {
        [SerializeField] private OutpostCardProductSlotView[] cardSlots = Array.Empty<OutpostCardProductSlotView>();
        [SerializeField] private OutpostKeystoneProductSlotView[] keystoneSlots = Array.Empty<OutpostKeystoneProductSlotView>();
        public int CardSlotCount => cardSlots?.Length ?? 0;
        public int KeystoneSlotCount => keystoneSlots?.Length ?? 0;
        public void Bind(Action<int> cardSelected, Action<KeystoneId> keystoneSelected) { foreach (var slot in cardSlots) slot.Bind(cardSelected); foreach (var slot in keystoneSlots) slot.Bind(keystoneSelected); }
        public void Render(IReadOnlyList<ShopCardDisplayModel> cards, IReadOnlyList<KeystoneDisplayModel> keystones) { for (int i = 0; i < cardSlots.Length; i++) { bool visible = cards != null && i < cards.Count; cardSlots[i].gameObject.SetActive(visible); if (visible) cardSlots[i].Render(cards[i]); } for (int i = 0; i < keystoneSlots.Length; i++) { bool visible = keystones != null && i < keystones.Count; keystoneSlots[i].gameObject.SetActive(visible); if (visible) keystoneSlots[i].Render(keystones[i]); } }
        public bool ValidateReferences() { if (CardSlotCount != ShopService.CardOfferCount || KeystoneSlotCount != 3) { Debug.LogError($"{name}: 商品枠はカード10、キーストーン3が必要です。", this); return false; } foreach (var slot in cardSlots) if (slot == null || !slot.ValidateReferences()) return false; foreach (var slot in keystoneSlots) if (slot == null || !slot.ValidateReferences()) return false; return true; }
    }
}
