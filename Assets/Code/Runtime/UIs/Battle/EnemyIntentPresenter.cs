using System.Collections.Generic;
using TestCardGame.BoardManage;
using TestCardGame.Character.Enemies;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Controller;
using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    /// <summary>敵次行動の再計算、選択、盤面Overlayへの反映を仲介する。</summary>
    public sealed class EnemyIntentPresenter : MonoBehaviour
    {
        [SerializeField] private GameController controller;
        [SerializeField] private CellBuilder cellBuilder;
        [SerializeField] private BoardView boardView;

        private readonly Dictionary<UnitID, EnemyIntentPreview> previews = new();
        private readonly HashSet<EnemyIntentView> subscribedViews = new();
        private UnitID selectedEnemyId;
        private bool refreshRequested;
        private bool cardPreviewActive;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.BoardStateChanged += RequestRefresh;
                controller.CardPreviewStarted += OnCardPreviewStarted;
                controller.CardPreviewEnded += OnCardPreviewEnded;
            }
            if (boardView != null) boardView.BackgroundClicked += ClearSelection;
            RequestRefresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.BoardStateChanged -= RequestRefresh;
                controller.CardPreviewStarted -= OnCardPreviewStarted;
                controller.CardPreviewEnded -= OnCardPreviewEnded;
            }
            if (boardView != null) boardView.BackgroundClicked -= ClearSelection;
            foreach (EnemyIntentView view in subscribedViews) if (view != null) view.EnemySelected -= SelectEnemy;
            subscribedViews.Clear();
        }

        /// <summary>同一フレーム中の複数盤面通知を最終状態の1回へまとめる。</summary>
        private void RequestRefresh() => refreshRequested = true;

        private void LateUpdate()
        {
            if (!refreshRequested) return;
            refreshRequested = false;
            RefreshAll();
        }

        /// <summary>全生存敵の次行動を再問い合わせし、各Viewへ反映する。</summary>
        public void RefreshAll()
        {
            previews.Clear();
            bool selectedStillAlive = false;
            if (controller == null || cellBuilder == null) return;

            foreach (EnemyUnit enemy in cellBuilder.Enemies)
            {
                if (enemy == null || enemy.Hp.CurrentValue <= 0) continue;
                if (!cellBuilder.EnemyUnitViews.TryGetValue(enemy.ID, out var unitView) || unitView == null
                    || !unitView.TryGetComponent(out EnemyIntentView view)) continue;

                if (subscribedViews.Add(view)) view.EnemySelected += SelectEnemy;
                view.Bind(enemy.ID);
                if (controller.TryGetEnemyIntent(enemy, out EnemyIntentPreview preview))
                {
                    previews[enemy.ID] = preview;
                    view.ShowIntent(preview);
                }

                bool selected = selectedEnemyId != null && selectedEnemyId.Equals(enemy.ID);
                view.SetSelected(selected);
                selectedStillAlive |= selected;
            }

            if (!selectedStillAlive) selectedEnemyId = null;
            RestoreSelectedOverlay();
        }

        /// <summary>同じ敵なら解除、別の敵なら選択を移し、選択時だけ範囲を表示する。</summary>
        private void SelectEnemy(UnitID enemyId)
        {
            selectedEnemyId = selectedEnemyId != null && selectedEnemyId.Equals(enemyId) ? null : enemyId;
            RefreshAll();
        }

        private void ClearSelection()
        {
            selectedEnemyId = null;
            RefreshAll();
        }

        private void OnCardPreviewStarted()
        {
            cardPreviewActive = true;
            boardView?.Clear(CellOverlayKind.EnemyIntentRange);
        }

        private void OnCardPreviewEnded()
        {
            cardPreviewActive = false;
            RestoreSelectedOverlay();
        }

        /// <summary>カードPreview中でなければ、選択中の敵予告範囲だけを赤枠表示する。</summary>
        private void RestoreSelectedOverlay()
        {
            boardView?.Clear(CellOverlayKind.EnemyIntentRange);
            if (cardPreviewActive || selectedEnemyId == null || !previews.TryGetValue(selectedEnemyId, out var preview)
                || !preview.CanExecute || preview.AffectedCells.Count == 0) return;
            boardView.Show(CellOverlayKind.EnemyIntentRange, preview.AffectedCells);
        }

        public bool ValidateReferences() => controller != null && cellBuilder != null && boardView != null;
    }
}
