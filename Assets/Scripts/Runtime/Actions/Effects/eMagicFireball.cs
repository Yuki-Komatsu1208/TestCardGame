using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：ファイアボールの効果。
    /// 集中を消費し、直線上の最初の対象にダメージと炎上効果。
    /// </summary>
    public sealed class eMagicFireball : ActionEffect
    {
        private static readonly Vector2Int[] AdjacentOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private readonly int damage;
        private readonly int range;
        private readonly int burnDuration;
        private readonly int burnDamage;

        /// <summary>
        /// ファイアボールの威力、射程、炎上を初期化する。
        /// </summary>
        public eMagicFireball(int damage, int range, int burnDuration, int burnDamage)
        {
            this.damage = damage;
            this.range = range;
            this.burnDuration = burnDuration;
            this.burnDamage = burnDamage;
        }

        /// <summary>
        /// 使用者自身以外の方向が指定されているか確認する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int difference = context.TargetPosition - context.User.Position;
            if (difference == Vector2Int.zero)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 現在の集中をすべて消費し、その量だけ炎上を強化して発動する。
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
            int actualBurnDuration = burnDuration + spentFocus;
            int actualBurnDamage = burnDamage + spentFocus;
            Debug.Log($"{user.Name}は集中を{spentFocus}解放して魔法：ファイアボールを発動した！ (炎上: {actualBurnDuration}ターン / {actualBurnDamage}ダメージ)");

            Vector2Int difference = context.TargetPosition - user.Position;
            Vector2Int direction = NormalizeDirection(difference);
            bool hitTarget = false;

            for (int distance = 1; distance <= range; distance++)
            {
                // 指定方向を1マスずつ見て、最初に当たったユニットだけを対象にする。
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

                // サービスがある場合は共通ダメージ計算を通し、なければ直接HPを減らす。
                if (context.StatusEffectService?.DamageService != null)
                {
                    context.StatusEffectService.DamageService.DealDamage(user, targetUnit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                }
                else
                {
                    targetUnit.Hp.TakeDamage(damage);
                }

                context.StatusEffectService?.ApplyBurn(targetUnit, actualBurnDuration, actualBurnDamage);

                // 直撃対象の周囲4マスにも炎上だけを拡散させる。
                foreach (var offset in AdjacentOffsets)
                {
                    Vector2Int adjacentPosition = targetUnit.Position + offset;
                    var adjacentUnit = context.MoveService.GetUnitAt(adjacentPosition);
                    if (adjacentUnit == null)
                    {
                        continue;
                    }

                    context.StatusEffectService?.ApplyBurn(adjacentUnit, actualBurnDuration, actualBurnDamage);
                }

                Debug.Log($"{user.Name}の魔法が{targetUnit.Name}に命中！ {damage}ダメージと炎上効果({actualBurnDuration}ターン, 毎ターン{actualBurnDamage}ダメージ)を付与。");
                hitTarget = true;
                break;
            }

            if (!hitTarget)
            {
                Debug.Log("ファイアボールは空振りに終わった。");
            }
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
