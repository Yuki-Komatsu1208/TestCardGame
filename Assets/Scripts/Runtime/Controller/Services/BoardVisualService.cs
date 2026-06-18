using System.Collections.Generic;
using TestCardGame.BoardManage;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// 盤面モデルの状態をセル表示へ反映するサービス。
    /// </summary>
    public class BoardVisualService
    {
        private readonly IReadOnlyDictionary<Vector2Int, RectTransform> cellRects;

        public BoardVisualService(IReadOnlyDictionary<Vector2Int, RectTransform> cellRects)
        {
            this.cellRects = cellRects;
        }

        /// <summary>
        /// セル効果の状態に応じて、セルの見た目を更新する。
        /// </summary>
        public void SyncCellVisuals(Board board)
        {
            if (board == null || cellRects == null)
            {
                return;
            }

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var cell = board.GetCell(x, y);
                    if (!cellRects.TryGetValue(new Vector2Int(x, y), out var rect) || rect == null)
                    {
                        continue;
                    }

                    if (rect.TryGetComponent<UnityEngine.UI.Image>(out var img))
                    {
                        img.color = cell.IsOnFire
                            ? new Color(1.0f, 0.5f, 0.2f, 1.0f)
                            : Color.white;
                    }
                }
            }
        }
    }
}
