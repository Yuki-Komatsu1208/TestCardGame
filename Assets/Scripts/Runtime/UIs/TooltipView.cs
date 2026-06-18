using TMPro;
using UnityEngine;

public class TooltipView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI tooltipText;

    public void Show(string message, Vector3 worldPosition)
    {
        root.SetActive(true);
        tooltipText.text = message;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        root.transform.position = screenPos + new Vector3(80f, 40f, 0f);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}