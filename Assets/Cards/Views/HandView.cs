using System.Collections.Generic;
using TestCardGame.Cards;
using TestCardGame.Cards.Views;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform handArea;

    public void ShowCards(List<CardBase> cards)
    {
        foreach (var card in cards)
        {
            var view = Instantiate(cardPrefab, handArea);
            view.Bind(card);
        }
    }
}