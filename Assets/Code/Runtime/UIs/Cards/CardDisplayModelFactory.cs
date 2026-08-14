using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Player;

namespace TestCardGame.UIs.Cards
{
    /// <summary>ゲーム中カードを、現在のPlayer文脈を反映した表示値へ変換する。</summary>
    public static class CardDisplayModelFactory
    {
        public static IReadOnlyList<CardDisplayModel> Create(CardSelectionRequest request, PlayerUnit player)
        {
            var models = new List<CardDisplayModel>();
            if (request?.SourceCards == null)
                return models;

            for (int i = 0; i < request.SourceCards.Count && i < CardSelectionSession.MaximumCardCount; i++)
            {
                CardBase card = request.SourceCards[i];
                if (card == null)
                    continue;

                bool selectable = request.IsCandidate == null || request.IsCandidate(card);
                string disabledReason = selectable ? string.Empty : request.GetDisabledReason?.Invoke(card);
                var modifiers = new List<string>();
                if (card.EnchantDefinitions != null)
                {
                    foreach (var modifier in card.EnchantDefinitions)
                    {
                        if (modifier != null)
                            modifiers.Add(string.IsNullOrWhiteSpace(modifier.DisplayName) ? modifier.name : modifier.DisplayName);
                    }
                }

                models.Add(new CardDisplayModel(new CardSelectionToken(i), card.CardName,
                    card.Level.Level, card.GetCost(player).Amount, card.GetCooldown(player).Turns,
                    card.Description, modifiers, selectable,
                    selectable ? string.Empty : (string.IsNullOrWhiteSpace(disabledReason) ? "このカードは対象に選べません。" : disabledReason)));
            }
            return models;
        }

        public static CardSelectionRequest CreateBrowseRequest(string title, IReadOnlyList<CardBase> cards)
        {
            return new CardSelectionRequest(title, cards, _ => true, 0, 0, true);
        }
    }
}
