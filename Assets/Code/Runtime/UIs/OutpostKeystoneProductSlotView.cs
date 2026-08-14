using System;
using TMPro;
using TestCardGame.Run;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>キーストーン商品1枠を表示し、IDだけを通知する。</summary>
    public sealed class OutpostKeystoneProductSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject soldOutTag;
        [SerializeField] private GameObject unavailableTag;
        [SerializeField] private Button button;
        private KeystoneId id;
        private Action<KeystoneId> selected;
        private void Awake() => button.onClick.AddListener(NotifySelected);
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(NotifySelected); }
        private void NotifySelected() => selected?.Invoke(id);
        public void Bind(Action<KeystoneId> callback) => selected = callback;
        public void Render(KeystoneDisplayModel model)
        { id = model.Id; nameText.text = model.Name; descriptionText.text = model.Description; priceText.text = $"{model.Price} G"; soldOutTag.SetActive(model.IsSoldOut); unavailableTag.SetActive(model.IsUnavailable); button.interactable = model.IsSelectable; gameObject.SetActive(true); }
        public bool ValidateReferences() => nameText != null && descriptionText != null && priceText != null && soldOutTag != null && unavailableTag != null && button != null;
    }
}
