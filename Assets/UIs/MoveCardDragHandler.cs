using TestCardGame.Controller;
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

    private void Awake()
    {
        if (cardRect == null)
        {
            cardRect = GetComponent<RectTransform>();
        }

        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }

        parentCanvas = GetComponentInParent<Canvas>();
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
        }

        pointerDownScreenPos = eventData.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || cardRect == null)
        {
            return;
        }

        var scale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        cardRect.anchoredPosition += eventData.delta / scale;
    }

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
            gameController = FindFirstObjectByType<GameController>();
        }

        if (gameController == null)
        {
            Debug.LogError("MoveCardDragHandler: GameController was not found in scene.", this);
            return;
        }

        gameController.RequestPlayerMoveByDropScreenPosition(eventData.position, moveCount);
    }

    private void ResetCardPosition()
    {
        if (cardRect == null)
        {
            return;
        }

        cardRect.anchoredPosition = initialAnchoredPos;
    }

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
        if (cardRect != null)
        {
            initialAnchoredPos = cardRect.anchoredPosition;
        }
    }
}
