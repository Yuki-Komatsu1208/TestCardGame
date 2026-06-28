using TMPro;
using UnityEngine;
using TestCardGame.Controller;
using TestCardGame.Cards.Core;
using System.Collections.Generic;

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
            nameText.text = $"{card.CardName} (Lv.{card.Level.Level})";
            costText.text = $"M:{card.GetCost(null).Amount} CT:{card.GetCooldown(null).Turns}";
            
            string desc = card.IsCoolingDown
                ? $"{card.Description}\n残りCT: {card.RemainingCooldown.Turns}"
                : card.Description;

            if (card.EnchantDefinitions != null && card.EnchantDefinitions.Count > 0)
            {
                var modNames = new List<string>();
                foreach (var modifierDefinition in card.EnchantDefinitions)
                {
                    if (modifierDefinition != null)
                    {
                        modNames.Add(modifierDefinition.DisplayName);
                    }
                }
                desc += $"\n<color=cyan>[MOD: {string.Join(", ", modNames)}]</color>";
            }

            descriptionText.text = desc;

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
