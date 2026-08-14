using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using System.Collections.Generic;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>前方の敵を押し出しながら前進し、永続シールドを得る複合効果。</summary>
    public sealed class eShieldBash : ActionEffect, IActionPreviewProvider
    {
        private readonly int damage;
        private readonly int shieldValue;
        private readonly int moveDistance;
        private readonly int forceMoveStunDamage;

        public eShieldBash(int damage, int shieldValue, int moveDistance, int forceMoveStunDamage)
        {
            this.damage = damage;
            this.shieldValue = shieldValue;
            this.moveDistance = moveDistance;
            this.forceMoveStunDamage = forceMoveStunDamage;
        }

        public override bool CanExecute(ActionContext context)
        {
            if (context?.User == null || context.MoveService == null) return false;
            return NormalizeCardinal(context.TargetPosition - context.User.Position) != Vector2Int.zero;
        }

        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;

            Vector2Int direction = NormalizeCardinal(context.TargetPosition - context.User.Position);
            var frontTarget = context.MoveService.GetUnitAt(context.User.Position + direction);
            if (frontTarget != null && frontTarget != context.User)
            {
                context.StatusEffectService?.DamageService?.DealDamage(context.User, frontTarget, damage, DamageType.Normal);
                context.MoveService.ForceMove(context.User, frontTarget, moveDistance, ForceMoveDirection.AwayFromUser, forceMoveStunDamage);
            }

            context.MoveService.Move(context.User.ID, direction * moveDistance);
            context.StatusEffectService?.ApplyStatus(context.User, StatusEffectId.Shield, 1, shieldValue);
        }

        private static Vector2Int NormalizeCardinal(Vector2Int offset)
        {
            if (offset == Vector2Int.zero) return Vector2Int.zero;
            return Mathf.Abs(offset.x) >= Mathf.Abs(offset.y)
                ? new Vector2Int(offset.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, offset.y > 0 ? 1 : -1);
        }

        /// <summary>前方対象と移動経路を副作用なしで返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            if (context?.User == null || context.MoveService == null) return ActionPreviewResult.None;
            Vector2Int direction = NormalizeCardinal(context.CandidatePosition - context.User.Position);
            if (direction == Vector2Int.zero) return new ActionPreviewResult(true, System.Array.Empty<Vector2Int>(), System.Array.Empty<Vector2Int>());
            var cells = new List<Vector2Int>();
            for (int i = 1; i <= Mathf.Max(1, moveDistance); i++)
            {
                Vector2Int position = context.User.Position + direction * i;
                if (context.MoveService.GetCellAt(position) == null) break;
                cells.Add(position);
            }
            return new ActionPreviewResult(true, new[] { context.CandidatePosition }, cells);
        }
    }
}
