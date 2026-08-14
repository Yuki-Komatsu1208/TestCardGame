using System;
using System.Collections.Generic;
using TestCardGame.Character.Player;
using TestCardGame.Cards.Core;

namespace TestCardGame.UIs.Cards
{
    /// <summary>戦闘手札Viewへ候補だけを渡し、通常操作との排他的な選択を仲介する。</summary>
    public sealed class CombatCardSelectionCoordinator : ICombatCardSelectionPort, IDisposable
    {
        private readonly ICombatCardSelectionView view;
        private readonly Func<PlayerUnit> getPlayer;
        private CardSelectionSession session;
        private Action<CardSelectionResult> confirmed;
        private Action cancelled;

        public bool IsSelecting => session != null;

        public CombatCardSelectionCoordinator(ICombatCardSelectionView view, Func<PlayerUnit> getPlayer)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.getPlayer = getPlayer;
            view.CardSelectionToggled += Toggle;
            view.ConfirmRequested += Confirm;
            view.CancelRequested += Cancel;
        }

        /// <summary>候補不足または別選択中なら手札を選択モードへ変更しない。</summary>
        public bool TryBegin(CardSelectionRequest request, Action<CardSelectionResult> onConfirmed,
            Action onCancelled = null)
        {
            if (session != null || !CardSelectionSession.TryCreate(request, out session, out _))
                return false;

            confirmed = onConfirmed;
            cancelled = onCancelled;
            IReadOnlyList<CardDisplayModel> allModels = CardDisplayModelFactory.Create(request, getPlayer?.Invoke());
            var candidates = new List<CardDisplayModel>();
            var uniqueCards = new HashSet<CardBase>();
            foreach (CardDisplayModel model in allModels)
            {
                CardBase source = request.SourceCards[model.Token.SourceIndex];
                if (model.IsSelectable && source != null && uniqueCards.Add(source))
                    candidates.Add(model);
            }
            view.EnterSelectionMode(request.Title, candidates, request.MinimumSelectionCount,
                request.MaximumSelectionCount, request.CanCancel);
            view.UpdateSelection(session.SelectedTokens, session.CanConfirm);
            return true;
        }

        public void Dispose()
        {
            view.CardSelectionToggled -= Toggle;
            view.ConfirmRequested -= Confirm;
            view.CancelRequested -= Cancel;
            if (session != null) view.ExitSelectionMode();
            Clear();
        }

        private void Toggle(CardSelectionToken token)
        {
            if (session == null) return;
            session.TryToggle(token, out _);
            view.UpdateSelection(session.SelectedTokens, session.CanConfirm);
        }

        private void Confirm()
        {
            if (session == null || !session.TryBuildResult(out CardSelectionResult result, out _))
                return;
            Action<CardSelectionResult> callback = confirmed;
            view.ExitSelectionMode();
            Clear();
            callback?.Invoke(result);
        }

        private void Cancel()
        {
            if (session == null || !session.Request.CanCancel) return;
            Action callback = cancelled;
            view.ExitSelectionMode();
            Clear();
            callback?.Invoke();
        }

        private void Clear()
        {
            session = null;
            confirmed = null;
            cancelled = null;
        }
    }
}
