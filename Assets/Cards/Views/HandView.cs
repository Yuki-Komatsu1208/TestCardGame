using System.Collections.Generic;
using TestCardGame.Cards;
using TestCardGame.Cards.Views;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform handArea;
    [SerializeField] private float cardSpacing = 16f;
    [SerializeField] private float bottomPadding = 12f;
    [SerializeField] private float sidePadding = 16f;

    public void ShowCards(IReadOnlyList<CardBase> cards)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("HandView: cardPrefab is not assigned.", this);
            return;
        }

        var parent = handArea != null ? handArea : transform;
        ClearCards(parent);

        if (cards == null || cards.Count == 0)
        {
            return;
        }

        foreach (var card in cards)
        {
            var view = Instantiate(cardPrefab, parent);
            view.Bind(card);
        }

        LayoutCards(parent);
    }

    private static void ClearCards(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    private void LayoutCards(Transform parent)
    {
        if (parent is not RectTransform parentRect)
        {
            return;
        }

        var childRects = new List<RectTransform>(parent.childCount);
        for (var i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) is RectTransform childRect)
            {
                childRects.Add(childRect);
            }
        }

        if (childRects.Count == 0)
        {
            return;
        }

        var totalWidth = 0f;
        foreach (var childRect in childRects)
        {
            totalWidth += childRect.rect.width;
        }
        totalWidth += cardSpacing * Mathf.Max(0, childRects.Count - 1);

        var startX = Mathf.Max(sidePadding, (parentRect.rect.width - totalWidth) * 0.5f);
        var currentX = startX;

        foreach (var childRect in childRects)
        {
            childRect.anchorMin = new Vector2(0f, 0f);
            childRect.anchorMax = new Vector2(0f, 0f);
            childRect.pivot = new Vector2(0f, 0f);
            childRect.anchoredPosition = new Vector2(currentX, bottomPadding);

            currentX += childRect.rect.width + cardSpacing;
        }
    }
}
