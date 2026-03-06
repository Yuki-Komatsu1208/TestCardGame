using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Charactor;
using TestCardGame.Charactor.ValueObjects;
using TestCardGame.BoardManage;

namespace TestCardGame.Controller
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private CellBuilder cellBuilder;

        private Board board;
        private readonly Dictionary<UnitID, IUnit> unitsById = new();
        private readonly Dictionary<int, UnitView> viewByCharacterCode = new();
        private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
        private bool isBusy;
        private bool initialized;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;

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

            board = cellBuilder.Board;

            unitsById.Clear();
            unitsById[cellBuilder.Player.ID] = cellBuilder.Player;

            viewByCharacterCode.Clear();
            viewByCharacterCode[cellBuilder.Player.ID.CharaID.Code] = cellBuilder.PlayerUnitView;

            cellRects.Clear();
            foreach (var entry in cellBuilder.CellRects)
            {
                cellRects[entry.Key] = entry.Value;
            }

            cellBuilder.CellClicked += OnCellClicked;
            SyncAllViewsFromModel();
            initialized = true;
            return true;
        }

        public bool RequestMove(UnitID unitId, Vector2Int to)
        {
            if (!initialized)
            {
                NotifyMoveFailed(unitId, to, "Controller is not initialized.");
                return false;
            }

            if (isBusy)
            {
                NotifyMoveFailed(unitId, to, "Controller is busy.");
                return false;
            }

            if (!TryResolveMove(unitId, to, out var unit, out var from))
            {
                return false;
            }

            MoveStarted?.Invoke(unitId, from, to);
            SetBusy(true);

            if (!board.TryMoveUnit(unit, to.x, to.y))
            {
                SetBusy(false);
                NotifyMoveFailed(unitId, to, "Move failed in model.");
                return false;
            }

            PlayMoveView(unit, from, to);
            return true;
        }

        public void SyncAllViewsFromModel()
        {
            foreach (var unit in unitsById.Values)
            {
                if (!viewByCharacterCode.ContainsKey(unit.ID.CharaID.Code))
                {
                    continue;
                }
                cellBuilder.MoveUnitView(unit);
            }
        }

        private bool TryResolveMove(UnitID unitId, Vector2Int to, out IUnit unit, out Vector2Int from)
        {
            unit = null;
            from = default;

            if (!unitsById.TryGetValue(unitId, out unit))
            {
                NotifyMoveFailed(unitId, to, "Unit not found.");
                return false;
            }

            from = unit.Position;
            if (from == to)
            {
                NotifyMoveFailed(unitId, to, "Already at target.");
                return false;
            }

            if (!board.IsInside(to.x, to.y))
            {
                NotifyMoveFailed(unitId, to, "Target is outside board.");
                return false;
            }

            return true;
        }

        private void PlayMoveView(IUnit unit, Vector2Int from, Vector2Int to)
        {
            if (cellRects.ContainsKey(to))
            {
                cellBuilder.MoveUnitView(unit);
            }

            SetBusy(false);
            MoveCompleted?.Invoke(unit.ID, from, to);
        }

        private void OnCellClicked(int x, int y)
        {
            var playerId = cellBuilder.Player.ID;
            RequestMove(playerId, new Vector2Int(x, y));
        }

        private void SetBusy(bool busy)
        {
            isBusy = busy;
        }

        private void NotifyMoveFailed(UnitID unitId, Vector2Int to, string reason)
        {
            MoveRejected?.Invoke(unitId, to, reason);
        }

        private void OnDestroy()
        {
            if (cellBuilder != null)
            {
                cellBuilder.CellClicked -= OnCellClicked;
            }
        }
    }
}
