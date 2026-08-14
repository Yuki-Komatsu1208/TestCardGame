using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TestCardGame.Character;
using UnityEngine.EventSystems;

namespace TestCardGame.UIs.Battle
{
    public enum CellOverlayKind { MoveCandidate, PlayerActionRange, EnemyIntentRange }

    public interface IBoardOverlayView
    {
        void Show(CellOverlayKind kind, IReadOnlyCollection<Vector2Int> cells);
        void Clear(CellOverlayKind kind);
        void ClearAllTransient();
    }

    /// <summary>盤面の自動フィットとセル別Overlayを担当する。</summary>
    public sealed class BoardView : MonoBehaviour, IBoardOverlayView, IPointerClickHandler
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField, Min(0f)] private float spacing = 5f;
        [SerializeField, Min(1f)] private float maximumCellSize = 96f;
        [SerializeField] private Vector4 padding = new(8f, 8f, 8f, 8f);

        private Vector2Int boardSize;
        private IReadOnlyDictionary<Vector2Int, RectTransform> cellRects;
        private Vector2 lastViewportSize;

        public RectTransform Viewport => viewport;
        public event Action BackgroundClicked;

        /// <summary>敵以外の盤面をクリックした際、敵選択解除を通知する。</summary>
        public void OnPointerClick(PointerEventData eventData) => BackgroundClicked?.Invoke();

        /// <summary>生成済みセルを盤面座標へ関連付け、現在の表示領域へ収める。</summary>
        public void Bind(Vector2Int size, IReadOnlyDictionary<Vector2Int, RectTransform> cells)
        {
            boardSize = size;
            cellRects = cells;
            ApplyLayout();
            ClearAllTransient();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (viewport != null && viewport.rect.size != lastViewportSize && boardSize.x > 0) ApplyLayout();
        }

        /// <summary>セルサイズ、列数、Content寸法を計算結果だけから更新する。</summary>
        public void ApplyLayout()
        {
            if (viewport == null || content == null || grid == null || boardSize.x <= 0 || boardSize.y <= 0) return;
            lastViewportSize = viewport.rect.size;
            if (lastViewportSize.x <= 0f || lastViewportSize.y <= 0f) return;
            try
            {
                BoardLayoutResult result = BoardLayoutCalculator.Calculate(lastViewportSize, boardSize, spacing, padding, maximumCellSize);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = boardSize.x;
                grid.spacing = Vector2.one * spacing;
                grid.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.z), Mathf.RoundToInt(padding.w));
                grid.cellSize = Vector2.one * result.CellSize;
                content.sizeDelta = result.ContentSize;
                content.anchoredPosition = Vector2.zero;
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                ResizeUnitViews(result.CellSize);
            }
            catch (Exception exception) { Debug.LogError($"BoardView: 盤面を表示領域へ配置できません。{exception.Message}", this); }
        }

        /// <summary>盤面上の事前配置・複製UnitViewをセル内へ収まる寸法にそろえる。</summary>
        private void ResizeUnitViews(float cellSize)
        {
            float unitSize = Mathf.Max(1f, cellSize * 0.78f);
            foreach (UnitView unitView in GetComponentsInChildren<UnitView>(true))
            {
                if (unitView.TryGetComponent(out RectTransform unitRect)) unitRect.sizeDelta = Vector2.one * unitSize;
            }
        }

        public void Show(CellOverlayKind kind, IReadOnlyCollection<Vector2Int> cells)
        {
            Clear(kind);
            if (cells == null || cellRects == null) return;
            foreach (Vector2Int position in cells)
                if (cellRects.TryGetValue(position, out RectTransform rect) && rect != null && rect.TryGetComponent(out CellView view)) view.SetOverlay(kind, true);
        }

        public void Clear(CellOverlayKind kind)
        {
            if (cellRects == null) return;
            foreach (RectTransform rect in cellRects.Values)
                if (rect != null && rect.TryGetComponent(out CellView view)) view.SetOverlay(kind, false);
        }

        public void ClearAllTransient()
        {
            Clear(CellOverlayKind.MoveCandidate); Clear(CellOverlayKind.PlayerActionRange); Clear(CellOverlayKind.EnemyIntentRange);
        }

        public bool ValidateReferences() => viewport != null && content != null && grid != null;
    }
}
