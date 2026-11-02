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
        [Header("Card Template")]
        [SerializeField] private UnityEngine.UI.Image artworkImage;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField, Tooltip("丸形コスト欄と独立したレベル/CT表示を使う。旧Prefabでは無効のまま。")]
        private bool useTemplateLayout;

        private CardBase card;
        private Sprite artworkOverride;
        private Graphic[] tintTargets;
        private Color[] originalColors;

        private void Awake()
        {
            CacheTintColors();
        }

        private void CacheTintColors()
        {
            if (tintTargets != null) return;
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
            Bind(card, null);
        }

        /// <summary>単体テンプレートでは必要に応じて画像を上書きできる。</summary>
        public void Bind(CardBase card, Sprite illustrationOverride)
        {
            CacheTintColors();
            this.card = card;
            artworkOverride = illustrationOverride;
            if (card == null)
            {
                if (cooldownOverlayImage != null) cooldownOverlayImage.gameObject.SetActive(false);
                for (int i = 0; i < tintTargets.Length; i++)
                    if (tintTargets[i] != null) tintTargets[i].color = originalColors[i];
                if (nameText != null) nameText.text = string.Empty;
                if (costText != null) costText.text = string.Empty;
                if (descriptionText != null) descriptionText.text = string.Empty;
                if (levelText != null) levelText.text = string.Empty;
                if (cooldownText != null) cooldownText.text = string.Empty;
                if (artworkImage != null)
                {
                    artworkImage.sprite = null;
                    artworkImage.enabled = false;
                }
                return;
            }
            RefreshView();
        }

        /// <summary>カード状態変更時に表示内容を明示的に再描画する。</summary>
        public void RefreshView()
        {
            if (card == null)
            {
                return;
            }

            if (nameText != null) nameText.text = useTemplateLayout ? card.CardName : $"{card.CardName} (Lv.{card.Level.Level})";
            if (costText != null) costText.text = useTemplateLayout
                ? card.GetCost(null).Amount.ToString()
                : $"M:{card.GetCost(null).Amount} CT:{card.GetCooldown(null).Turns}";
            if (levelText != null) levelText.text = $"Lv.{card.Level.Level}";
            if (cooldownText != null) cooldownText.text = $"CT:{card.GetCooldown(null).Turns}";
            if (artworkImage != null)
            {
                artworkImage.sprite = artworkOverride != null ? artworkOverride : card.Definition.Artwork;
                artworkImage.enabled = artworkImage.sprite != null;
                artworkImage.preserveAspect = true;
            }

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

            if (descriptionText != null) descriptionText.text = desc;

            ApplyCooldownTint();
        }

        /// <summary>ドラッグ操作のController参照を初期化時に一度だけ渡す。</summary>
        public void ConfigureInteraction(GameController controller)
        {
            if (TryGetComponent<MoveCardDragHandler>(out var dragHandler))
            {
                dragHandler.Configure(controller, card != null ? Mathf.Max(1, (int)card.Level) : 1,
                    40f, GetComponent<RectTransform>());
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
