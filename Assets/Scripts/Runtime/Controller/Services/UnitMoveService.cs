using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.BoardManage;
using TestCardGame.Character;
using TestCardGame.Character.ValueObjects;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    public enum ForceMoveCollision { None, Obstacle, Unit }

    public readonly struct ForceMoveResult
    {
        public int MovedSteps { get; }
        public ForceMoveCollision Collision { get; }
        public IUnit CollidedUnit { get; }
        public bool DidCollide => Collision != ForceMoveCollision.None;

        public ForceMoveResult(int movedSteps, ForceMoveCollision collision, IUnit collidedUnit = null)
        {
            MovedSteps = movedSteps;
            Collision = collision;
            CollidedUnit = collidedUnit;
        }
    }

    /// <summary>通常移動、強制移動、引き寄せ、転移を扱う盤面移動サービス。</summary>
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

        public IUnit GetUnitAt(Vector2Int pos) => board != null && board.IsInside(pos.x, pos.y) ? board.GetCell(pos.x, pos.y).Occupant : null;
        public Cell GetCellAt(Vector2Int pos) => board != null && board.IsInside(pos.x, pos.y) ? board.GetCell(pos.x, pos.y) : null;

        /// <summary>通常移動。移動不能なら失敗し、位置補正は行わない。</summary>
        public bool Move(UnitID unitId, Vector2Int relative)
        {
            if (!TryResolveUnit(unitId, relative, out IUnit unit)) return false;
            Vector2Int direction = NormalizeCardinal(relative);
            int steps = Mathf.Max(Mathf.Abs(relative.x), Mathf.Abs(relative.y));
            bool moved = false;
            for (int step = 0; step < steps; step++)
            {
                Vector2Int next = unit.Position + direction;
                if (!MoveAbsoluteInternal(unitId, unit, unit.Position, next)) break;
                moved = true;
            }
            return moved;
        }

        /// <summary>指定マスへの通常移動。初期配置にも使用する。</summary>
        public bool Teleport(UnitID unitId, Vector2Int destination)
        {
            if (!TryResolveUnit(unitId, destination, out IUnit unit)) return false;
            return MoveAbsoluteInternal(unitId, unit, unit.Position, destination);
        }

        /// <summary>
        /// 一方向へ1マスずつ対象を移動させる。衝突時だけ双方へ基礎値の2倍のスタン耐性ダメージを与える。
        /// </summary>
        public ForceMoveResult ForceMove(IUnit user, IUnit target, int distance, ForceMoveDirection direction, int baseStunDamage)
        {
            if (user == null || target == null || distance <= 0 || board == null) return new ForceMoveResult(0, ForceMoveCollision.None);
            Vector2Int moveDirection = ResolveForceMoveDirection(user.Position, target.Position, direction);
            if (moveDirection == Vector2Int.zero) return new ForceMoveResult(0, ForceMoveCollision.None);

            int movedSteps = 0;
            for (int step = 0; step < distance; step++)
            {
                Vector2Int next = target.Position + moveDirection;
                if (!board.IsInside(next.x, next.y))
                {
                    DealCollisionStun(target, null, baseStunDamage);
                    return new ForceMoveResult(movedSteps, ForceMoveCollision.Obstacle);
                }

                Cell nextCell = board.GetCell(next.x, next.y);
                if (!nextCell.CanMove)
                {
                    IUnit collidedUnit = nextCell.Occupant;
                    DealCollisionStun(target, collidedUnit, baseStunDamage);
                    return new ForceMoveResult(movedSteps, collidedUnit == null ? ForceMoveCollision.Obstacle : ForceMoveCollision.Unit, collidedUnit);
                }

                if (!MoveAbsoluteInternal(target.ID, target, target.Position, next))
                    return new ForceMoveResult(movedSteps, ForceMoveCollision.Obstacle);
                movedSteps++;
            }

            return new ForceMoveResult(movedSteps, ForceMoveCollision.None);
        }

        /// <summary>対象を使用者へ近づける。衝突時にもスタン耐性ダメージを与えない。</summary>
        public bool Pull(IUnit user, IUnit target, int distance)
        {
            if (user == null || target == null || distance <= 0 || user == target) return false;
            Vector2Int direction = NormalizeCardinal(user.Position - target.Position);
            int remainingAxisDistance = Mathf.Max(Mathf.Abs(user.Position.x - target.Position.x), Mathf.Abs(user.Position.y - target.Position.y));
            int steps = Mathf.Min(distance, Mathf.Max(0, remainingAxisDistance - 1));
            bool moved = false;
            for (int step = 0; step < steps; step++)
            {
                Vector2Int next = target.Position + direction;
                if (GetCellAt(next)?.CanMove != true) break;
                if (!MoveAbsoluteInternal(target.ID, target, target.Position, next)) break;
                moved = true;
            }
            return moved;
        }

        // 既存の初期配置・UI呼び出し用の薄い互換窓口。強制移動には使わない。
        public bool RequestMoveRelative(UnitID unitId, Vector2Int relative) => Move(unitId, relative);
        public bool RequestMoveAbsolute(UnitID unitId, Vector2Int absolute) => Teleport(unitId, absolute);

        private void DealCollisionStun(IUnit movedUnit, IUnit collidedUnit, int baseStunDamage)
        {
            int collisionDamage = Mathf.Max(0, baseStunDamage) * 2;
            if (collisionDamage == 0) return;
            statusEffectService?.DealStunDamage(movedUnit, collisionDamage);
            if (collidedUnit != null && collidedUnit != movedUnit)
                statusEffectService?.DealStunDamage(collidedUnit, collisionDamage);
        }

        private static Vector2Int ResolveForceMoveDirection(Vector2Int userPosition, Vector2Int targetPosition, ForceMoveDirection direction)
        {
            return direction switch
            {
                ForceMoveDirection.AwayFromUser => NormalizeCardinal(targetPosition - userPosition),
                ForceMoveDirection.TowardUser => NormalizeCardinal(userPosition - targetPosition),
                ForceMoveDirection.BoardUp => Vector2Int.up,
                ForceMoveDirection.BoardRight => Vector2Int.right,
                ForceMoveDirection.BoardDown => Vector2Int.down,
                ForceMoveDirection.BoardLeft => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        private static Vector2Int NormalizeCardinal(Vector2Int offset)
        {
            if (offset == Vector2Int.zero) return Vector2Int.zero;
            return Mathf.Abs(offset.x) >= Mathf.Abs(offset.y)
                ? new Vector2Int(Math.Sign(offset.x), 0)
                : new Vector2Int(0, Math.Sign(offset.y));
        }

        private bool TryResolveUnit(UnitID unitId, Vector2Int targetForError, out IUnit unit)
        {
            unit = null;
            if (board == null) { MoveRejected?.Invoke(unitId, targetForError, "盤面が初期化されていません。"); return false; }
            if (isBusy) { MoveRejected?.Invoke(unitId, targetForError, "移動サービスが処理中です。"); return false; }
            if (unitsById == null || !unitsById.TryGetValue(unitId, out unit)) { MoveRejected?.Invoke(unitId, targetForError, "指定されたユニットが見つかりません。"); return false; }
            return true;
        }

        private bool MoveAbsoluteInternal(UnitID unitId, IUnit unit, Vector2Int from, Vector2Int to)
        {
            if (!board.IsInside(to.x, to.y)) { MoveRejected?.Invoke(unitId, to, "目的地が盤面外です。"); return false; }
            if (from == to && board.GetCell(to.x, to.y).Occupant == unit) { MoveRejected?.Invoke(unitId, to, "すでに目的地にいます。"); return false; }
            if (!board.GetCell(to.x, to.y).CanMove) { MoveRejected?.Invoke(unitId, to, "目的地に移動できません。"); return false; }

            MoveStarted?.Invoke(unitId, from, to);
            isBusy = true;
            bool moved = board.TryMoveUnit(unit, to.x, to.y);
            isBusy = false;
            if (!moved) { MoveRejected?.Invoke(unitId, to, "盤面モデル上の移動に失敗しました。"); return false; }

            Cell targetCell = board.GetCell(to.x, to.y);
            if (targetCell.IsOnFire) statusEffectService?.ApplyBurn(unit, targetCell.FireTurns, targetCell.FireDamage);
            MoveCompleted?.Invoke(unitId, from, to);
            return true;
        }
    }
}
