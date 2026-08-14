using TestCardGame.Controller;
using TestCardGame.Cards.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using TestCardGame.UIs.Battle;

public class MoveCardDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private GameController gameController;
    [SerializeField] private int moveCount = 1;
    [SerializeField] private float dragThreshold = 40f;
    [SerializeField] private RectTransform cardRect;
    [SerializeField, Range(0.2f, 1f)] private float dragScale = 0.68f;

    private Canvas parentCanvas;
    private Vector2 initialAnchoredPos;
    private Vector2 pointerDownScreenPos;
    private bool isDragging;
    private CardView cardView;
    private HandView handView;
    private Vector3 initialScale;
    private int cardIndex = -1;

    /// <summary>
    /// 必要な参照を取得し、カードの初期位置を記録する。
    /// </summary>
    private void Awake()
    {
        if (cardRect == null)
        {
            cardRect = GetComponent<RectTransform>();
        }

        parentCanvas = GetComponentInParent<Canvas>();
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
            initialScale = cardRect.localScale;
        }

        cardView = GetComponent<CardView>();
        handView = GetComponentInParent<HandView>();
    }

    /// <summary>
    /// ポインター押下時にドラッグ開始位置を記録する。
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
        }

        pointerDownScreenPos = eventData.position;
        isDragging = true;
        cardIndex = ResolveCardIndex();
        if (cardRect != null) cardRect.localScale = initialScale * dragScale;
        (gameController as ICardDragPreviewController)?.BeginPreview(cardIndex);
    }

    /// <summary>
    /// ドラッグ中のカードUI位置をポインター移動に追従させる。
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || cardRect == null)
        {
            return;
        }

        var scale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        cardRect.anchoredPosition += eventData.delta / scale;
        (gameController as ICardDragPreviewController)?.UpdatePreview(cardIndex, eventData.position);
    }

    /// <summary>
    /// ドロップ位置に応じてカード使用または移動リクエストを実行する。
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        var dragDistance = (eventData.position - pointerDownScreenPos).magnitude;
        if (dragDistance < dragThreshold)
        {
            ResetCardPosition();
            return;
        }
        if (gameController == null)
        {
            Debug.LogError("MoveCardDragHandler: シーン内に GameController が見つかりません。", this);
            ResetCardPosition();
            return;
        }

        if (cardView != null && cardView.Card != null)
            (gameController as ICardDragPreviewController)?.Commit(cardIndex, eventData.position);
        else
            gameController.RequestPlayerMoveByDropScreenPosition(eventData.position, moveCount);
        ResetCardPosition();
    }

    /// <summary>
    /// カードUIをドラッグ開始前の位置へ戻す。
    /// </summary>
    private void ResetCardPosition()
    {
        if (cardRect == null)
        {
            return;
        }

        cardRect.anchoredPosition = initialAnchoredPos;
        cardRect.localScale = initialScale;
        (gameController as ICardDragPreviewController)?.Cancel(cardIndex);
        handView?.RefreshLayout();
    }

    /// <summary>
    /// ドラッグ操作に必要な参照とパラメータを外部から設定する。
    /// </summary>
    public void Configure(GameController controller, int count, float threshold, RectTransform rect = null)
    {
        gameController = controller;
        moveCount = Mathf.Max(1, count);
        dragThreshold = Mathf.Max(1f, threshold);

        if (rect != null)
        {
            cardRect = rect;
        }
        else if (cardRect == null)
        {
            cardRect = GetComponent<RectTransform>();
        }

        parentCanvas = GetComponentInParent<Canvas>();
        handView = GetComponentInParent<HandView>();
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
            initialScale = cardRect.localScale;
        }
    }

    private int ResolveCardIndex()
    {
        if (gameController == null || cardView?.Card == null) return -1;
        var cards = gameController.GetPlayerCards();
        for (int i = 0; i < cards.Count; i++) if (ReferenceEquals(cards[i], cardView.Card)) return i;
        return -1;
    }

    private void OnDisable()
    {
        if (isDragging) { isDragging = false; ResetCardPosition(); }
    }
}
