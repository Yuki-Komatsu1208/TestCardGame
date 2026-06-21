using System;
using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 直線攻撃が対象へ命中した後の処理方式。
    /// </summary>
    public enum HitType
    {
        /// <summary>射程内で最初に見つかった対象だけに命中する。</summary>
        FirstTargetOnly,
        /// <summary>射程内にいるすべての対象に命中する。</summary>
        Penetrating
    }

    /// <summary>
    /// 使用者から指定方向へ、設定された射程まで直線攻撃を行う。
    /// </summary>
    public sealed class eLineAttack : ActionEffect
    {
        private readonly int damage;
        private readonly int range;
        private readonly HitType hitType;

        /// <summary>
        /// ダメージ、射程、命中方式を指定して作成する。
        /// </summary>
        public eLineAttack(int damage, int range, HitType hitType)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "ダメージは0以上である必要があります。");
            }

            if (range <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(range), "射程は1以上である必要があります。");
            }

            if (!Enum.IsDefined(typeof(HitType), hitType))
            {
                throw new ArgumentOutOfRangeException(nameof(hitType), "未定義の命中方式です。");
            }

            this.damage = damage;
            this.range = range;
            this.hitType = hitType;
        }

        /// <summary>
        /// 攻撃方向に隣接セルがあるか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int difference = context.TargetPosition - context.User.Position;
            if (difference == Vector2Int.zero)
            {
                return false;
            }

            return context.MoveService.GetCellAt(context.User.Position + NormalizeDirection(difference)) != null;
        }

        /// <summary>
        /// 指定方向へ直線上の対象を攻撃する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("攻撃方向を指定してください。");
                return;
            }

            Vector2Int difference = context.TargetPosition - context.User.Position;
            Vector2Int direction = NormalizeDirection(difference);
            bool hitTarget = false;

            for (int distance = 1; distance <= range; distance++)
            {
                Vector2Int attackPosition = context.User.Position + direction * distance;
                if (context.MoveService.GetCellAt(attackPosition) == null)
                {
                    break;
                }

                var targetUnit = context.MoveService.GetUnitAt(attackPosition);
                if (targetUnit == null)
                {
                    continue;
                }

                if (context.StatusEffectService?.DamageService != null)
                {
                    context.StatusEffectService.DamageService.DealDamage(context.User, targetUnit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                }
                else
                {
                    targetUnit.Hp.TakeDamage(damage);
                    Debug.Log($"{context.User.Name}は{targetUnit.Name}に{damage}ポイントのダメージを与えました。");
                }
                hitTarget = true;

                if (hitType == HitType.FirstTargetOnly)
                {
                    break;
                }
            }

            if (!hitTarget)
            {
                Debug.Log("直線攻撃の射程内に対象がいません。");
            }
        }

        /// <summary>
        /// 入力方向を上下左右のどれかへ丸める。
        /// </summary>
        private static Vector2Int NormalizeDirection(Vector2Int difference)
        {
            if (Mathf.Abs(difference.x) >= Mathf.Abs(difference.y))
            {
                return new Vector2Int(difference.x > 0 ? 1 : -1, 0);
            }

            return new Vector2Int(0, difference.y > 0 ? 1 : -1);
        }
    }
}
