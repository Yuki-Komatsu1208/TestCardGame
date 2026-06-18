using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CellHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string cellName;
    [SerializeField] private string description;

    private TooltipView _tooltipView;

    /// <summary>
    /// シーン内のツールチップ表示を取得する。
    /// </summary>
    private void Start()
    {
        _tooltipView = FindAnyObjectByType<TooltipView>();
    }

    /// <summary>
    /// セルにマウスが乗ったときに説明ツールチップを表示する。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltipView == null) return;

        Vector3 screenPos = Input.mousePosition + new Vector3(80f, 40f, 0f);
        _tooltipView.Show($"{cellName}\n{description}", screenPos);
    }

    /// <summary>
    /// セルからマウスが離れたときにツールチップを閉じる。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltipView == null) return;
        _tooltipView.Hide();
    }
}
