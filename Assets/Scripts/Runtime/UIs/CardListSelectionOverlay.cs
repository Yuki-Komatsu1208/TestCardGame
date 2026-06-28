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
            // If read-only mode, clicking does nothing
            if (currentChoice == null || onCardSelected == null) return;

            var card = currentDeck[index];

            // Prevent level up if card is already max level
            if (!currentChoice.CanApplyTo(card))
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

            // 2. Create AFTER Card Preview
            if (afterCardContainer != null)
            {
                var afterView = Instantiate(prefab, afterCardContainer);
                var afterRuntime = currentChoice.CreatePreview(card);

                if (currentChoice.Type == RewardType.LevelUp)
                {
                    if (confirmMessageText != null)
                    {
                        confirmMessageText.text = $"カード「<b>{card.CardName}</b>」を\n<color=yellow>レベル {card.Level.Level} ➡ {afterRuntime.Level.Level}</color> へレベルアップします。よろしいですか？";
                    }
                }
                else if (currentChoice.Type == RewardType.Mod)
                {
                    if (confirmMessageText != null)
                    {
                        confirmMessageText.text = $"カード「<b>{card.CardName}</b>」に\n<color=cyan>MOD: {currentChoice.Title}</color> を付与します。よろしいですか？";
                    }
                }

                afterView.Bind(afterRuntime);
                DisableInteractiveCardComponents(afterView);
            }

            confirmModal.SetActive(true);
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
