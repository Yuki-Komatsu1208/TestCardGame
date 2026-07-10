using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.StatusEffects
{
    /// <summary>
    /// 集中スタックの取得、加算、消費をまとめる。
    /// </summary>
    public static class MagicFocusHelper
    {
        /// <summary>
        /// ユニットが持つ集中スタック数を返す。
        /// </summary>
        public static int GetFocusCount(IUnit unit)
        {
            if (unit == null) return 0;
            foreach (var effect in unit.StatusEffects)
            {
                if (effect.Id == StatusEffectId.Focus)
                {
                    return effect.Value;
                }
            }
            return 0;
        }

        /// <summary>
        /// ユニットへ集中スタックを加算する。
        /// </summary>
        public static void AddFocus(IUnit unit, int count)
        {
            if (unit == null || count <= 0) return;
            // 集中はターン減少させないが、初期値として適当なRemainingTurnsを設定し、
            // 毎ターンの減少はStatusEffectService側で無視される。
            unit.ApplyStatusEffect(new StatusEffectInstance(StatusEffectId.Focus, 999, count));
        }

        /// <summary>
        /// ユニットの集中スタックを消費する。
        /// </summary>
        public static void ConsumeFocus(IUnit unit, int count)
        {
            if (unit == null || count <= 0) return;
            foreach (var effect in unit.StatusEffects)
            {
                if (effect.Id == StatusEffectId.Focus)
                {
                    effect.Value -= count;
                    if (effect.Value <= 0)
                    {
                        effect.Value = 0;
                        // 消費しきった集中は既存の期限切れ掃除に任せて消す。
                        effect.RemainingTurns = 0;
                        unit.CleanExpiredStatusEffects();
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// ユニットが持つ集中をすべて消費し、消費した量を返す。
        /// </summary>
        public static int ConsumeAllFocus(IUnit unit)
        {
            int focus = GetFocusCount(unit);
            ConsumeFocus(unit, focus);
            return focus;
        }
    }
}
