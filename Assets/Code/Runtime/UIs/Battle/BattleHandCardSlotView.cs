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
    [SerializeField, Min(1f)] private float focusedScale = 1.6f;

    private HandView owner;
    private RectTransform rectTransform;
    private Transform layoutParent;
    private readonly Vector3[] corners = new Vector3[4];
    private Vector2 layoutPosition;
    private int layoutSiblingIndex;
    private bool selected;
    private bool restoreFocusOnNextFrame;
    private bool combatSelectionMode;
    private bool combatSelected;
    private CardSelectionToken combatToken;
    private MoveCardDragHandler dragHandler;

    public CardView CardView => cardView;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutParent = transform.parent;
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
        SetFocused(false);
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
            SetFocused(false);
        }
    }

    /// <summary>押下中の再ホバーでは、運搬用の縮小とドラッグ位置を上書きしない。</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected) SetFocused(true);
    }

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

    /// <summary>フォーカス時はマスク外へ移して前面に表示し、解除時に元の手札枠へ戻す。</summary>
    private void SetFocused(bool focused)
    {
        restoreFocusOnNextFrame = false;
        rectTransform ??= GetComponent<RectTransform>();
        layoutParent ??= transform.parent;
        if (layoutParent == null) return;
        if (rectTransform.parent != layoutParent) rectTransform.SetParent(layoutParent, false);
        if (focused)
        {
            // Pivotの位置に関わらず、拡大前の下端を維持する。
            rectTransform.anchoredPosition = layoutPosition
                + Vector2.up * rectTransform.rect.height * rectTransform.pivot.y * (focusedScale - 1f);
            rectTransform.localScale = Vector3.one * focusedScale;
            if (owner != null && owner.FocusedCardRoot != null)
                rectTransform.SetParent(owner.FocusedCardRoot, true);
            transform.SetAsLastSibling();
            ClampToCanvasHorizontally();
        }
        else
        {
            rectTransform.anchoredPosition = layoutPosition;
            rectTransform.localScale = Vector3.one;
            transform.SetSiblingIndex(Mathf.Clamp(layoutSiblingIndex, 0, transform.parent.childCount - 1));
        }
    }

    /// <summary>下端の高さを変えず、画面の左右端からはみ出す分だけ補正する。</summary>
    private void ClampToCanvasHorizontally()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var bounds = (RectTransform)canvas.rootCanvas.transform;
        rectTransform.GetWorldCorners(corners);
        Vector3 min = bounds.InverseTransformPoint(corners[0]);
        Vector3 max = bounds.InverseTransformPoint(corners[2]);
        float x = min.x < bounds.rect.xMin + 8f ? bounds.rect.xMin + 8f - min.x
            : max.x > bounds.rect.xMax - 8f ? bounds.rect.xMax - 8f - max.x : 0f;
        rectTransform.position += bounds.TransformVector(new Vector3(x, 0f, 0f));
    }

    /// <summary>親の有効状態が切り替わる間の階層変更を避け、次の表示時に復元する。</summary>
    private void OnDisable()
    {
        selected = false;
        restoreFocusOnNextFrame = true;
    }

    /// <summary>再表示時は有効化処理が完了したフレームで通常の配置へ戻す。</summary>
    private void LateUpdate()
    {
        if (restoreFocusOnNextFrame) SetFocused(false);
    }

}
}
