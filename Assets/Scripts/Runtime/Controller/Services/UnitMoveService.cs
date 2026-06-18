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

        /// <summary>
        /// 指定座標にいるユニットを取得する。
        /// </summary>
        public IUnit GetUnitAt(Vector2Int pos)
        {
            if (board.IsInside(pos.x, pos.y))
            {
                return board.GetCell(pos.x, pos.y).Occupant;
            }
            return null;
        }

        /// <summary>
        /// 指定座標のセルを取得する。
        /// </summary>
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

            // 目的地が有効ならそのまま移動する。
            if (board.IsInside(to.x, to.y) && board.GetCell(to.x, to.y).CanMove)
            {
                return RequestMoveAbsoluteInternal(unitId, unit, from, to);
            }

            // 目的地が無効なら、目的地に最も近い移動可能セルを探す。
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
                Debug.Log($"強制移動: 目的地 {to} が使用できないため、最寄りの移動可能セル {nearestWalkable} に補正します。");
                return RequestMoveAbsoluteInternal(unitId, unit, from, nearestWalkable);
            }

            Debug.LogWarning($"強制移動: 目的地 {to} が使用できず、代替の移動可能セルも見つかりませんでした。");
            MoveRejected?.Invoke(unitId, to, "強制移動に失敗しました（移動可能な目的地がありません）。");
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
                MoveRejected?.Invoke(unitId, targetForError, "盤面が初期化されていません。");
                return false;
            }

            if (isBusy)
            {
                MoveRejected?.Invoke(unitId, targetForError, "移動サービスが処理中です。");
                return false;
            }

            if (unitsById == null || !unitsById.TryGetValue(unitId, out unit))
            {
                MoveRejected?.Invoke(unitId, targetForError, "指定されたユニットが見つかりません。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// モデル上の絶対座標移動を実行し、移動イベントと炎上マス処理を行う。
        /// </summary>
        private bool RequestMoveAbsoluteInternal(UnitID unitId, IUnit unit, Vector2Int from, Vector2Int to)
        {
            if (from == to)
            {
                MoveRejected?.Invoke(unitId, to, "すでに目的地にいます。");
                return false;
            }

            if (!board.IsInside(to.x, to.y))
            {
                MoveRejected?.Invoke(unitId, to, "目的地が盤面外です。");
                return false;
            }

            MoveStarted?.Invoke(unitId, from, to);
            isBusy = true;

            if (!board.TryMoveUnit(unit, to.x, to.y))
            {
                isBusy = false;
                MoveRejected?.Invoke(unitId, to, "盤面モデル上の移動に失敗しました。");
                return false;
            }

            isBusy = false;

            // 炎上しているマスに移動した場合の状態異常付与
            var targetCell = board.GetCell(to.x, to.y);
            if (targetCell.IsOnFire && unit != null)
            {
                statusEffectService?.ApplyBurn(unit, targetCell.FireTurns, targetCell.FireDamage);
                Debug.Log($"移動効果: 炎上マス（{to.x}, {to.y}）に進入したため、{unit.Name}に炎上状態（{targetCell.FireTurns}ターン、ダメージ: {targetCell.FireDamage}）を適用しました。");
            }

            MoveCompleted?.Invoke(unitId, from, to);
            return true;
        }
    }
}
