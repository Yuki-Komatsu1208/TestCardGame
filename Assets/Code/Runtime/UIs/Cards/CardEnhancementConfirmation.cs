using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;

namespace TestCardGame.UIs.Cards
{
    /// <summary>強化確認に表示する内容と、OK時だけ呼ぶバックエンド処理を束ねる。</summary>
    public sealed class CardEnhancementConfirmation
    {
        public string EnhancementSummary { get; }
        public Func<IReadOnlyList<CardBase>, bool> TryApply { get; }

        public CardEnhancementConfirmation(string enhancementSummary,
            Func<IReadOnlyList<CardBase>, bool> tryApply)
        {
            EnhancementSummary = enhancementSummary ?? string.Empty;
            TryApply = tryApply;
        }
    }
}
