using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;

namespace TestCardGame.UIs.Cards
{
    /// <summary>UIに依存せず、候補数、トグル、確定直前の同一実体確認を行う。</summary>
    public sealed class CardSelectionSession
    {
        public const int MaximumCardCount = 50;

        private readonly CardSelectionRequest request;
        private readonly CardBase[] originalCards;
        private readonly HashSet<CardSelectionToken> selected = new();

        public CardSelectionRequest Request => request;
        public IReadOnlyCollection<CardSelectionToken> SelectedTokens => selected;
        public int SelectedCount => selected.Count;
        public bool CanConfirm => selected.Count >= request.MinimumSelectionCount &&
                                  selected.Count <= request.MaximumSelectionCount;

        private CardSelectionSession(CardSelectionRequest request)
        {
            this.request = request;
            originalCards = new CardBase[request.SourceCards.Count];
            for (int i = 0; i < request.SourceCards.Count; i++)
                originalCards[i] = request.SourceCards[i];
        }

        /// <summary>要求の形式と一意なカード実体の候補数を検証する。</summary>
        public static CardSelectionValidation Validate(CardSelectionRequest request)
        {
            if (request == null)
                return new CardSelectionValidation(false, "カード選択要求がありません。", 0);
            if (request.SourceCards == null)
                return new CardSelectionValidation(false, "選択元のカード一覧がありません。", 0);
            if (request.SourceCards.Count > MaximumCardCount)
                return new CardSelectionValidation(false, "カード一覧は50枚までです。", 0);
            if (request.MinimumSelectionCount < 0)
                return new CardSelectionValidation(false, "最小選択数は0以上で指定してください。", 0);
            if (request.MaximumSelectionCount < request.MinimumSelectionCount)
                return new CardSelectionValidation(false, "最大選択数は最小選択数以上で指定してください。", 0);
            if (request.MaximumSelectionCount > MaximumCardCount)
                return new CardSelectionValidation(false, "最大選択数は50以下で指定してください。", 0);

            var uniqueCandidates = new HashSet<CardBase>(ReferenceEqualityComparer<CardBase>.Instance);
            foreach (CardBase card in request.SourceCards)
            {
                if (card != null && (request.IsCandidate == null || request.IsCandidate(card)))
                    uniqueCandidates.Add(card);
            }

            if (uniqueCandidates.Count < request.MinimumSelectionCount)
                return new CardSelectionValidation(false, "対象となるカードが足りません。", uniqueCandidates.Count);

            return new CardSelectionValidation(true, string.Empty, uniqueCandidates.Count);
        }

        public static bool TryCreate(CardSelectionRequest request, out CardSelectionSession session,
            out CardSelectionValidation validation)
        {
            validation = Validate(request);
            session = validation.IsValid ? new CardSelectionSession(request) : null;
            return validation.IsValid;
        }

        /// <summary>同じカード実体を二重選択せず、最大数到達後も解除だけは許可する。</summary>
        public bool TryToggle(CardSelectionToken token, out string reason)
        {
            if (selected.Remove(token))
            {
                reason = string.Empty;
                return true;
            }

            if (!TryResolveCurrent(token, out CardBase card, out reason))
                return false;
            if (request.IsCandidate != null && !request.IsCandidate(card))
            {
                reason = GetDisabledReason(card);
                return false;
            }
            if (selected.Count >= request.MaximumSelectionCount)
            {
                reason = $"選択できるのは最大{request.MaximumSelectionCount}枚です。";
                return false;
            }

            foreach (CardSelectionToken selectedToken in selected)
            {
                if (TryResolveCurrent(selectedToken, out CardBase selectedCard, out _) && ReferenceEquals(card, selectedCard))
                {
                    reason = "同じカードを複数回選択することはできません。";
                    return false;
                }
            }

            selected.Add(token);
            reason = string.Empty;
            return true;
        }

        /// <summary>確定直前に枚数、実体の現存、候補条件を再検証する。</summary>
        public bool TryBuildResult(out CardSelectionResult result, out string reason)
        {
            result = null;
            if (!CanConfirm)
            {
                reason = $"{request.MinimumSelectionCount}枚以上、{request.MaximumSelectionCount}枚以下を選択してください。";
                return false;
            }

            var tokens = new List<CardSelectionToken>(selected);
            tokens.Sort((left, right) => left.SourceIndex.CompareTo(right.SourceIndex));
            var uniqueCards = new HashSet<CardBase>(ReferenceEqualityComparer<CardBase>.Instance);
            foreach (CardSelectionToken token in tokens)
            {
                if (!TryResolveCurrent(token, out CardBase card, out reason))
                    return false;
                if (request.IsCandidate != null && !request.IsCandidate(card))
                {
                    reason = GetDisabledReason(card);
                    return false;
                }
                if (!uniqueCards.Add(card))
                {
                    reason = "同じカードが重複して選択されています。";
                    return false;
                }
            }

            result = new CardSelectionResult(tokens);
            reason = string.Empty;
            return true;
        }

        public bool TryResolveResult(CardSelectionResult result, out IReadOnlyList<CardBase> cards, out string reason)
        {
            var resolved = new List<CardBase>();
            if (result == null)
            {
                cards = resolved;
                reason = "選択結果がありません。";
                return false;
            }

            foreach (CardSelectionToken token in result.SelectedTokens)
            {
                if (!TryResolveCurrent(token, out CardBase card, out reason))
                {
                    cards = resolved;
                    return false;
                }
                resolved.Add(card);
            }

            cards = resolved;
            reason = string.Empty;
            return true;
        }

        public bool IsSelected(CardSelectionToken token) => selected.Contains(token);

        private bool TryResolveCurrent(CardSelectionToken token, out CardBase card, out string reason)
        {
            card = null;
            int index = token.SourceIndex;
            if (index < 0 || index >= originalCards.Length || index >= request.SourceCards.Count)
            {
                reason = "選択したカードを確認できません。";
                return false;
            }
            card = request.SourceCards[index];
            if (card == null || !ReferenceEquals(card, originalCards[index]))
            {
                reason = "選択中にカード一覧が変更されました。";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private string GetDisabledReason(CardBase card)
        {
            string reason = request.GetDisabledReason?.Invoke(card);
            return string.IsNullOrWhiteSpace(reason) ? "このカードは対象に選べません。" : reason;
        }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public static readonly ReferenceEqualityComparer<T> Instance = new();
            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
