using System;
using TMPro;
using TestCardGame.Character.Enemies;
using TestCardGame.Character.ValueObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>敵の頭上記号、選択枠、クリック通知だけを担当する。</summary>
    public sealed class EnemyIntentView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI intentText;
        [SerializeField] private Image selectionBorder;

        private UnitID enemyId;
        private bool bound;

        public event Action<UnitID> EnemySelected;

        /// <summary>対応する敵IDを保持し、頭上表示を有効にする。</summary>
        public void Bind(UnitID id)
        {
            enemyId = id;
            bound = id != null;
            if (intentText != null) intentText.gameObject.SetActive(bound);
        }

        /// <summary>敵ではないテンプレート本体の意図表示を隠す。</summary>
        public void DisableIntent()
        {
            bound = false;
            enemyId = null;
            if (intentText != null) intentText.gameObject.SetActive(false);
            SetSelected(false);
        }

        /// <summary>バックエンドが明示した分類を記号へ変換して表示する。</summary>
        public void ShowIntent(EnemyIntentPreview preview)
        {
            if (intentText == null || preview == null) return;
            intentText.gameObject.SetActive(true);
            intentText.text = !preview.CanExecute ? "—" : preview.Kind switch
            {
                EnemyIntentKind.Attack => "⚔",
                EnemyIntentKind.StatusEffect => "↻",
                EnemyIntentKind.Other => "?",
                _ => "—"
            };
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
