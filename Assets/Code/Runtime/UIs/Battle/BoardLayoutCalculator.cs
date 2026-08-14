using System;
using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    public readonly struct BoardLayoutResult
    {
        public float CellSize { get; }
        public Vector2 ContentSize { get; }

        public BoardLayoutResult(float cellSize, Vector2 contentSize)
        {
            CellSize = cellSize;
            ContentSize = contentSize;
        }
    }

    /// <summary>盤面をViewportへ収める正方形セル寸法を純粋計算する。</summary>
    public static class BoardLayoutCalculator
    {
        public static BoardLayoutResult Calculate(Vector2 viewportSize, Vector2Int boardSize, float spacing,
            Vector4 padding, float maximumCellSize)
        {
            if (boardSize.x <= 0 || boardSize.y <= 0) throw new ArgumentOutOfRangeException(nameof(boardSize), "盤面サイズは1以上である必要があります。");
            if (viewportSize.x <= 0f || viewportSize.y <= 0f) throw new ArgumentOutOfRangeException(nameof(viewportSize), "盤面表示領域は0より大きい必要があります。");
            if (spacing < 0f || padding.x < 0f || padding.y < 0f || padding.z < 0f || padding.w < 0f)
                throw new ArgumentOutOfRangeException(nameof(padding), "盤面の余白と間隔は0以上である必要があります。");
            if (maximumCellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(maximumCellSize), "セル最大寸法は0より大きい必要があります。");

            float usableWidth = viewportSize.x - padding.x - padding.y - spacing * (boardSize.x - 1);
            float usableHeight = viewportSize.y - padding.z - padding.w - spacing * (boardSize.y - 1);
            if (usableWidth <= 0f || usableHeight <= 0f) throw new ArgumentException("盤面表示領域が余白と間隔より小さいため配置できません。");
            float cellSize = Mathf.Min(maximumCellSize, usableWidth / boardSize.x, usableHeight / boardSize.y);
            return new BoardLayoutResult(cellSize, new Vector2(
                cellSize * boardSize.x + spacing * (boardSize.x - 1) + padding.x + padding.y,
                cellSize * boardSize.y + spacing * (boardSize.y - 1) + padding.z + padding.w));
        }
    }
}
