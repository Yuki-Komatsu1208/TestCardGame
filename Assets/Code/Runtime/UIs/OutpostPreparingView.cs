using TMPro;
using UnityEngine;

namespace TestCardGame.UIs
{
    public sealed class OutpostPreparingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text facilityText;
        [SerializeField] private TMP_Text messageText;
        public void Render(string facility, string message) { facilityText.text = facility; messageText.text = message; }
        public bool ValidateReferences() => facilityText != null && messageText != null;
    }
}
