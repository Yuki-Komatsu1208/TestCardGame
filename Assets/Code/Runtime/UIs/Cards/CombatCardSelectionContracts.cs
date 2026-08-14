using System;
using System.Collections.Generic;

namespace TestCardGame.UIs.Cards
{
    public interface ICombatCardSelectionPort
    {
        bool TryBegin(CardSelectionRequest request, Action<CardSelectionResult> onConfirmed,
            Action onCancelled = null);
    }

    /// <summary>PLAN 04の手札Viewが実装する、戦闘中選択専用の表示契約。</summary>
    public interface ICombatCardSelectionView
    {
        event Action<CardSelectionToken> CardSelectionToggled;
        event Action ConfirmRequested;
        event Action CancelRequested;

        void EnterSelectionMode(string title, IReadOnlyList<CardDisplayModel> candidateCards,
            int minimum, int maximum, bool canCancel);
        void UpdateSelection(IReadOnlyCollection<CardSelectionToken> selectedTokens, bool canConfirm);
        void ExitSelectionMode();
    }
}
