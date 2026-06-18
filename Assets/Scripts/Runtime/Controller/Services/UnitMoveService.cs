using TestCardGame.BoardManage;
using TestCardGame.Character;
using TestCardGame.Character.ValueObjects;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TestCardGame.Controller.Services
{
    public class UnitMoveService
    {
        private readonly Board board;
        private readonly IReadOnlyDictionary<UnitID, IUnit> unitsById;
        private readonly StatusEffectService statusEffectService;
        private bool isBusy;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;

        public UnitMoveService(Board board, IReadOnlyDictionary<UnitID, IUnit> unitsById, StatusEffectService statusEffectService)
        {
            this.board = board;
            this.unitsById = unitsById;
            this.statusEffectService = statusEffectService;
        }

        public IUnit GetUnitAt(Vector2Int pos)
        {
            if (board.IsInside(pos.x, pos.y))
            {
                return board.GetCell(pos.x, pos.y).Occupant;
            }
            return null;
        }

        public Cell GetCellAt(Vector2Int pos)
        {
            if (board.IsInside(pos.x, pos.y))
            {
                return board.GetCell(pos.x, pos.y);
            }
            return null;
        }
        /// <summary>
        /// ユニットの移動をリクエストする（相対座標）
        /// </summary>
        public bool RequestMoveRelative(UnitID unitId, Vector2Int relative)
        {
            if (!TryResolveUnit(unitId, relative, out var unit))
            {
                return false;
            }

            var from = unit.Position;
            var to = from + relative;
            return RequestMoveAbsoluteInternal(unitId, unit, from, to);
        }
        /// <summary>
        /// ユニットの移動をリクエストする（絶対座標）
        /// </summary>
        public bool RequestMoveAbsolute(UnitID unitId, Vector2Int absolute)
        {
            if (!TryResolveUnit(unitId, absolute, out var unit))
            {
                return false;
            }

            var from = unit.Position;
            return RequestMoveAbsoluteInternal(unitId, unit, from, absolute);
        }

        /// <summary>
        /// ユニットの強制移動をリクエストする。
        /// 移動先が盤面外またはブロックされている場合は、最も近い移動可能セルに補正されます。
        /// </summary>
        public bool RequestForcedMove(UnitID unitId, Vector2Int target)
        {
            if (!TryResolveUnit(unitId, target, out var unit))
            {
                return false;
            }

            var from = unit.Position;
            var to = target;

            // Check if destination is valid (inside and CanMove)
            if (board.IsInside(to.x, to.y) && board.GetCell(to.x, to.y).CanMove)
            {
                return RequestMoveAbsoluteInternal(unitId, unit, from, to);
            }

            // Target is blocked or outside. Find nearest walkable cell to "to"
            Vector2Int nearestWalkable = from;
            int minDistance = int.MaxValue;
            bool found = false;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var cell = board.GetCell(x, y);
                    if (cell.CanMove)
                    {
                        int dist = Mathf.Abs(x - to.x) + Mathf.Abs(y - to.y);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearestWalkable = new Vector2Int(x, y);
                            found = true;
                        }
                    }
                }
            }

            if (found && nearestWalkable != from)
            {
                Debug.Log($"ForcedMove: Target {to} was blocked/outside. Correcting destination to nearest walkable {nearestWalkable}");
                return RequestMoveAbsoluteInternal(unitId, unit, from, nearestWalkable);
            }

            Debug.LogWarning($"ForcedMove: Target {to} was blocked/outside and no alternative walkable cell was found.");
            MoveRejected?.Invoke(unitId, to, "Forced move failed (no walkable destination).");
            return false;
        }
        
        /// <summary>
        /// ユニットIDからユニットを解決する。失敗した場合はエラーイベントを発火する。
        /// </summary>
        private bool TryResolveUnit(UnitID unitId, Vector2Int targetForError, out IUnit unit)
        {
            unit = null;

            if (board == null)
            {
                MoveRejected?.Invoke(unitId, targetForError, "Board is not initialized.");
                return false;
            }

            if (isBusy)
            {
                MoveRejected?.Invoke(unitId, targetForError, "Move service is busy.");
                return false;
            }

            if (unitsById == null || !unitsById.TryGetValue(unitId, out unit))
            {
                MoveRejected?.Invoke(unitId, targetForError, "Unit not found.");
                return false;
            }

            return true;
        }

        private bool RequestMoveAbsoluteInternal(UnitID unitId, IUnit unit, Vector2Int from, Vector2Int to)
        {
            if (from == to)
            {
                MoveRejected?.Invoke(unitId, to, "Already at target.");
                return false;
            }

            if (!board.IsInside(to.x, to.y))
            {
                MoveRejected?.Invoke(unitId, to, "Target is outside board.");
                return false;
            }

            MoveStarted?.Invoke(unitId, from, to);
            isBusy = true;

            if (!board.TryMoveUnit(unit, to.x, to.y))
            {
                isBusy = false;
                MoveRejected?.Invoke(unitId, to, "Move failed in model.");
                return false;
            }

            isBusy = false;

            // 炎上しているマスに移動した場合の状態異常付与
            var targetCell = board.GetCell(to.x, to.y);
            if (targetCell.IsOnFire && unit != null)
            {
                statusEffectService?.ApplyBurn(unit, targetCell.FireTurns, targetCell.FireDamage);
                Debug.Log($"移動効果：炎上マス（{to.x}, {to.y}）に進入したため、{unit.Name}にOnFire状態異常（{targetCell.FireTurns}ターン、ダメージ: {targetCell.FireDamage}）を適用しました。");
            }

            MoveCompleted?.Invoke(unitId, from, to);
            return true;
        }
    }
}
