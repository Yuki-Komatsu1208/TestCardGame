using System;
using TMPro;
using TestCardGame.Character.Enemies;
using TestCardGame.Character.ValueObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>敵の行動予告の保持、選択枠、クリック通知を担当する。</summary>
    public sealed class EnemyIntentView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI intentText;
        [SerializeField] private Image selectionBorder;

        private UnitID enemyId;
        private bool bound;

        public event Action<UnitID> EnemySelected;
        public EnemyIntentPreview Preview { get; private set; }

        /// <summary>対応する敵IDを保持し、クリックによる選択を有効にする。</summary>
        public void Bind(UnitID id)
        {
            enemyId = id;
            bound = id != null;
            if (intentText != null) intentText.gameObject.SetActive(false);
        }

        /// <summary>敵ではないテンプレート本体の意図表示を隠す。</summary>
        public void DisableIntent()
        {
            bound = false;
            Preview = null;
            enemyId = null;
            if (intentText != null) intentText.gameObject.SetActive(false);
            SetSelected(false);
        }

        /// <summary>既存の予告を共有説明用に保持し、常設記号を隠す。</summary>
        public void ShowIntent(EnemyIntentPreview preview)
        {
            Preview = preview;
            if (intentText != null) intentText.gameObject.SetActive(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectionBorder != null) selectionBorder.gameObject.SetActive(selected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (bound) EnemySelected?.Invoke(enemyId);
        }

        public bool ValidateReferences() => intentText != null && selectionBorder != null;
    }
}
