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
        private readonly int focusCost;

        /// <summary>
        /// ライトニングの単発ダメージ、射程、集中コストを初期化する。
        /// </summary>
        public eMagicLightning(int damagePerStrike, int range, int focusCost)
        {
            this.damagePerStrike = damagePerStrike;
            this.range = range;
            this.focusCost = Mathf.Max(0, focusCost);
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
        /// 集中が足りれば発動し、不足時は詠唱として集中を得る。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("指定されたライトニングの射線/座標が射程外、または盤面外です。");
                return;
            }

            IUnit user = context.User;
            int focus = MagicFocusHelper.GetFocusCount(user);

            if (focus >= focusCost)
            {
                // 集中コスト0も許可し、同じ発動処理に乗せる。
                MagicFocusHelper.ConsumeFocus(user, focusCost);
                Debug.Log($"{user.Name}は集中を{focusCost}消費して極大魔法：ライトニングを詠唱した！ (残り集中: {focus - focusCost})");

                Vector2Int targetPosition = context.TargetPosition;
                var targetUnit = context.MoveService.GetUnitAt(targetPosition);

                if (targetUnit == null)
                {
                    Debug.Log("ライトニングの対象座標にユニットがいません。雷は地面を焦がした。");
                    return;
                }

                // 同じ対象に3回命中させ、途中で倒れたら追撃を止める。
                for (int i = 1; i <= 3; i++)
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
            else
            {
                MagicFocusHelper.AddFocus(user, 1);
                Debug.Log($"{user.Name}は十分な集中（現在: {focus}/{focusCost}）を保持していないため、ライトニングの精神統一を行い集中を1獲得した。");
            }
        }
    }
}
