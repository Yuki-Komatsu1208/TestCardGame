using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：魔法の矢の効果。
    /// 集中を消費し、直線上の最初の対象へダメージを与える。
    /// </summary>
    public sealed class eMagicArrow : ActionEffect
    {
        private readonly int damage;
        private readonly int range;

        /// <summary>
        /// 魔法の矢の基本ダメージと基本射程を初期化する。
        /// </summary>
        public eMagicArrow(int damage, int range)
        {
            this.damage = damage;
            this.range = range;
        }

        /// <summary>
        /// 使用者自身以外の方向が指定されているか確認する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int difference = context.TargetPosition - context.User.Position;
            return difference != Vector2Int.zero;
        }

        /// <summary>
        /// 現在の集中をすべて消費し、その量だけ射程を伸ばして発動する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("射撃方向を指定してください。");
                return;
            }

            IUnit user = context.User;
            int spentFocus = MagicFocusHelper.ConsumeAllFocus(user);
            int actualRange = range + spentFocus;
            Debug.Log($"{user.Name}は集中を{spentFocus}解放して魔法：魔法の矢を放った！ (射程: {actualRange})");

            Vector2Int difference = context.TargetPosition - user.Position;
            Vector2Int direction = NormalizeDirection(difference);

            for (int distance = 1; distance <= actualRange; distance++)
            {
                Vector2Int attackPosition = user.Position + direction * distance;
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
                    context.StatusEffectService.DamageService.DealDamage(user, targetUnit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                }
                else
                {
                    targetUnit.Hp.TakeDamage(damage);
                }

                Debug.Log($"{user.Name}の魔法の矢が{targetUnit.Name}に命中！ {damage}ダメージ。");
                return;
            }

            Debug.Log("魔法の矢は空振りに終わった。");
        }

        /// <summary>
        /// 入力ベクトルを上下左右の射撃方向へ丸める。
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
