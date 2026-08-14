using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;

namespace TestCardGame.UIs.Cards
{
    public enum CardListMode
    {
        Browse,
        OutOfBattleSelection
    }

    /// <summary>要求元リスト上の位置を、UIへ公開する不透明な選択識別子として扱う。</summary>
    public readonly struct CardSelectionToken : IEquatable<CardSelectionToken>
    {
        public int SourceIndex { get; }

        public CardSelectionToken(int sourceIndex)
        {
            SourceIndex = sourceIndex;
        }

        public bool Equals(CardSelectionToken other) => SourceIndex == other.SourceIndex;
        public override bool Equals(object obj) => obj is CardSelectionToken other && Equals(other);
        public override int GetHashCode() => SourceIndex;
        public override string ToString() => SourceIndex.ToString();
    }

    /// <summary>カード一覧Viewだけが参照する変更不能な表示値。</summary>
    public sealed class CardDisplayModel
    {
        public CardSelectionToken Token { get; }
        public string Name { get; }
        public int CurrentLevel { get; }
        public int CurrentCost { get; }
        public int CurrentCooldown { get; }
        public string EffectDescription { get; }
        public IReadOnlyList<string> ModifierNames { get; }
        public bool IsSelectable { get; }
        public string DisabledReason { get; }

        public CardDisplayModel(CardSelectionToken token, string name, int currentLevel, int currentCost,
            int currentCooldown, string effectDescription, IReadOnlyList<string> modifierNames,
            bool isSelectable, string disabledReason)
        {
            Token = token;
            Name = name ?? string.Empty;
            CurrentLevel = currentLevel;
            CurrentCost = currentCost;
            CurrentCooldown = currentCooldown;
            EffectDescription = effectDescription ?? string.Empty;
            ModifierNames = modifierNames ?? Array.Empty<string>();
            IsSelectable = isSelectable;
            DisabledReason = disabledReason ?? string.Empty;
        }
    }

    /// <summary>選択元、候補条件、選択枚数を一つの要求として保持する。</summary>
    public sealed class CardSelectionRequest
    {
        public string Title { get; }
        public IReadOnlyList<CardBase> SourceCards { get; }
        public Predicate<CardBase> IsCandidate { get; }
        public Func<CardBase, string> GetDisabledReason { get; }
        public int MinimumSelectionCount { get; }
        public int MaximumSelectionCount { get; }
        public bool CanCancel { get; }

        public CardSelectionRequest(string title, IReadOnlyList<CardBase> sourceCards,
            Predicate<CardBase> isCandidate, int minimumSelectionCount, int maximumSelectionCount,
            bool canCancel, Func<CardBase, string> getDisabledReason = null)
        {
            Title = title ?? string.Empty;
            SourceCards = sourceCards;
            IsCandidate = isCandidate;
            MinimumSelectionCount = minimumSelectionCount;
            MaximumSelectionCount = maximumSelectionCount;
            CanCancel = canCancel;
            GetDisabledReason = getDisabledReason;
        }
    }

    public sealed class CardSelectionResult
    {
        public IReadOnlyList<CardSelectionToken> SelectedTokens { get; }

        public CardSelectionResult(IReadOnlyList<CardSelectionToken> selectedTokens)
        {
            SelectedTokens = selectedTokens ?? Array.Empty<CardSelectionToken>();
        }
    }

    public readonly struct CardSelectionValidation
    {
        public bool IsValid { get; }
        public string Reason { get; }
        public int CandidateCount { get; }

        public CardSelectionValidation(bool isValid, string reason, int candidateCount)
        {
            IsValid = isValid;
            Reason = reason ?? string.Empty;
            CandidateCount = candidateCount;
        }
    }
}
