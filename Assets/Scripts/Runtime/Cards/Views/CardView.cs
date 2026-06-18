using TMPro;
using UnityEngine;
using TestCardGame.Controller;
using TestCardGame.Cards.Core;

namespace TestCardGame.Cards.Views
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private CardBase card;

        /// <summary>
        /// カード実体の情報をUI表示へ反映する。
        /// </summary>
        public void Bind(CardBase card)
        {
            this.card = card;
            nameText.text = card.CardName;
            costText.text = card.Cost.ToString();
            descriptionText.text = card.Description;

            if (TryGetComponent<MoveCardDragHandler>(out var dragHandler))
            {
                dragHandler.Configure(
                    FindAnyObjectByType<GameController>(),
                    Mathf.Max(1, (int)card.Level),
                    40f,
                    GetComponent<RectTransform>());
            }
        }

        public CardBase Card => card;
    }
}
