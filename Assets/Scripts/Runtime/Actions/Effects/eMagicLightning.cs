using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：ライトニングの効果。
    /// 集中を消費し、指定セルのユニットへ3回ダメージを与える。
    /// </summary>
    public sealed class eMagicLightning : ActionEffect
    {
        private readonly int damagePerStrike;
        private readonly int range;
        private readonly int baseStrikeCount;

        /// <summary>
        /// ライトニングの単発ダメージ、射程、基本ヒット数を初期化する。
        /// </summary>
        public eMagicLightning(int damagePerStrike, int range, int baseStrikeCount)
        {
            this.damagePerStrike = damagePerStrike;
            this.range = range;
            this.baseStrikeCount = Mathf.Max(1, baseStrikeCount);
        }

        /// <summary>
        /// 対象座標が盤面内かつ射程内か確認する。
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

            return distance <= range;
        }

        /// <summary>
        /// 現在の集中をすべて消費し、その量だけヒット数を増やして発動する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("指定されたライトニングの射線/座標が射程外、または盤面外です。");
                return;
            }

            IUnit user = context.User;
            int spentFocus = MagicFocusHelper.ConsumeAllFocus(user);
            int strikeCount = baseStrikeCount + spentFocus;
            Debug.Log($"{user.Name}は集中を{spentFocus}解放して極大魔法：ライトニングを詠唱した！ (雷撃回数: {strikeCount})");

            Vector2Int targetPosition = context.TargetPosition;
            var targetUnit = context.MoveService.GetUnitAt(targetPosition);

            if (targetUnit == null)
            {
                Debug.Log("ライトニングの対象座標にユニットがいません。雷は地面を焦がした。");
                return;
            }

            // 同じ対象に連続で命中させ、途中で倒れたら追撃を止める。
            for (int i = 1; i <= strikeCount; i++)
            {
                if (targetUnit.Hp.CurrentValue <= 0)
                {
                    Debug.Log($"{targetUnit.Name}は既に倒れているため、追撃は発生しません。");
                    break;
                }

                // サービスがある場合は共通ダメージ計算を通し、なければ直接HPを減らす。
                if (context.StatusEffectService?.DamageService != null)
                {
                    context.StatusEffectService.DamageService.DealDamage(user, targetUnit, damagePerStrike, TestCardGame.Controller.Services.DamageType.Normal);
                }
                else
                {
                    targetUnit.Hp.TakeDamage(damagePerStrike);
                }

                Debug.Log($"ライトニング第 {i} 撃！ {targetUnit.Name}に {damagePerStrike} ダメージ！ (残りHP: {targetUnit.Hp.CurrentValue})");
            }
        }
    }
}
