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
                bool sourceIsWeak = false;
                foreach (var effect in source.StatusEffects)
                {
                    if (effect.Id == StatusEffectId.Weak)
                    {
                        sourceIsWeak = true;
                        break;
                    }
                }
                if (sourceIsWeak)
                {
                    calculatedAmount *= 0.5f;
                }
            }

            bool targetIsWeak = false;
            foreach (var effect in target.StatusEffects)
            {
                if (effect.Id == StatusEffectId.Weak)
                {
                    targetIsWeak = true;
                    break;
                }
            }
            if (targetIsWeak)
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
                    if (effect.Id == StatusEffectId.Shield)
                    {
                        if (effect.RemainingTurns > 0 && effect.Value > 0)
                        {
                            shieldModified = true;
                            if (finalAmount >= effect.Value)
                            {
                                finalAmount -= effect.Value;
                                effect.Value = 0;
                                effect.RemainingTurns = 0;
                            }
                            else
                            {
                                effect.Value -= finalAmount;
                                finalAmount = 0;
                            }
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
