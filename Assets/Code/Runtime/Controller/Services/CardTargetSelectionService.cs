using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.UIs;
using TestCardGame.UIs.Cards;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>カードを対象にする操作で、UI選択と確定済みコールバックを仲介する。</summary>
    public sealed class CardTargetSelectionService
    {
        private readonly Action onSelectionResolved;
        private readonly ICombatCardSelectionPort selectionPort;

        public CardTargetSelectionService(ICombatCardSelectionPort selectionPort, Action onSelectionResolved)
        {
            this.selectionPort = selectionPort;
            this.onSelectionResolved = onSelectionResolved;
        }

        public bool RequestSelection(List<CardBase> cards, Func<CardBase, bool> canSelect, Action<CardBase> onConfirm, string title)
        {
            return RequestSelection(cards, canSelect, selected =>
            {
                if (selected.Count == 1) onConfirm?.Invoke(selected[0]);
            }, title, 1, 1, false);
        }

        /// <summary>戦闘手札上で最小・最大枚数を指定したトグル選択を開始する。</summary>
        public bool RequestSelection(List<CardBase> cards, Func<CardBase, bool> canSelect,
            Action<IReadOnlyList<CardBase>> onConfirm, string title, int minimum, int maximum,
            bool canCancel = false)
        {
            if (selectionPort == null || cards == null) return false;
            var request = new CardSelectionRequest(title, cards,
                card => canSelect == null || canSelect(card), minimum, maximum, canCancel);
            return selectionPort.TryBegin(request, result =>
            {
                if (result?.SelectedTokens == null) return;
                var selected = new List<CardBase>();
                var unique = new HashSet<CardBase>();
                foreach (CardSelectionToken token in result.SelectedTokens)
                {
                    int index = token.SourceIndex;
                    if (index < 0 || index >= cards.Count || cards[index] == null ||
                        (canSelect != null && !canSelect(cards[index])) || !unique.Add(cards[index])) return;
                    selected.Add(cards[index]);
                }
                if (selected.Count < minimum || selected.Count > maximum) return;
                onConfirm?.Invoke(selected);
                onSelectionResolved?.Invoke();
            });
        }
    }
}
