using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.UIs;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>カードを対象にする操作で、UI選択と確定済みコールバックを仲介する。</summary>
    public sealed class CardTargetSelectionService
    {
        private readonly Action onSelectionResolved;

        public CardTargetSelectionService(Action onSelectionResolved)
        {
            this.onSelectionResolved = onSelectionResolved;
        }

        public bool RequestSelection(List<CardBase> cards, Func<CardBase, bool> canSelect, Action<CardBase> onConfirm, string title)
        {
            var overlay = CardListSelectionOverlay.Instance ?? UnityEngine.Object.FindAnyObjectByType<CardListSelectionOverlay>();
            if (overlay == null || cards == null) return false;

            overlay.ShowCardOperation(title, cards, canSelect, card =>
            {
                onConfirm?.Invoke(card);
                onSelectionResolved?.Invoke();
            }, "このカードを発動します。よろしいですか？", null, false);
            return true;
        }
    }
}
