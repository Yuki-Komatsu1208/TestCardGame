using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    public sealed class OutpostInnView : MonoBehaviour
    {
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private Button useButton;
        public Button UseButton => useButton;
        public void Render(InnDisplayModel model) { hpText.text = $"HP  {model.CurrentHp} / {model.MaxHp}  →  {model.HealedHp} / {model.MaxHp}"; costText.text = $"必要ゴールド  {model.Cost} G"; reasonText.text = model.UnavailableReason; useButton.interactable = model.IsAvailable; }
        public bool ValidateReferences() => hpText != null && costText != null && reasonText != null && useButton != null;
    }
}
