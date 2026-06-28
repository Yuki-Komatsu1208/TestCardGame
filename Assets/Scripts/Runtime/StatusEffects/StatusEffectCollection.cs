using System.Collections.Generic;
using TestCardGame.Character;
using UnityEngine;

namespace TestCardGame.Character.StatusEffects
{
    /// <summary>
    /// ユニットに付与されている状態異常の保持、統合、削除、行動可否判定をまとめる。
    /// </summary>
    public class StatusEffectCollection
    {
        private readonly List<StatusEffectInstance> effects = new();

        public IReadOnlyList<StatusEffectInstance> Effects => effects;

        public void Apply(IUnit owner, StatusEffectInstance effect)
        {
            if (owner == null || effect == null) return;

            var existing = effects.Find(e => e.Id == effect.Id);
            if (existing != null)
            {
                existing.Merge(effect);
                Debug.Log($"{owner.Name}の既存の状態異常 {effect.DisplayName} が更新されました（残り持続: {existing.RemainingTurns}ターン）。");
            }
            else
            {
                effects.Add(effect);
                Debug.Log($"{owner.Name}に状態異常 {effect.DisplayName} が付与されました（持続: {effect.RemainingTurns}ターン）。");
            }
        }

        public void CleanExpired(IUnit owner)
        {
            if (owner == null) return;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var effect = effects[i];
                if (effect.IsExpired)
                {
                    effects.RemoveAt(i);
                    Debug.Log($"{owner.Name}の状態異常 {effect.DisplayName} が終了しました。");
                }
            }
        }

        public bool CanAct(IUnit owner)
        {
            foreach (var effect in effects)
            {
                if (!effect.CanAct())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
