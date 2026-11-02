using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Views;
using TestCardGame.Controller;
using TestCardGame.UIs.Battle;
using TestCardGame.UIs.Cards;
using TMPro;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Battle専用の事前配置済み50枠へ手札を割り当て、重なりと横スクロールを制御する。
/// </summary>
public sealed class HandView : MonoBehaviour, ICombatCardSelectionView, IScrollHandler
{
    public const int RequiredSlotCount = 50;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform focusedCardRoot;
    [SerializeField] private List<BattleHandCardSlotView> slots = new();
    [SerializeField, Min(1)] private int scrollThreshold = 20;
    [SerializeField, Min(1f)] private float cardWidth = 150f;
    [SerializeField, Min(1f)] private float minimumVisibleWidth = 54f;
    [SerializeField, Min(0f)] private float cardOverlap = 94f;
    [SerializeField, Min(1f)] private float scrollCardStep = 86f;
    [SerializeField, Min(0f)] private float bottomPadding = 10f;
    [SerializeField, Min(0f)] private float sidePadding = 22f;
    [Header("Combat card selection")]
    [SerializeField] private GameObject selectionHeaderRoot;
    [SerializeField] private TMP_Text selectionTitleText;
    [SerializeField] private TMP_Text selectionCountText;
    [SerializeField] private Button selectionOkButton;
    [SerializeField] private Button selectionCancelButton;

    private GameController gameController;
    private int boundCount;
    private bool interactionEnabled = true;
    private readonly List<CardBase> boundCards = new();
    private readonly List<CardBase> pendingBoundCards = new();
    private readonly Dictionary<CardSelectionToken, BattleHandCardSlotView> selectionSlots = new();
    private bool combatSelectionMode;
    private bool hasPendingBoundCards;
    private int selectionMaximum;

    public event Action<CardSelectionToken> CardSelectionToggled;
    public event Action ConfirmRequested;
    public event Action CancelRequested;

    public IReadOnlyList<BattleHandCardSlotView> Slots => slots;
    public int ScrollThreshold => scrollThreshold;
    public bool IsScrollEnabled => scrollRect != null && scrollRect.horizontal;
    public RectTransform FocusedCardRoot => focusedCardRoot;

    private void Awake()
    {
        if (selectionOkButton != null) selectionOkButton.onClick.AddListener(NotifyConfirmRequested);
        if (selectionCancelButton != null) selectionCancelButton.onClick.AddListener(NotifyCancelRequested);
        if (selectionHeaderRoot != null) selectionHeaderRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (selectionOkButton != null) selectionOkButton.onClick.RemoveListener(NotifyConfirmRequested);
        if (selectionCancelButton != null) selectionCancelButton.onClick.RemoveListener(NotifyCancelRequested);
    }

    /// <summary>ゲーム操作の入口を一度だけ各カードへ渡す。</summary>
    public void Configure(GameController controller)
    {
        gameController = controller;
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i]?.Configure(this, gameController, i);
        }
    }

    /// <summary>渡されたカード一覧を事前配置済みスロットへ割り当てる。</summary>
    public void ShowCards(IReadOnlyList<CardBase> cards)
    {
        if (!ValidateReferences())
        {
            return;
        }

        int count = cards?.Count ?? 0;
        if (count > RequiredSlotCount)
        {
            Debug.LogError($"HandView: 手札が上限の{RequiredSlotCount}枚を超えています。現在{count}枚です。", this);
            return;
        }

        if (combatSelectionMode)
        {
            pendingBoundCards.Clear();
            if (cards != null) pendingBoundCards.AddRange(cards);
            hasPendingBoundCards = true;
            return;
        }

        boundCount = count;
        boundCards.Clear();
        if (cards != null) boundCards.AddRange(cards);
        for (int i = 0; i < slots.Count; i++)
        {
            bool used = i < count;
            var slot = slots[i];
            slot.gameObject.SetActive(used);
            if (used)
            {
                slot.Configure(this, gameController, i);
                slot.Bind(cards[i]);
                slot.SetInteractionEnabled(interactionEnabled);
            }
            else
            {
                slot.Unbind();
            }
        }

        RefreshLayout();
    }

    /// <summary>対象候補だけを手札枠へ再配置し、通常のカードドラッグを停止する。</summary>
    public void EnterSelectionMode(string title, IReadOnlyList<CardDisplayModel> candidateCards,
        int minimum, int maximum, bool canCancel)
    {
        combatSelectionMode = true;
        pendingBoundCards.Clear();
        hasPendingBoundCards = false;
        selectionMaximum = maximum;
        selectionSlots.Clear();
        if (selectionTitleText != null) selectionTitleText.text = title ?? string.Empty;
        if (selectionCountText != null) selectionCountText.text = $"0 / {maximum}";
        if (selectionOkButton != null) selectionOkButton.interactable = minimum == 0;
        if (selectionCancelButton != null) selectionCancelButton.gameObject.SetActive(canCancel);
        selectionHeaderRoot?.SetActive(true);

        int count = Mathf.Min(candidateCards?.Count ?? 0, slots.Count);
        boundCount = count;
        for (int index = 0; index < slots.Count; index++)
        {
            BattleHandCardSlotView slot = slots[index];
            bool visible = index < count;
            slot.gameObject.SetActive(visible);
            if (!visible)
            {
                slot.Unbind();
                continue;
            }
            CardDisplayModel model = candidateCards[index];
            int sourceIndex = model.Token.SourceIndex;
            CardBase card = sourceIndex >= 0 && sourceIndex < boundCards.Count ? boundCards[sourceIndex] : null;
            slot.Bind(card);
            slot.SetInteractionEnabled(true);
            slot.SetCombatSelectionMode(true, model.Token);
            selectionSlots[model.Token] = slot;
        }
        RefreshLayout();
    }

    /// <summary>選択数、OK可否、各カードのトグル表示を更新する。</summary>
    public void UpdateSelection(IReadOnlyCollection<CardSelectionToken> selectedTokens, bool canConfirm)
    {
        var selected = selectedTokens != null
            ? new HashSet<CardSelectionToken>(selectedTokens)
            : new HashSet<CardSelectionToken>();
        foreach (var entry in selectionSlots) entry.Value.SetCombatSelected(selected.Contains(entry.Key));
        if (selectionCountText != null) selectionCountText.text = $"{selected.Count} / {selectionMaximum}";
        if (selectionOkButton != null) selectionOkButton.interactable = canConfirm;
    }

    /// <summary>戦闘選択表示を閉じ、選択開始前の通常手札を復元する。</summary>
    public void ExitSelectionMode()
    {
        foreach (var entry in selectionSlots) entry.Value.SetCombatSelectionMode(false, entry.Key);
        selectionSlots.Clear();
        combatSelectionMode = false;
        selectionHeaderRoot?.SetActive(false);
        CardBase[] restoreCards = (hasPendingBoundCards ? pendingBoundCards : boundCards).ToArray();
        pendingBoundCards.Clear();
        hasPendingBoundCards = false;
        ShowCards(restoreCards);
    }

    /// <summary>手札枠クリックを戦闘選択Coordinatorへ通知する。</summary>
    public void NotifyCombatSelectionToggle(CardSelectionToken token)
    {
        if (combatSelectionMode) CardSelectionToggled?.Invoke(token);
    }

    private void NotifyConfirmRequested() => ConfirmRequested?.Invoke();
    private void NotifyCancelRequested() => CancelRequested?.Invoke();

    /// <summary>敵行動中やモーダル表示中の手札操作可否を一括変更する。</summary>
    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
        for (int i = 0; i < boundCount; i++)
        {
            slots[i]?.SetInteractionEnabled(enabled);
        }
    }

    /// <summary>ターン進行などで変化したカード表示を、割り当てを変えずに更新する。</summary>
    public void RefreshCardViews()
    {
        for (int i = 0; i < boundCount; i++)
            slots[i]?.CardView?.RefreshView();
    }

    /// <summary>マスク外の拡大カード上でも、既存の手札スクロールへホイール入力を渡す。</summary>
    public void OnScroll(PointerEventData eventData) => scrollRect?.OnScroll(eventData);

    private void OnRectTransformDimensionsChange()
    {
        RefreshLayout();
    }

    /// <summary>現在の枚数と表示幅からカードの位置とContent幅を再計算する。</summary>
    public void RefreshLayout()
    {
        if (content == null || viewport == null)
        {
            return;
        }

        float viewportWidth = Mathf.Max(1f, viewport.rect.width);
        bool useScroll = ShouldUseScroll(boundCount, scrollThreshold)
            || cardWidth + minimumVisibleWidth * Mathf.Max(0, boundCount - 1) + sidePadding * 2f > viewportWidth;
        float step = CalculateCardStep(boundCount, viewportWidth, cardWidth, minimumVisibleWidth,
            cardOverlap, scrollCardStep, sidePadding, useScroll);
        float contentWidth = boundCount <= 0
            ? viewportWidth
            : Mathf.Max(viewportWidth, sidePadding * 2f + cardWidth + step * (boundCount - 1));

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        if (scrollRect != null)
        {
            scrollRect.horizontal = useScroll;
            scrollRect.vertical = false;
            if (!useScroll)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        float occupiedWidth = boundCount <= 0 ? 0f : cardWidth + step * (boundCount - 1);
        float startX = useScroll ? sidePadding : Mathf.Max(sidePadding, (viewportWidth - occupiedWidth) * 0.5f);
        for (int i = 0; i < boundCount; i++)
        {
            slots[i].ApplyLayout(new Vector2(startX + step * i, bottomPadding), i);
        }
    }

    /// <summary>指定枚数が横スクロールへ切り替わるか返す。</summary>
    public static bool ShouldUseScroll(int cardCount, int threshold)
    {
        return cardCount > Mathf.Max(1, threshold);
    }

    /// <summary>カード識別部が最低幅を残す範囲で配置間隔を計算する。</summary>
    public static float CalculateCardStep(int cardCount, float viewportWidth, float width, float minimumWidth,
        float overlap, float scrollStep, float padding, bool useScroll)
    {
        if (cardCount <= 1)
        {
            return 0f;
        }

        if (useScroll)
        {
            return Mathf.Max(minimumWidth, scrollStep);
        }

        float available = Mathf.Max(0f, viewportWidth - padding * 2f - width);
        float fittingStep = available / (cardCount - 1);
        float preferredStep = width + Mathf.Max(0f, padding * 0.5f);
        return Mathf.Max(minimumWidth, Mathf.Min(preferredStep, fittingStep));
    }

    /// <summary>Inspector参照と50枠が揃っているか検証する。</summary>
    public bool ValidateReferences()
    {
        if (scrollRect == null || viewport == null || content == null || focusedCardRoot == null
            || focusedCardRoot.IsChildOf(viewport) || selectionHeaderRoot == null
            || selectionTitleText == null || selectionCountText == null || selectionOkButton == null
            || selectionCancelButton == null)
        {
            Debug.LogError("HandView: ScrollRect、マスク外の拡大表示先、選択操作UI、または必須参照が不足しています。", this);
            return false;
        }

        if (slots == null || slots.Count != RequiredSlotCount)
        {
            Debug.LogError($"HandView: 事前配置手札スロットは{RequiredSlotCount}枠必要です。", this);
            return false;
        }

        return true;
    }
}
