using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>Battle常設HUDの表示とボタン入力通知だけを担当するView。</summary>
    public sealed class BattleHudView : MonoBehaviour
    {
        [SerializeField] private bool compactLayout;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private UnitHudSlotView playerSlot;
        public bool IsCompactLayout => compactLayout;
        [SerializeField] private List<UnitHudSlotView> enemySlots = new();
        [SerializeField] private ScrollRect enemyScrollRect;
        [SerializeField] private RectTransform enemyContent;
        [SerializeField, Min(1f)] private float enemySlotWidth = 252f;
        [SerializeField, Min(1)] private int visibleEnemySlotCount = 4;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text expeditionText;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text overHuntText;
        [SerializeField] private TMP_Text pendingGoldText;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button deckButton;
        [SerializeField] private Color normalExpeditionColor = new(0.12f, 0.10f, 0.08f, 1f);
        [SerializeField] private Color overHuntColor = new(0.72f, 0.08f, 0.08f, 1f);

        public event Action EndTurnRequested;
        public event Action DeckRequested;
        public IReadOnlyList<UnitHudSlotView> EnemySlots => enemySlots;

        private void OnEnable()
        {
            if (endTurnButton != null) endTurnButton.onClick.AddListener(NotifyEndTurnRequested);
            if (deckButton != null) deckButton.onClick.AddListener(NotifyDeckRequested);
        }

        private void OnDisable()
        {
            if (endTurnButton != null) endTurnButton.onClick.RemoveListener(NotifyEndTurnRequested);
            if (deckButton != null) deckButton.onClick.RemoveListener(NotifyDeckRequested);
        }

        /// <summary>表示モデルを事前配置済みHUDへ反映する。</summary>
        public void Render(BattleHudViewModel model)
        {
            if (model == null) return;
            if (compactLayout)
            {
                if (playerSlot != null) playerSlot.gameObject.SetActive(false);
                if (enemyScrollRect != null) enemyScrollRect.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                playerSlot?.Render(model.Player);
                int enemyCount = model.Enemies?.Count ?? 0;
                if (enemyCount > enemySlots.Count)
                {
                    Debug.LogError($"BattleHudView: 敵HUD枠が不足しています。必要{enemyCount}枠、設定{enemySlots.Count}枠です。", this);
                }
                for (int i = 0; i < enemySlots.Count; i++)
                {
                    enemySlots[i]?.Render(i < enemyCount ? model.Enemies[i] : null);
                }
                UpdateEnemyScroll(enemyCount);
            }
            if (manaText != null) manaText.text = model.Player != null
                ? $"マナ {model.Player.CurrentMana} / {model.Player.MaxMana}" : "マナ --";

            if (turnText != null) turnText.text = model.TurnLabel;
            if (expeditionText != null) expeditionText.text = model.ExpeditionLabel;
            if (stageText != null) stageText.text = model.StageLabel;
            if (overHuntText != null)
            {
                overHuntText.text = model.IsOverHunt ? $"OVERHUNT  深度 {model.OverHuntDepth}" : "通常遠征";
                overHuntText.color = model.IsOverHunt ? overHuntColor : normalExpeditionColor;
            }
            if (pendingGoldText != null) pendingGoldText.text = $"保留ゴールド {model.PendingGold} G";
            if (endTurnButton != null) endTurnButton.interactable = model.CanEndTurn;
            if (deckButton != null) deckButton.interactable = model.CanOpenDeck;
        }

        /// <summary>必須参照が揃っているか日本語エラーで検証する。</summary>
        public bool ValidateReferences()
        {
            bool valid = (compactLayout ? manaText != null : playerSlot != null && playerSlot.HasRequiredReferences()) && turnText != null
                && expeditionText != null && stageText != null && overHuntText != null && pendingGoldText != null
                && endTurnButton != null && deckButton != null
                && (compactLayout || (enemyScrollRect != null && enemyContent != null));
            if (!valid) Debug.LogError("BattleHudView: 必須のInspector参照が不足しています。", this);
            return valid;
        }

        private void NotifyEndTurnRequested() => EndTurnRequested?.Invoke();
        private void NotifyDeckRequested() => DeckRequested?.Invoke();

        /// <summary>実際の敵数だけContent幅を確保し、4体超のときだけ横スクロールを有効にする。</summary>
        private void UpdateEnemyScroll(int enemyCount)
        {
            if (enemyScrollRect == null || enemyContent == null) return;
            int displayedCount = Mathf.Min(enemyCount, enemySlots.Count);
            bool useScroll = displayedCount > visibleEnemySlotCount;
            float viewportWidth = enemyScrollRect.viewport != null ? enemyScrollRect.viewport.rect.width : 0f;
            float requiredWidth = enemySlotWidth * Mathf.Max(displayedCount, Mathf.Min(visibleEnemySlotCount, enemySlots.Count));
            enemyContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(viewportWidth, requiredWidth));
            enemyScrollRect.horizontal = useScroll;
            if (!useScroll) enemyScrollRect.horizontalNormalizedPosition = 0f;
        }
    }
}
