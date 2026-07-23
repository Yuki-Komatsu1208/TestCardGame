using System;
using TestCardGame.Actions.Core;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定した直線上の対象と、その奥の敵1体までにダメージを与える。
    /// </summary>
    public sealed class ePiercingPositionAttack : ActionEffect
    {
        private readonly int damage;
        private readonly int maxRange;

        public ePiercingPositionAttack(int damage, int maxRange)
        {
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (maxRange <= 0) throw new ArgumentOutOfRangeException(nameof(maxRange));

            this.damage = damage;
            this.maxRange = maxRange;
        }

        public override bool CanExecute(ActionContext context)
        {
            if (context?.MoveService?.GetCellAt(context.TargetPosition) == null) return false;

            var offset = context.TargetPosition - context.User.Position;
            var distance = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
            return distance > 0 && distance <= maxRange && (offset.x == 0 || offset.y == 0);
        }

        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;

            var direction = context.TargetPosition - context.User.Position;
            direction = new Vector2Int(Math.Sign(direction.x), Math.Sign(direction.y));
            DealDamage(context, context.TargetPosition);

            var piercedPosition = context.TargetPosition + direction;
            if (context.MoveService.GetCellAt(piercedPosition) != null)
            {
                DealDamage(context, piercedPosition);
            }
        }

        private void DealDamage(ActionContext context, Vector2Int position)
        {
            var target = context.MoveService.GetUnitAt(position);
            if (target == null) return;

            if (context.StatusEffectService?.DamageService != null)
            {
                context.StatusEffectService.DamageService.DealDamage(context.User, target, damage, DamageType.Normal);
                return;
            }

            target.Hp.TakeDamage(damage);
        }
    }
}
