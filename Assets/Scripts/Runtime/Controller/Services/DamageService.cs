using TestCardGame.Character;
using TestCardGame.Definitions.StatusEffects;
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
        public void DealDamage(IUnit source, IUnit target, int amount, DamageType damageType)
        {
            if (target == null) return;

            float calculatedAmount = amount;
            if (source != null)
            {
                bool sourceIsWeak = false;
                foreach (var effect in source.StatusEffects)
                {
                    if (effect.Definition is WeakStatusEffectSO || effect.Definition.EffectId == "Weak")
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
                if (effect.Definition is WeakStatusEffectSO || effect.Definition.EffectId == "Weak")
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
            target.Hp.TakeDamage(finalAmount);

            string sourceName = source != null ? source.Name : "状態異常/環境効果";
            UnityEngine.Debug.Log($"{sourceName}は{target.Name}に {finalAmount} (元: {amount}) ポイントの [{damageType}] ダメージを与えました。残りHP: {target.Hp.CurrentValue}");
        }
    }
}
