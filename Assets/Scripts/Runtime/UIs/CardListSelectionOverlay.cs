using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Views;
using TestCardGame.Rewards;

namespace TestCardGame.UIs
{
    public class CardListSelectionOverlay : MonoBehaviour
    {
        [Header("UI Prefabs & References")]
        [SerializeField] private CardView cardPrefab;

        [Header("Main Panel References")]
        [SerializeField] private GameObject containerPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform gridContent;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Confirmation Modal References")]
        [SerializeField] private GameObject confirmModal;
        [SerializeField] private Transform beforeCardParent;
        [SerializeField] private Transform afterCardContainer;
        [SerializeField] private TextMeshProUGUI confirmMessageText;
        [SerializeField] private Button confirmOkButton;
        [SerializeField] private Button confirmCancelButton;

        // Runtime State
        private List<CardBase> currentDeck;
        private RewardChoice currentChoice;
        private Action<int> onCardSelected;
        private Action onOverlayClosed;
        private Func<CardBase, bool> canSelectCard;
        private string selectionConfirmationMessage;
        private int selectedDeckIndex = -1;

        public static CardListSelectionOverlay Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Register button events programmatically to keep inspector clean
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseOverlay);
            }

            if (confirmOkButton != null)
            {
                confirmOkButton.onClick.RemoveAllListeners();
                confirmOkButton.onClick.AddListener(OnConfirmOk);
            }

            if (confirmCancelButton != null)
            {
                confirmCancelButton.onClick.RemoveAllListeners();
                confirmCancelButton.onClick.AddListener(OnConfirmCancel);
            }

            // Hide initially
            if (containerPanel != null)
            {
                containerPanel.SetActive(false);
            }
            if (confirmModal != null)
            {
                confirmModal.SetActive(false);
            }
        }

        private CardView GetCardPrefab()
        {
            if (cardPrefab != null) return cardPrefab;

            var handView = FindAnyObjectByType<HandView>();
            if (handView != null)
            {
                var field = typeof(HandView).GetField("cardPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    cardPrefab = field.GetValue(handView) as CardView;
                }
            }

            if (cardPrefab == null)
            {
                cardPrefab = FindAnyObjectByType<CardView>();
            }

            return cardPrefab;
        }

        /// <summary>
        /// Generic entry point to display the deck.
        /// </summary>
        public void Show(
            string title, 
            List<CardBase> deck, 
            RewardChoice choice = null, 
            Action<int> onSelect = null, 
            Action onCancel = null
        )
        {
            currentDeck = deck;
            currentChoice = choice;
            onCardSelected = onSelect;
            onOverlayClosed = onCancel;
            canSelectCard = null;
            selectionConfirmationMessage = null;
            selectedDeckIndex = -1;

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (confirmModal != null)
            {
                confirmModal.SetActive(false);
            }

            if (containerPanel != null)
            {
                containerPanel.SetActive(true);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }

            PopulateDeckGrid();
        }

        /// <summary>
        /// 任意のカードを対象にした操作を確認付きで実行する。
        /// カード状態の変更は呼び出し側のドメイン処理へ委譲する。
        /// </summary>
        public void ShowCardOperation(
            string title,
            List<CardBase> cards,
            Func<CardBase, bool> canSelect,
            Action<CardBase> onConfirm,
            string confirmationMessage,
            Action onCancel = null,
            bool allowCancel = true)
        {
            Show(title, cards, null, null, onCancel);
            canSelectCard = canSelect;
            selectionConfirmationMessage = confirmationMessage;
            onCardSelected = index => onConfirm?.Invoke(currentDeck[index]);
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(allowCancel);
            }
        }

        private void PopulateDeckGrid()
        {
            if (gridContent == null) return;

            // Clear existing grid cards
            foreach (Transform child in gridContent)
            {
                Destroy(child.gameObject);
            }

            var prefab = GetCardPrefab();
            if (prefab == null)
            {
                Debug.LogError("CardListSelectionOverlay: Card prefab could not be resolved.");
                return;
            }

            for (int i = 0; i < currentDeck.Count; i++)
            {
                int index = i;
                var card = currentDeck[i];
                if (card == null) continue;

                // Instantiate Card prefab
                var view = Instantiate(prefab, gridContent);
                view.name = $"DeckCardItem_{index}";

                // Bind state
                view.Bind(card);

                // Add button click handler to card
                var btn = view.gameObject.GetComponent<Button>();
                if (btn == null)
                {
                    btn = view.gameObject.AddComponent<Button>();
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCardClicked(index));

                // Block drag & drop on the list
                if (view.TryGetComponent<MoveCardDragHandler>(out var drag))
                {
                    drag.enabled = false;
                }
            }
        }

        private void OnCardClicked(int index)
        {
            var card = currentDeck[index];
            if (card == null)
            {
                return;
            }

            // 報酬選択ではないデッキ確認モードでは、クリックをカード詳細表示として扱う。
            if (onCardSelected == null)
            {
                selectedDeckIndex = -1;
                OpenConfirmationModal(card);
                return;
            }

            if (canSelectCard != null && !canSelectCard(card))
            {
                Debug.LogWarning("このカードは対象に選べません。");
                return;
            }

            // 報酬選択時だけ、報酬固有の適用可否を確認する。
            if (currentChoice != null && !currentChoice.CanApplyTo(card))
            {
                Debug.LogWarning("This reward cannot be applied to the selected card.");
                return;
            }

            selectedDeckIndex = index;
            OpenConfirmationModal(card);
        }

        private void OpenConfirmationModal(CardBase card)
        {
            if (confirmModal == null) return;

            // 報酬適用の確認ではなく、カード詳細だけを確認するモードかを判定する。
            bool isReadOnlyDetail = onCardSelected == null;

            // Clear prior modal previews
            if (beforeCardParent != null)
            {
                foreach (Transform child in beforeCardParent) Destroy(child.gameObject);
            }
            if (afterCardContainer != null)
            {
                foreach (Transform child in afterCardContainer) Destroy(child.gameObject);
            }

            var prefab = GetCardPrefab();
            if (prefab == null) return;

            // 1. Create BEFORE Card Preview
            if (beforeCardParent != null)
            {
                var beforeView = Instantiate(prefab, beforeCardParent);
                beforeView.Bind(card);
                DisableInteractiveCardComponents(beforeView);
            }

            // 詳細確認モードでは確定操作が不要なため、OKボタンを隠す。
            if (confirmOkButton != null)
            {
                confirmOkButton.gameObject.SetActive(!isReadOnlyDetail);
            }

            if (confirmCancelButton != null)
            {
                // 同じボタンを、詳細確認モードでは閉じる操作として分かりやすく表示する。
                var cancelLabel = confirmCancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (cancelLabel != null)
                {
                    cancelLabel.text = isReadOnlyDetail ? "閉じる" : "キャンセル";
                }
            }

            if (isReadOnlyDetail)
            {
                // 詳細確認では比較後カードが存在しないため、右側のプレビュー領域を隠す。
                if (afterCardContainer != null)
                {
                    afterCardContainer.gameObject.SetActive(false);
                }

                // カード詳細に、現在付与されているModifierを一覧として追記する。
                if (confirmMessageText != null)
                {
                    confirmMessageText.text = $"カード「<b>{card.CardName}</b>」の詳細\n{BuildModifierSummary(card, "付与中Modifier")}";
                }

                confirmModal.SetActive(true);
                return;
            }

            if (currentChoice == null)
            {
                if (afterCardContainer != null)
                {
                    afterCardContainer.gameObject.SetActive(false);
                }
                if (confirmMessageText != null)
                {
                    confirmMessageText.text = string.IsNullOrWhiteSpace(selectionConfirmationMessage)
                        ? $"カード「<b>{card.CardName}</b>」を対象にします。よろしいですか？"
                        : selectionConfirmationMessage;
                }

                confirmModal.SetActive(true);
                return;
            }

            // 2. Create AFTER Card Preview
            if (afterCardContainer != null)
            {
                // 報酬適用時は、適用後カードの比較プレビューを再表示する。
                afterCardContainer.gameObject.SetActive(true);

                var afterView = Instantiate(prefab, afterCardContainer);
                var afterRuntime = currentChoice.CreatePreview(card);

                if (currentChoice.Type == RewardType.LevelUp)
                {
                    if (confirmMessageText != null)
                    {
                        // レベルアップ後にも保持されるModifierを確認できるよう、適用後の一覧を表示する。
                        confirmMessageText.text = $"カード「<b>{card.CardName}</b>」を\n<color=yellow>レベル {card.Level.Level} ➡ {afterRuntime.Level.Level}</color> へレベルアップします。よろしいですか？\n{BuildModifierSummary(afterRuntime, "適用後Modifier")}";
                    }
                }
                else if (currentChoice.Type == RewardType.Mod)
                {
                    if (confirmMessageText != null)
                    {
                        // MOD付与後の最終状態として、既存分と新規分を含めたModifier一覧を表示する。
                        confirmMessageText.text = $"カード「<b>{card.CardName}</b>」に\n<color=cyan>MOD: {currentChoice.Title}</color> を付与します。よろしいですか？\n{BuildModifierSummary(afterRuntime, "適用後Modifier")}";
                    }
                }

                afterView.Bind(afterRuntime);
                DisableInteractiveCardComponents(afterView);
            }

            confirmModal.SetActive(true);
        }

        private static string BuildModifierSummary(CardBase card, string label)
        {
            // EnchantDefinitionsをUI向けのModifier一覧テキストへ変換する。
            if (card?.EnchantDefinitions == null || card.EnchantDefinitions.Count == 0)
            {
                return $"<color=#CFCFCF>{label}: なし</color>";
            }

            var modifierNames = new List<string>();
            foreach (var modifierDefinition in card.EnchantDefinitions)
            {
                if (modifierDefinition == null)
                {
                    continue;
                }

                // ユーザーに見せる名前はScriptableObject側のDisplayNameを採用する。
                modifierNames.Add($"・{modifierDefinition.DisplayName}");
            }

            if (modifierNames.Count == 0)
            {
                return $"<color=#CFCFCF>{label}: なし</color>";
            }

            // TextMeshProのリッチテキストで見出しだけ色を付け、各Modifierを改行で列挙する。
            return $"<color=#7FDBFF>{label}</color>\n{string.Join("\n", modifierNames)}";
        }

        private void DisableInteractiveCardComponents(CardView view)
        {
            if (view.TryGetComponent<MoveCardDragHandler>(out var drag)) drag.enabled = false;
            if (view.TryGetComponent<Button>(out var btn)) btn.enabled = false;
        }

        private void OnConfirmOk()
        {
            if (selectedDeckIndex >= 0 && onCardSelected != null)
            {
                onCardSelected.Invoke(selectedDeckIndex);
            }

            if (confirmModal != null)
            {
                confirmModal.SetActive(false);
            }
            if (containerPanel != null)
            {
                containerPanel.SetActive(false);
            }
        }

        private void OnConfirmCancel()
        {
            if (confirmModal != null)
            {
                confirmModal.SetActive(false);
            }
            selectedDeckIndex = -1;
        }

        public void CloseOverlay()
        {
            if (confirmModal != null)
            {
                confirmModal.SetActive(false);
            }
            if (containerPanel != null)
            {
                containerPanel.SetActive(false);
            }
            onOverlayClosed?.Invoke();
        }
    }
}
