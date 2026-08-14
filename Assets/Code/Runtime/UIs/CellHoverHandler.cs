using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CellHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string cellName;
    [SerializeField] private string description;

    [SerializeField] private TooltipView tooltipView;

    /// <summary>生成済みセルへツールチップ表示先を明示的に渡す。</summary>
    public void Configure(TooltipView view) => tooltipView = view;

    /// <summary>
    /// セルにマウスが乗ったときに説明ツールチップを表示する。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipView == null) return;

        Vector3 screenPos = Input.mousePosition + new Vector3(80f, 40f, 0f);
        tooltipView.Show($"{cellName}\n{description}", screenPos);
    }

    /// <summary>
    /// セルからマウスが離れたときにツールチップを閉じる。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipView == null) return;
        tooltipView.Hide();
    }
}
