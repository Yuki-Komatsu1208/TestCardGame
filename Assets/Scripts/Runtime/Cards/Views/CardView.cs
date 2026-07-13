using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Image cooldownOverlayImage;

        private CardBase card;
        private Graphic[] tintTargets;
        private Color[] originalColors;

        private void Awake()
        {
            tintTargets = GetComponentsInChildren<Graphic>(true);
            originalColors = new Color[tintTargets.Length];
            for (int i = 0; i < tintTargets.Length; i++)
            {
                originalColors[i] = tintTargets[i].color;
            }
        }

        /// <summary>
        /// カード実体の情報をUI表示へ反映する。
        /// </summary>
        public void Bind(CardBase card)
        {
            this.card = card;
            RefreshView();
        }

        private void LateUpdate()
        {
            if (card == null)
            {
                return;
            }

            RefreshView();
        }

        private void RefreshView()
        {
            if (card == null)
            {
                return;
            }

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

            ApplyCooldownTint();

            if (TryGetComponent<MoveCardDragHandler>(out var dragHandler))
            {
                dragHandler.Configure(
                    FindAnyObjectByType<GameController>(),
                    Mathf.Max(1, (int)card.Level),
                    40f,
                    GetComponent<RectTransform>());
            }
        }

        private void ApplyCooldownTint()
        {
            int totalCooldown = card.GetCooldown(null).Turns;
            int remainingCooldown = card.RemainingCooldown.Turns;
            float ratio = card.IsCoolingDown && totalCooldown > 0 && remainingCooldown > 0
                ? Mathf.Clamp01((float)remainingCooldown / totalCooldown)
                : 0f;

            if (cooldownOverlayImage != null)
            {
                cooldownOverlayImage.gameObject.SetActive(false);
            }

            if (tintTargets == null || originalColors == null)
            {
                return;
            }

            for (int i = 0; i < tintTargets.Length; i++)
            {
                var target = tintTargets[i];
                if (target == null)
                {
                    continue;
                }

                var original = originalColors[i];
                float gray = original.grayscale;
                var cooledColor = new Color(gray * 0.72f, gray * 0.72f, gray * 0.72f, original.a);
                target.color = Color.Lerp(original, cooledColor, ratio);
            }
        }

        public CardBase Card => card;
    }
}
