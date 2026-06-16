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

        public override void Execute(ActionContext context)
        {
            Vector2Int targetPosition = context.TargetPosition;
            if (context.MoveService.GetCellAt(targetPosition) == null)
            {
                Debug.LogWarning("指定された攻撃座標は盤面の範囲外です。");
                return;
            }

            int distance = Mathf.Abs(targetPosition.x - context.User.Position.x)
                + Mathf.Abs(targetPosition.y - context.User.Position.y);

            if (maxRange.HasValue && distance > maxRange.Value)
            {
                Debug.LogWarning($"指定された攻撃座標が射程外です。射程は{maxRange.Value}マス以内です。");
                return;
            }

            var targetUnit = context.MoveService.GetUnitAt(targetPosition);
            if (targetUnit == null)
            {
                Debug.Log("指定された攻撃座標に対象がいません。");
                return;
            }

            targetUnit.Hp.TakeDamage(damage);
            Debug.Log($"{context.User.Name}は座標（{targetPosition.x}, {targetPosition.y}）の{targetUnit.Name}に{damage}ポイントのダメージを与えました。");
        }
    }
}
