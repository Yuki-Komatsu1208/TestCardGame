using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// 画面上の入力座標を、盤面上のセル座標へ変換するサービス。
    /// </summary>
    public class BoardTargetingService
    {
        private readonly IReadOnlyDictionary<Vector2Int, RectTransform> cellRects;

        public BoardTargetingService(IReadOnlyDictionary<Vector2Int, RectTransform> cellRects)
        {
            this.cellRects = cellRects;
        }

        /// <summary>
        /// 画面座標に最も近いセル座標を取得する。
        /// </summary>
        public bool TryGetClosestCellPosition(Vector2 screenPosition, out Vector2Int closestPosition)
        {
            closestPosition = default;

            if (cellRects == null || cellRects.Count == 0)
            {
                return false;
            }

            var minDistanceSq = float.MaxValue;
            var found = false;

            foreach (var entry in cellRects)
            {
                var rect = entry.Value;
                if (rect == null)
                {
                    continue;
                }

                var cellCenterScreenPos = RectTransformUtility.WorldToScreenPoint(
                    null,
                    rect.TransformPoint(rect.rect.center));
                var distanceSq = (cellCenterScreenPos - screenPosition).sqrMagnitude;
                if (distanceSq >= minDistanceSq)
                {
                    continue;
                }

                minDistanceSq = distanceSq;
                closestPosition = entry.Key;
                found = true;
            }

            return found;
        }

        /// <summary>
        /// 任意の方向ベクトルを上下左右のいずれかへ正規化する。
        /// </summary>
        public static Vector2Int NormalizeCardinalDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
            {
                return Vector2Int.zero;
            }

            return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? new Vector2Int(direction.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, direction.y > 0 ? 1 : -1);
        }
    }
}
