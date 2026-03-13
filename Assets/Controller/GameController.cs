using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Cards;
using TestCardGame.Charactor;
using TestCardGame.Charactor.Player;
using TestCardGame.Charactor.ValueObjects;
using TestCardGame.BoardManage;
using TestCardGame.Controller.Services;

namespace TestCardGame.Controller
{

    /// <summary>
    /// ゲーム全体のロジックを管理するコントローラー
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] private CellBuilder cellBuilder;

        private UnitMoveService moveService;
        private ViewMoveService viewMoveService;
        private readonly Dictionary<UnitID, IUnit> unitsById = new();
        private readonly Dictionary<int, UnitView> viewByCharacterCode = new();
        private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
        private bool initialized;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;

        /// <summary>
        /// コントローラーの初期化を行う
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if (cellBuilder == null)
            {
                cellBuilder = FindFirstObjectByType<CellBuilder>();
            }
            if (cellBuilder == null)
            {
                Debug.LogError("GameController: CellBuilder was not found in scene.", this);
                return false;
            }

            if (!cellBuilder.Initialize())
            {
                Debug.LogError("GameController: CellBuilder initialization failed.", this);
                return false;
            }

            var board = cellBuilder.Board;
            if (board == null)
            {
                Debug.LogError("GameController: Board was not initialized.", this);
                return false;
            }

            unitsById.Clear();
            unitsById[cellBuilder.Player.ID] = cellBuilder.Player;

            viewByCharacterCode.Clear();
            viewByCharacterCode[cellBuilder.Player.ID.CharaID.Code] = cellBuilder.PlayerUnitView;

            cellRects.Clear();
            foreach (var entry in cellBuilder.CellRects)
            {
                cellRects[entry.Key] = entry.Value;
            }

            moveService = new UnitMoveService(board, unitsById);
            moveService.MoveStarted += OnMoveStarted;
            moveService.MoveCompleted += OnMoveCompleted;
            moveService.MoveRejected += OnMoveRejected;

            viewMoveService = new ViewMoveService(cellRects, viewByCharacterCode, unitsById);
            viewMoveService.Bind(moveService);

            initialized = true;
            viewMoveService.SyncAllViewsFromModel();

            var placed = moveService.RequestMoveAbsolute(cellBuilder.Player.ID, new Vector2Int(2, 2));
             

            if (!placed)
            {
                Debug.LogError("GameController: failed to place player at initial position (2,2).", this);
            }

            return true;
        }

        /// <summary>
        /// ユニットの移動をリクエストする
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="relativeDelta">現在座標からの相対移動量</param>
        /// <returns></returns>
        public bool RequestMove(UnitID unitId, Vector2Int relativeDelta)
        {
            if (!initialized)
            {
                MoveRejected?.Invoke(unitId, relativeDelta, "Controller is not initialized.");
                return false;
            }

            if (moveService == null)
            {
                MoveRejected?.Invoke(unitId, relativeDelta, "Move service is not initialized.");
                return false;
            }

            return moveService.RequestMoveRelative(unitId, relativeDelta);
        }
        /// <summary>
        /// ユニットの移動をリクエストする（方向指定版）
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="moveCount"></param>
        /// <returns></returns>
        public bool RequestPlayerMoveByDirection(Vector2Int direction, int moveCount = 1)
        {
            if (cellBuilder == null)
            {
                return false;
            }

            return RequestMoveByDirection(cellBuilder.Player.ID, direction, moveCount);
        }

        public bool RequestPlayerMoveByDropScreenPosition(Vector2 screenPosition, int moveCount = 1)
        {
            if (!initialized || cellBuilder?.Player == null)
            {
                return false;
            }

            if (!TryGetClosestCellPosition(screenPosition, out var targetCellPosition))
            {
                return false;
            }

            var playerPosition = cellBuilder.Player.Position;
            var direction = targetCellPosition - playerPosition;
            return RequestMoveByDirection(cellBuilder.Player.ID, direction, moveCount);
        }
        /// <summary>
        /// ユニットの移動をリクエストする（方向指定版）
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="direction"></param>
        /// <param name="moveCount"></param>
        /// <returns></returns>
        public bool RequestMoveByDirection(UnitID unitId, Vector2Int direction, int moveCount = 1)
        {
            if (!initialized)
            {
                MoveRejected?.Invoke(unitId, default, "Controller is not initialized.");
                return false;
            }

            if (moveCount <= 0)
            {
                MoveRejected?.Invoke(unitId, default, "Move count must be positive.");
                return false;
            }

            var offset = NormalizeCardinalDirection(direction);
            if (offset == Vector2Int.zero)
            {
                MoveRejected?.Invoke(unitId, offset, "Direction must not be zero.");
                return false;
            }

            return RequestMove(unitId, offset * moveCount);
        }
        /// <summary>
        /// モデルの状態に基づいて全てのユニットビューを同期する
        /// </summary>
        public void SyncAllViewsFromModel()
        {
            if (viewMoveService == null)
            {
                return;
            }

            viewMoveService.SyncAllViewsFromModel();
        }

        public IReadOnlyList<CardBase> GetPlayerCards()
        {
            if (cellBuilder?.Player is PlayerUnit player)
            {
                return player.Cards;
            }

            return Array.Empty<CardBase>();
        }
        /// <summary>
        /// 方向ベクトルを正規化して、最も近いカードナル方向（上下左右）に変換する
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static Vector2Int NormalizeCardinalDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
            {
                return Vector2Int.zero;
            }

            return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? new Vector2Int(direction.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, direction.y > 0 ? 1 : -1);
        }

        private bool TryGetClosestCellPosition(Vector2 screenPosition, out Vector2Int closestPosition)
        {
            closestPosition = default;

            if (cellRects.Count == 0)
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
        /// ユニットの移動開始を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void OnMoveStarted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            MoveStarted?.Invoke(unitId, from, to);
        }
        /// <summary>
        /// ユニットの移動完了を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void OnMoveCompleted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            MoveCompleted?.Invoke(unitId, from, to);
        }
        /// <summary>
        /// ユニットの移動拒否を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="to"></param>
        /// <param name="reason"></param>
        private void OnMoveRejected(UnitID unitId, Vector2Int to, string reason)
        {
            MoveRejected?.Invoke(unitId, to, reason);
        }
        
        private void OnDestroy()
        {
            if (moveService != null)
            {
                moveService.MoveStarted -= OnMoveStarted;
                moveService.MoveCompleted -= OnMoveCompleted;
                moveService.MoveRejected -= OnMoveRejected;
            }

            if (viewMoveService != null)
            {
                viewMoveService.Dispose();
            }
        }
    }
}
