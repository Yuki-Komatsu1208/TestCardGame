using TestCardGame.Controller;
using TestCardGame.Cards.Views;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveCardDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private GameController gameController;
    [SerializeField] private int moveCount = 1;
    [SerializeField] private float dragThreshold = 40f;
    [SerializeField] private RectTransform cardRect;

    private Canvas parentCanvas;
    private Vector2 initialAnchoredPos;
    private Vector2 pointerDownScreenPos;
    private bool isDragging;
    private CardView cardView;
    private HandView handView;

    /// <summary>
    /// 必要な参照を取得し、カードの初期位置を記録する。
    /// </summary>
    private void Awake()
    {
        if (cardRect == null)
        {
            cardRect = GetComponent<RectTransform>();
        }

        if (gameController == null)
        {
            gameController = FindAnyObjectByType<GameController>();
        }

        parentCanvas = GetComponentInParent<Canvas>();
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
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
        ResetCardPosition();

        if (gameController == null)
        {
            gameController = FindAnyObjectByType<GameController>();
        }

        if (gameController == null)
        {
            Debug.LogError("MoveCardDragHandler: シーン内に GameController が見つかりません。", this);
            return;
        }

        if (cardView != null && cardView.Card != null)
        {
            gameController.UseCardAtDropScreenPosition(cardView.Card, eventData.position);
        }
        else
        {
            gameController.RequestPlayerMoveByDropScreenPosition(eventData.position, moveCount);
        }
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
        }
    }
}
