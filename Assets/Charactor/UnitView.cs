using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.Charactor
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (image == null)
            {
                image = GetComponent<Image>();
            }
        }

        public void Initialize(Sprite fallbackSprite)
        {
            if (image == null)
            {
                return;
            }

            image.enabled = true;
            if (image.sprite == null)
            {
                image.sprite = fallbackSprite;
            }
        }

        public void MoveToCell(RectTransform cellRect)
        {
            if (rectTransform == null || cellRect == null)
            {
                return;
            }

            rectTransform.SetParent(cellRect, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.SetAsLastSibling();
        }
    }
}
