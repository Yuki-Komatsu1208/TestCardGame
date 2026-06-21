using System;
using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定座標にいる対象へダメージを与える。
    /// </summary>
    public sealed class ePositionAttack : ActionEffect
    {
        private readonly int damage;
        private readonly int? maxRange;

        /// <summary>
        /// ダメージと必要なら最大射程を指定して作成する。
        /// </summary>
        /// <param name="damage">対象へ与えるダメージ。</param>
        /// <param name="maxRange">使用者からの最大射程。nullの場合は距離を制限しない。</param>
        public ePositionAttack(int damage, int? maxRange = null)
        {
            if (damage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "ダメージは1以上である必要があります。");
            }

            if (maxRange.HasValue && maxRange.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRange), "最大射程は1以上である必要があります。");
            }

            this.damage = damage;
            this.maxRange = maxRange;
        }

        /// <summary>
        /// 対象座標が盤面内かつ射程内か判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int targetPosition = context.TargetPosition;
            if (context.MoveService.GetCellAt(targetPosition) == null)
            {
                return false;
            }

            int distance = Mathf.Abs(targetPosition.x - context.User.Position.x)
                + Mathf.Abs(targetPosition.y - context.User.Position.y);

            return !maxRange.HasValue || distance <= maxRange.Value;
        }

        /// <summary>
        /// 指定座標のユニットへダメージを与える。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("指定された攻撃座標が盤面外、または射程外です。");
                return;
            }

            Vector2Int targetPosition = context.TargetPosition;

            var targetUnit = context.MoveService.GetUnitAt(targetPosition);
            if (targetUnit == null)
            {
                Debug.Log("指定された攻撃座標に対象がいません。");
                return;
            }

            if (context.StatusEffectService?.DamageService != null)
            {
                context.StatusEffectService.DamageService.DealDamage(context.User, targetUnit, damage, TestCardGame.Controller.Services.DamageType.Normal);
            }
            else
            {
                targetUnit.Hp.TakeDamage(damage);
                Debug.Log($"{context.User.Name}は座標（{targetPosition.x}, {targetPosition.y}）の{targetUnit.Name}に{damage}ポイントのダメージを与えました。");
            }
        }
    }
}
