using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Player;
using TestCardGame.Rewards;
using TestCardGame.UIs.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>事前配置された50枠を使い、閲覧と戦闘外選択を表示する共通カード一覧。</summary>
    public sealed class CardListSelectionOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject containerPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private CardListSlotView[] slots = Array.Empty<CardListSlotView>();
        [SerializeField] private CardDetailPanelView detailPanel;
        [SerializeField] private GameObject footerRoot;
        [SerializeField] private TMP_Text selectionCountText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button okButton;
        [SerializeField] private CardEnhancementConfirmationView enhancementModal;

        private readonly Dictionary<int, CardDisplayModel> modelsBySourceIndex = new();
        private CardSelectionSession session;
        private CardListMode mode;
        private CardEnhancementConfirmation enhancement;
        private Action<CardSelectionResult> confirmed;
        private Action cancelled;
        private Action closed;
        private CardSelectionToken focusedToken;
        private bool hasFocus;
        private bool enhancementApplyStarted;

        public static CardListSelectionOverlay Instance { get; private set; }
        public bool IsOpen => containerPanel != null && containerPanel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogError("カード一覧UIが複数配置されています。");
            else
                Instance = this;

            if (closeButton != null) closeButton.onClick.AddListener(CloseOverlay);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelSelection);
            if (okButton != null) okButton.onClick.AddListener(ConfirmSelection);
            if (containerPanel != null) containerPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ShowBrowse(string title, IReadOnlyList<CardBase> cards, PlayerUnit player = null,
            Action onClosed = null)
        {
            if (IsOpen)
            {
                Debug.LogWarning("別のカード一覧または選択が進行中です。");
                return;
            }

            CardSelectionRequest request = CardDisplayModelFactory.CreateBrowseRequest(title, cards);
            if (!CardSelectionSession.TryCreate(request, out session, out CardSelectionValidation validation))
            {
                Debug.LogWarning(validation.Reason);
                return;
            }

            mode = CardListMode.Browse;
            closed = onClosed;
            cancelled = null;
            confirmed = null;
            enhancement = null;
            Open(request, player);
        }

        /// <summary>候補不足ならUIを開かず、理由を返す。</summary>
        public bool TryShowSelection(CardSelectionRequest request, PlayerUnit player,
            CardEnhancementConfirmation enhancementConfirmation,
            Action<CardSelectionResult> onConfirmed, Action onCancelled, out string reason)
        {
            if (IsOpen)
            {
                reason = "別のカード一覧または選択が進行中です。";
                return false;
            }
            if (!CardSelectionSession.TryCreate(request, out session, out CardSelectionValidation validation))
            {
                reason = validation.Reason;
                return false;
            }

            mode = CardListMode.OutOfBattleSelection;
            enhancement = enhancementConfirmation;
            confirmed = onConfirmed;
            cancelled = onCancelled;
            closed = null;
            Open(request, player);
            reason = string.Empty;
            return true;
        }

        private void Open(CardSelectionRequest request, PlayerUnit player)
        {
            enhancementApplyStarted = false;
            modelsBySourceIndex.Clear();
            IReadOnlyList<CardDisplayModel> models = CardDisplayModelFactory.Create(request, player);
            foreach (CardDisplayModel model in models)
                modelsBySourceIndex[model.Token.SourceIndex] = model;

            for (int i = 0; i < slots.Length; i++)
            {
                if (i < models.Count)
                    slots[i].Bind(models[i], OnSlotClicked);
                else if (slots[i] != null)
                    slots[i].Clear();
            }

            if (titleText != null) titleText.text = request.Title;
            if (footerRoot != null) footerRoot.SetActive(mode == CardListMode.OutOfBattleSelection);
            if (closeButton != null) closeButton.gameObject.SetActive(mode == CardListMode.Browse || request.CanCancel);
            if (cancelButton != null) cancelButton.gameObject.SetActive(request.CanCancel);
            if (containerPanel != null) containerPanel.SetActive(true);
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;

            hasFocus = models.Count > 0;
            if (hasFocus)
            {
                focusedToken = models[0].Token;
                detailPanel?.Show(models[0]);
            }
            else
            {
                detailPanel?.Clear();
            }
            RefreshSelectionState();
        }

        private void OnSlotClicked(CardSelectionToken token)
        {
            focusedToken = token;
            hasFocus = true;
            if (modelsBySourceIndex.TryGetValue(token.SourceIndex, out CardDisplayModel model))
                detailPanel?.Show(model);

            if (mode == CardListMode.OutOfBattleSelection && model != null && model.IsSelectable)
            {
                if (!session.TryToggle(token, out string reason))
                    Debug.LogWarning(reason);
            }
            RefreshSelectionState();
        }

        private void RefreshSelectionState()
        {
            foreach (CardListSlotView slot in slots)
            {
                if (slot == null || !slot.gameObject.activeSelf) continue;
                CardSelectionToken token = slot.Token;
                slot.SetState(mode == CardListMode.OutOfBattleSelection && session.IsSelected(token),
                    hasFocus && token.Equals(focusedToken));
            }
            if (selectionCountText != null)
                selectionCountText.text = $"{session.SelectedCount} / {session.Request.MaximumSelectionCount}";
            if (okButton != null)
                okButton.interactable = mode == CardListMode.OutOfBattleSelection && session.CanConfirm;
        }

        private void ConfirmSelection()
        {
            if (session == null)
            {
                Debug.LogWarning("カード選択が開始されていません。");
                return;
            }
            if (!session.TryBuildResult(out CardSelectionResult result, out string reason))
            {
                Debug.LogWarning(reason);
                return;
            }

            if (enhancement == null)
            {
                Complete(result);
                return;
            }
            if (!session.TryResolveResult(result, out IReadOnlyList<CardBase> cards, out reason))
            {
                Debug.LogWarning(reason);
                return;
            }

            enhancementModal.Show(BuildTargetNames(cards), enhancement.EnhancementSummary,
                () => ApplyEnhancement(result), RefreshSelectionState);
        }

        /// <summary>OK連打を防ぎ、再検証に通った場合だけバックエンドへ一度適用を依頼する。</summary>
        private void ApplyEnhancement(CardSelectionResult result)
        {
            if (enhancementApplyStarted) return;
            string reason = string.Empty;
            CardSelectionResult currentResult = null;
            IReadOnlyList<CardBase> cards = null;
            bool valid = session != null &&
                         session.TryBuildResult(out currentResult, out reason) &&
                         SameTokens(result, currentResult) &&
                         session.TryResolveResult(currentResult, out cards, out reason);
            if (!valid)
            {
                enhancementModal.ShowError(string.IsNullOrWhiteSpace(reason) ? "選択内容が変更されました。" : reason);
                return;
            }

            enhancementApplyStarted = true;
            bool applied = enhancement.TryApply != null && enhancement.TryApply(cards);
            if (!applied)
            {
                enhancementApplyStarted = false;
                enhancementModal.ShowError("強化を適用できませんでした。対象を確認してください。");
                return;
            }
            enhancementModal.Hide();
            Complete(currentResult);
        }

        private void Complete(CardSelectionResult result)
        {
            Action<CardSelectionResult> callback = confirmed;
            HideAndClear();
            callback?.Invoke(result);
        }

        private void CancelSelection()
        {
            if (session == null || !session.Request.CanCancel) return;
            Action callback = cancelled;
            HideAndClear();
            callback?.Invoke();
        }

        public void CloseOverlay()
        {
            if (mode == CardListMode.OutOfBattleSelection)
            {
                CancelSelection();
                return;
            }
            Action callback = closed;
            HideAndClear();
            callback?.Invoke();
        }

        private void HideAndClear()
        {
            enhancementModal?.Hide();
            if (containerPanel != null) containerPanel.SetActive(false);
            session = null;
            enhancement = null;
            confirmed = null;
            cancelled = null;
            closed = null;
            modelsBySourceIndex.Clear();
        }

        private static string BuildTargetNames(IReadOnlyList<CardBase> cards)
        {
            var builder = new StringBuilder("対象: ");
            for (int i = 0; i < cards.Count; i++)
            {
                if (i > 0) builder.Append("、");
                builder.Append(cards[i].CardName);
            }
            return builder.ToString();
        }

        private static bool SameTokens(CardSelectionResult left, CardSelectionResult right)
        {
            if (left.SelectedTokens.Count != right.SelectedTokens.Count) return false;
            for (int i = 0; i < left.SelectedTokens.Count; i++)
                if (!left.SelectedTokens[i].Equals(right.SelectedTokens[i])) return false;
            return true;
        }

        // 旧呼び出しをScene統合まで維持する薄い互換入口。
        public void Show(string title, List<CardBase> deck, RewardChoice choice = null,
            Action<int> onSelect = null, Action onCancel = null)
        {
            if (choice == null || onSelect == null)
            {
                ShowBrowse(title, deck, null, onCancel);
                return;
            }
            var request = new CardSelectionRequest(title, deck, choice.CanApplyTo, 1, 1, true,
                _ => "この報酬を適用できません。");
            var enhancementConfirmation = new CardEnhancementConfirmation(choice.Title, cards =>
            {
                onSelect(deck.IndexOf(cards[0]));
                return true;
            });
            if (!TryShowSelection(request, null, enhancementConfirmation, null, onCancel, out string reason))
                Debug.LogWarning(reason);
        }

        public void ShowCardOperation(string title, List<CardBase> cards, Func<CardBase, bool> canSelect,
            Action<CardBase> onConfirm, string confirmationMessage, Action onCancel = null, bool allowCancel = true)
        {
            var request = new CardSelectionRequest(title, cards, card => canSelect == null || canSelect(card),
                1, 1, allowCancel);
            var confirmation = new CardEnhancementConfirmation(confirmationMessage, selected =>
            {
                onConfirm?.Invoke(selected[0]);
                return true;
            });
            if (!TryShowSelection(request, null, confirmation, null, onCancel, out string reason))
                Debug.LogWarning(reason);
        }
    }
}
