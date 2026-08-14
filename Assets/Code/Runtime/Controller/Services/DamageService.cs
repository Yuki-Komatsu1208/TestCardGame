using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    public enum DamageType
    {
        Normal,
        Fire,
        Poison
    }

    /// <summary>
    /// すべてのダメージ計算と適用を一括して行うサービス。
    /// バリア、弱点、防御力などの計算を割り込ませやすい。
    /// </summary>
    public class DamageService
    {
        /// <summary>
        /// 状態異常などの補正を計算したうえで、対象へダメージを適用する。
        /// </summary>
        public void DealDamage(IUnit source, IUnit target, int amount, DamageType damageType)
        {
            if (target == null) return;

            float calculatedAmount = amount;
            if (source != null)
            {
                bool sourceIsWeakened = false;
                int sourcePower = 0;
                foreach (var effect in source.StatusEffects)
                {
                    if (effect.Id == StatusEffectId.Weakened)
                    {
                        sourceIsWeakened = true;
                    }
                    else if (effect.Id == StatusEffectId.Power)
                    {
                        sourcePower += effect.Value;
                    }
                }
                calculatedAmount += sourcePower;
                if (sourceIsWeakened)
                {
                    calculatedAmount *= 0.5f;
                }
            }

            bool targetIsVulnerable = false;
            bool targetIsStunned = false;
            foreach (var effect in target.StatusEffects)
            {
                if (effect.Id == StatusEffectId.Vulnerable)
                {
                    targetIsVulnerable = true;
                }
                else if (effect.Id == StatusEffectId.Stun)
                {
                    targetIsStunned = true;
                }
            }
            if (targetIsVulnerable && targetIsStunned)
            {
                // 脆弱とスタンの被ダメージ増加は加算し、合計2.0倍に固定する。
                calculatedAmount *= 2.0f;
            }
            else if (targetIsVulnerable || targetIsStunned)
            {
                calculatedAmount *= 1.5f;
            }

            int finalAmount = Mathf.Max(0, Mathf.RoundToInt(calculatedAmount));

            if (finalAmount > 0)
            {
                bool shieldModified = false;
                for (int i = 0; i < target.StatusEffects.Count; i++)
                {
                    var effect = target.StatusEffects[i];
                    int remainingAmount = effect.AbsorbDamage(finalAmount);
                    if (remainingAmount != finalAmount)
                    {
                        shieldModified = true;
                        finalAmount = remainingAmount;

                        if (finalAmount <= 0)
                        {
                            break;
                        }
                    }
                }
                if (shieldModified)
                {
                    target.CleanExpiredStatusEffects();
                }
            }

            target.Hp.TakeDamage(finalAmount);

            string sourceName = source != null ? source.Name : "状態異常/環境効果";
            UnityEngine.Debug.Log($"{sourceName}は{target.Name}に {finalAmount} (元: {amount}) ポイントの [{GetDamageTypeLabel(damageType)}] ダメージを与えました。残りHP: {target.Hp.CurrentValue}");
        }

        /// <summary>
        /// ダメージ種別を表示用の日本語名に変換する。
        /// </summary>
        private static string GetDamageTypeLabel(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Fire:
                    return "炎";
                case DamageType.Poison:
                    return "毒";
                default:
                    return "通常";
            }
        }
    }
}
