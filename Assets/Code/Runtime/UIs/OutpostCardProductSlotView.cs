using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>ショップのカード商品1枠を表示し、商品番号だけを通知する。</summary>
    public sealed class OutpostCardProductSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject saleTag;
        [SerializeField] private GameObject soldOutTag;
        [SerializeField] private Button button;
        private int index;
        private Action<int> selected;
        private void Awake() => button.onClick.AddListener(NotifySelected);
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(NotifySelected); }
        private void NotifySelected() => selected?.Invoke(index);
        public void Bind(Action<int> callback) => selected = callback;
        public void Render(ShopCardDisplayModel model)
        { index = model.Index; nameText.text = model.Name; priceText.text = $"{model.Price} G"; saleTag.SetActive(model.IsOnSale && !model.IsSoldOut); soldOutTag.SetActive(model.IsSoldOut); button.interactable = model.IsSelectable && !model.IsSoldOut; gameObject.SetActive(true); }
        public bool ValidateReferences() => nameText != null && priceText != null && saleTag != null && soldOutTag != null && button != null;
    }
}
