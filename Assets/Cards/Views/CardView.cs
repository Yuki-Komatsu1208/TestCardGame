using TMPro;
using UnityEngine;

namespace TestCardGame.Cards.Views
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private CardBase card;

        public void Bind(CardBase card)
        {
            this.card = card;
            nameText.text = card.CardName;
            costText.text = card.Cost.ToString();
            descriptionText.text = card.Description;
        }

        public CardBase Card => card;
    }
}