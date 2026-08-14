using System;
using System.Collections.Generic;
using System.Linq;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Run
{
    /// <summary>Outpostショップの抽選・価格・購入を扱う純C#サービス。</summary>
    public sealed class ShopService
    {
        public const int CardOfferCount = 10;
        public const int SaleChancePercent = 10;

        private readonly IRandomService random;

        public ShopService(IRandomService random)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void EnsureInventory(RunState state, ShopCatalogSO catalog)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (state.hasShopInventory) return;

            state.shopCardOffers ??= new List<ShopCardOfferState>();
            state.shopCardOffers.Clear();
            var candidates = catalog.cardPool?
                .Where(entry => entry?.card != null && entry.baseWeight > 0)
                .ToList() ?? new List<ShopCardEntry>();

            int count = Math.Min(CardOfferCount, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                ShopCardEntry selected = SelectWeighted(candidates);
                candidates.Remove(selected);
                bool isOnSale = random.Range(0, 100) < SaleChancePercent;
                int price = CalculatePrice(catalog.referenceWeight, selected.baseWeight, selected.standardPrice);
                state.shopCardOffers.Add(new ShopCardOfferState
                {
                    card = selected.card,
                    isOnSale = isOnSale,
                    price = isOnSale ? price / 2 : price
                });
            }

            state.hasShopInventory = true;
        }

        public int CalculatePrice(int referenceWeight, int baseWeight, int standardPrice)
        {
            if (referenceWeight < 1) throw new ArgumentOutOfRangeException(nameof(referenceWeight));
            if (baseWeight < 1) throw new ArgumentOutOfRangeException(nameof(baseWeight));
            return (int)Math.Ceiling(standardPrice * (double)referenceWeight / baseWeight);
        }

        public bool TryBuyCard(RunState state, ShopCatalogSO catalog, int offerIndex)
        {
            if (state == null || catalog == null || state.phase != RunProgressPhase.Town) return false;
            EnsureInventory(state, catalog);
            if (offerIndex < 0 || offerIndex >= state.shopCardOffers.Count) return false;

            ShopCardOfferState offer = state.shopCardOffers[offerIndex];
            if (offer == null || offer.isPurchased || offer.card == null || state.ownedGold < offer.price) return false;

            state.ownedGold -= offer.price;
            state.playerDeck.Add(new CardBase(offer.card, CardLevel.one));
            offer.isPurchased = true;
            return true;
        }

        public bool TryBuyKeystone(RunState state, ShopCatalogSO catalog, KeystoneId keystoneId)
        {
            if (state == null || catalog == null || state.phase != RunProgressPhase.Town
                || state.selectedKeystone != KeystoneId.None || state.ownedGold < catalog.keystonePrice)
            {
                return false;
            }

            KeystoneDefinition keystone = catalog.keystones?
                .FirstOrDefault(candidate => candidate != null && candidate.id == keystoneId && candidate.id != KeystoneId.None);
            if (keystone == null) return false;

            state.ownedGold -= catalog.keystonePrice;
            state.selectedKeystone = keystone.id;
            state.ownedItemIds ??= new List<string>();
            state.ownedItemIds.Add(keystone.id.ToString());
            state.favoredBuildTags = keystone.favoredTags?.Distinct().ToList() ?? new List<BuildTag>();
            foreach (CardDefinitionSO card in keystone.initialCards?.Where(card => card != null) ?? Enumerable.Empty<CardDefinitionSO>())
            {
                state.playerDeck.Add(new CardBase(card, CardLevel.one));
            }

            return true;
        }

        private ShopCardEntry SelectWeighted(IReadOnlyList<ShopCardEntry> candidates)
        {
            int totalWeight = candidates.Sum(entry => entry.baseWeight);
            int roll = random.Range(0, totalWeight);
            foreach (ShopCardEntry entry in candidates)
            {
                roll -= entry.baseWeight;
                if (roll < 0) return entry;
            }

            return candidates[candidates.Count - 1];
        }
    }
}
