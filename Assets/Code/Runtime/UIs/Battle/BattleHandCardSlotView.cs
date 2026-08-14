using TestCardGame.Cards.Core;
using TestCardGame.Cards.Views;
using TestCardGame.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
using TestCardGame.UIs.Cards;

namespace TestCardGame.UIs.Battle
{
/// <summary>Battle手札の1枠を表示し、フォーカス時の拡大と前面化を行う。</summary>
public sealed class BattleHandCardSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Min(1f)] private float focusedScale = 1.18f;
    [SerializeField, Min(0f)] private float focusedYOffset = 42f;

    private HandView owner;
    private RectTransform rectTransform;
    private Vector2 layoutPosition;
    private int layoutSiblingIndex;
    private bool selected;
    private bool combatSelectionMode;
    private bool combatSelected;
    private CardSelectionToken combatToken;
    private MoveCardDragHandler dragHandler;

    public CardView CardView => cardView;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dragHandler = GetComponent<MoveCardDragHandler>();
    }

    /// <summary>親手札、操作Controller、安定したスロット番号を設定する。</summary>
    public void Configure(HandView handView, GameController controller, int slotIndex)
    {
        owner = handView;
        if (cardView != null)
        {
            cardView.ConfigureInteraction(controller);
        }
    }

    /// <summary>カード実体をこの事前配置枠へ表示する。</summary>
    public void Bind(CardBase card)
    {
        cardView?.Bind(card);
        selected = false;
        SetFocused(false);
    }

    /// <summary>未使用枠のカード参照を解除する。</summary>
    public void Unbind()
    {
        cardView?.Bind(null);
        selected = false;
    }

    /// <summary>操作可否をCanvasGroupへ反映する。</summary>
    public void SetInteractionEnabled(bool enabled)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = enabled;
            canvasGroup.blocksRaycasts = enabled;
            canvasGroup.alpha = enabled ? 1f : 0.72f;
        }
    }

    /// <summary>親Viewで計算された通常位置とSibling順を適用する。</summary>
    public void ApplyLayout(Vector2 position, int siblingIndex)
    {
        rectTransform ??= GetComponent<RectTransform>();
        layoutPosition = position;
        layoutSiblingIndex = siblingIndex;
        if (!selected)
        {
            rectTransform.anchoredPosition = layoutPosition;
            rectTransform.localScale = Vector3.one;
            transform.SetSiblingIndex(layoutSiblingIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => SetFocused(true);

    public void OnPointerExit(PointerEventData eventData)
    {
        if (combatSelectionMode)
        {
            if (!combatSelected) SetFocused(false);
            return;
        }
        if (!selected)
        {
            SetFocused(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (combatSelectionMode)
        {
            SetFocused(true);
            return;
        }
        selected = true;
        SetFocused(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (combatSelectionMode)
        {
            owner?.NotifyCombatSelectionToggle(combatToken);
            SetFocused(combatSelected);
            return;
        }
        selected = false;
        SetFocused(false);
        owner?.RefreshLayout();
    }

    /// <summary>通常ドラッグを止め、戦闘中カード選択用のトグル操作へ切り替える。</summary>
    public void SetCombatSelectionMode(bool enabled, CardSelectionToken token)
    {
        combatSelectionMode = enabled;
        combatToken = token;
        combatSelected = false;
        dragHandler ??= GetComponent<MoveCardDragHandler>();
        if (dragHandler != null) dragHandler.enabled = !enabled;
        SetFocused(false);
    }

    /// <summary>選択済みカードをフォーカス表示として維持する。</summary>
    public void SetCombatSelected(bool value)
    {
        combatSelected = value;
        SetFocused(value);
    }

    /// <summary>フォーカス時だけカードを上へ移動・拡大し、最前面へ出す。</summary>
    private void SetFocused(bool focused)
    {
        rectTransform ??= GetComponent<RectTransform>();
        if (focused)
        {
            rectTransform.anchoredPosition = layoutPosition + Vector2.up * focusedYOffset;
            rectTransform.localScale = Vector3.one * focusedScale;
            transform.SetAsLastSibling();
        }
        else
        {
            rectTransform.anchoredPosition = layoutPosition;
            rectTransform.localScale = Vector3.one;
            transform.SetSiblingIndex(Mathf.Clamp(layoutSiblingIndex, 0, transform.parent.childCount - 1));
        }
    }
}
}
