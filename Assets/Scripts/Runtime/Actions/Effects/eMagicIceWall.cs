using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：アイスウォールの効果。
    /// 自身へバリアを付与し、周囲4マスへダメージと凍傷を与える。
    /// </summary>
    public sealed class eMagicIceWall : ActionEffect
    {
        private static readonly Vector2Int[] AdjacentOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private readonly int shieldDuration;
        private readonly int shieldValue;
        private readonly int damage;
        private readonly int frostbiteDuration;

        /// <summary>
        /// アイスウォールのバリア、ダメージ、凍傷を初期化する。
        /// </summary>
        public eMagicIceWall(int shieldDuration, int shieldValue, int damage, int frostbiteDuration)
        {
            this.shieldDuration = shieldDuration;
            this.shieldValue = shieldValue;
            this.damage = damage;
            this.frostbiteDuration = frostbiteDuration;
        }

        /// <summary>
        /// 自己中心の魔法なので、使用者がいれば実行できる。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            return context.User != null;
        }

        /// <summary>
        /// 現在の集中をすべて消費し、その量だけバリア値を増やして発動する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                return;
            }

            IUnit user = context.User;
            int spentFocus = MagicFocusHelper.ConsumeAllFocus(user);
            int actualShieldValue = shieldValue + spentFocus;
            context.StatusEffectService?.ApplyStatus(user, StatusEffectId.Barrier, shieldDuration, actualShieldValue);
            Debug.Log($"{user.Name}は集中を{spentFocus}解放して魔法：アイスウォールを展開した！ 自身にバリア{actualShieldValue}を付与。");

            foreach (var offset in AdjacentOffsets)
            {
                Vector2Int targetPosition = user.Position + offset;
                if (context.MoveService.GetCellAt(targetPosition) == null)
                {
                    continue;
                }

                var targetUnit = context.MoveService.GetUnitAt(targetPosition);
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

                context.StatusEffectService?.ApplyFrostbite(targetUnit, frostbiteDuration);
                Debug.Log($"{targetUnit.Name}は氷壁の冷気を受け、{damage}ダメージと凍傷（{frostbiteDuration}ターン）を受けた。");
            }
        }
    }
}
