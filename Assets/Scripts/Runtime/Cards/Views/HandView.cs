using System.Collections.Generic;
using TestCardGame.Cards;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Views;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform handArea;
    [SerializeField] private float cardSpacing = 16f;
    [SerializeField] private float bottomPadding = 12f;
    [SerializeField] private float sidePadding = 16f;

    /// <summary>
    /// 渡されたカード一覧を手札UIとして表示する。
    /// </summary>
    public void ShowCards(IReadOnlyList<CardBase> cards)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("HandView: cardPrefab が設定されていません。", this);
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

    /// <summary>
    /// 現在の手札UIを再レイアウトする。
    /// </summary>
    public void RefreshLayout()
    {
        var parent = handArea != null ? handArea : transform;
        LayoutCards(parent);
    }

    /// <summary>
    /// 手札表示領域の既存カードUIをすべて削除する。
    /// </summary>
    private static void ClearCards(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 生成済みカードUIを手札エリア内に横並びで配置する。
    /// </summary>
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
