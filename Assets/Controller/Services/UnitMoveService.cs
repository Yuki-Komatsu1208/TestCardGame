using TestCardGame.BoardManage;
using TestCardGame.Charactor;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace TestCardGame.Controller.Services
{
    public class UnitMoveService
    {
        private readonly Board board;
        private readonly IReadOnlyDictionary<UnitID, IUnit> unitsById;
        private bool isBusy;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;

        public UnitMoveService(Board board, IReadOnlyDictionary<UnitID, IUnit> unitsById)
        {
            this.board = board;
            this.unitsById = unitsById;
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
            MoveCompleted?.Invoke(unitId, from, to);
            return true;
        }
    }
}
