using System;
using System.Collections.Generic;
using System.Linq;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Core.Modifiers;
using TestCardGame.Cards.VOs;
using TestCardGame.Rewards;

namespace TestCardGame.Run
{
    public sealed class TownPurchaseResult
    {
        public bool Succeeded { get; }
        public string Message { get; }
        public int HealAmount { get; }

        private TownPurchaseResult(bool succeeded, string message, int healAmount)
        {
            Succeeded = succeeded;
            Message = message;
            HealAmount = healAmount;
        }

        public static TownPurchaseResult Success(string message, int healAmount = 0)
        {
            return new TownPurchaseResult(true, message, healAmount);
        }

        public static TownPurchaseResult Failure()
        {
            return new TownPurchaseResult(false, string.Empty, 0);
        }
    }

    public sealed class TownService
    {
        private readonly IRandomService randomService;

        public TownService(IRandomService randomService)
        {
            this.randomService = randomService ?? throw new ArgumentNullException(nameof(randomService));
        }

        public bool CanBuyHeal(RunState runState, RunEconomyConfig economyConfig, int maxHp)
        {
            return IsTownOpen(runState) && runState.ownedGold >= economyConfig.TownHealCost && runState.currentHp < maxHp;
        }

        public bool CanBuyLevelUp(RunState runState, RunEconomyConfig economyConfig)
        {
            return IsTownOpen(runState)
                && runState.ownedGold >= economyConfig.TownLevelUpCost
                && runState.playerDeck.Any(card => card != null && card.Level.CanUpgrade);
        }

        public bool CanBuyModifier(RunState runState, RunEconomyConfig economyConfig, IEnumerable<CardModifierSO> modifierPool)
        {
            return IsTownOpen(runState)
                && runState.ownedGold >= economyConfig.TownModCost
                && runState.playerDeck.Any(card => card != null)
                && modifierPool != null
                && modifierPool.Any(mod => mod != null);
        }

        public bool CanBuyNewCard(RunState runState, RunDefinitionSO runDefinition, RunEconomyConfig economyConfig)
        {
            return IsTownOpen(runState)
                && runState.ownedGold >= economyConfig.TownNewCardCost
                && HasTownRewardCardCandidate(runDefinition);
        }

        public TownPurchaseResult TryBuyHeal(RunState runState, RunEconomyConfig economyConfig, int maxHp)
        {
            if (!CanBuyHeal(runState, economyConfig, maxHp))
            {
                return TownPurchaseResult.Failure();
            }

            SpendGold(runState, economyConfig.TownHealCost);
            int healAmount = (int)Math.Round(maxHp * 0.25f, MidpointRounding.AwayFromZero);
            runState.currentHp = Math.Min(maxHp, runState.currentHp + healAmount);

            return TownPurchaseResult.Success($"街でHP回復を購入: {healAmount} 回復, 残りゴールド {runState.ownedGold}", healAmount);
        }

        public TownPurchaseResult TryBuyLevelUp(RunState runState, RunEconomyConfig economyConfig)
        {
            if (!CanBuyLevelUp(runState, economyConfig))
            {
                return TownPurchaseResult.Failure();
            }

            var candidates = runState.playerDeck.Where(card => card != null && card.Level.CanUpgrade).ToList();
            var target = candidates[randomService.Range(0, candidates.Count)];
            target.ChangeLevel(target.Level.Upgrade());
            SpendGold(runState, economyConfig.TownLevelUpCost);

            return TownPurchaseResult.Success($"街でレベルアップを購入: {target.CardName} -> Lv{target.Level.Level}, 残りゴールド {runState.ownedGold}");
        }

        public TownPurchaseResult TryBuyModifier(RunState runState, RunEconomyConfig economyConfig, IEnumerable<CardModifierSO> modifierPool)
        {
            if (!CanBuyModifier(runState, economyConfig, modifierPool))
            {
                return TownPurchaseResult.Failure();
            }

            var availableMods = modifierPool.Where(mod => mod != null).Distinct().ToList();
            var cards = runState.playerDeck.Where(card => card != null).ToList();
            var mod = availableMods[randomService.Range(0, availableMods.Count)];
            var card = cards[randomService.Range(0, cards.Count)];

            card.AddEnchant(mod);
            SpendGold(runState, economyConfig.TownModCost);

            return TownPurchaseResult.Success($"街でMOD付与を購入: {card.CardName} に {mod.DisplayName} を付与, 残りゴールド {runState.ownedGold}");
        }

        public TownPurchaseResult TryBuyNewCard(RunState runState, RunDefinitionSO runDefinition, RunEconomyConfig economyConfig)
        {
            if (!CanBuyNewCard(runState, runDefinition, economyConfig))
            {
                return TownPurchaseResult.Failure();
            }

            var rewardCard = RollTownRewardCard(runState, runDefinition);
            if (rewardCard == null)
            {
                return TownPurchaseResult.Failure();
            }

            runState.playerDeck.Add(new CardBase(rewardCard, CardLevel.one));
            SpendGold(runState, economyConfig.TownNewCardCost);

            return TownPurchaseResult.Success($"街で新カードを購入: {rewardCard.cardName}, 残りゴールド {runState.ownedGold}");
        }

        private bool IsTownOpen(RunState runState)
        {
            return runState != null && runState.phase == RunProgressPhase.Town;
        }

        private void SpendGold(RunState runState, int amount)
        {
            runState.ownedGold = Math.Max(0, runState.ownedGold - Math.Max(0, amount));
        }

        private bool HasTownRewardCardCandidate(RunDefinitionSO runDefinition)
        {
            return EnumerateTownRewardCardCandidates(runDefinition).Any();
        }

        private CardDefinitionSO RollTownRewardCard(RunState runState, RunDefinitionSO runDefinition)
        {
            var candidates = EnumerateTownRewardCardCandidates(runDefinition).ToList();
            if (candidates.Count == 0)
            {
                return null;
            }

            int totalWeight = candidates.Sum(card => BuildWeightService.GetWeight(card.buildTags, runState));
            int roll = randomService.Range(0, totalWeight);
            foreach (CardDefinitionSO card in candidates)
            {
                roll -= BuildWeightService.GetWeight(card.buildTags, runState);
                if (roll < 0) return card;
            }

            return candidates[candidates.Count - 1];
        }

        private IEnumerable<CardDefinitionSO> EnumerateTownRewardCardCandidates(RunDefinitionSO runDefinition)
        {
            if (runDefinition?.stages == null)
            {
                yield break;
            }

            foreach (var stage in runDefinition.stages)
            {
                if (stage?.rewardPool?.candidates == null)
                {
                    continue;
                }

                foreach (var entry in stage.rewardPool.candidates)
                {
                    if (entry?.card == null)
                    {
                        continue;
                    }

                    int weight = Math.Max(1, entry.weight);
                    for (int i = 0; i < weight; i++)
                    {
                        yield return entry.card;
                    }
                }
            }
        }
    }
}
