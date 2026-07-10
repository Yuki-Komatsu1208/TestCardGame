using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// ユニットに付与されているすべての状態異常をタイミングごとに実行・管理するサービス。
    /// </summary>
    public class StatusEffectService
    {
        public DamageService DamageService { get; }

        public StatusEffectService(DamageService damageService)
        {
            DamageService = damageService;
        }

        /// <summary>
        /// 任意の状態異常を対象ユニットへ付与する。
        /// </summary>
        public void ApplyStatus(IUnit unit, StatusEffectId status, int duration, int value = 0)
        {
            if (unit == null || status == StatusEffectId.None)
            {
                return;
            }

            unit.ApplyStatusEffect(new StatusEffectInstance(status, Mathf.Max(1, duration), Mathf.Max(0, value)));
        }

        /// <summary>
        /// 対象ユニットが行動可能か判定する。
        /// </summary>
        public bool CanAct(IUnit unit)
        {
            if (unit == null) return true;
            return unit.CanAct;
        }

        /// <summary>
        /// 炎上状態を対象ユニットへ付与する。
        /// </summary>
        public void ApplyBurn(IUnit unit, int duration, int damage)
        {
            ApplyStatus(unit, StatusEffectId.Burn, duration, damage);
        }

        /// <summary>
        /// 睡眠状態を対象ユニットへ付与する。
        /// </summary>
        public void ApplySleep(IUnit unit, int duration)
        {
            ApplyStatus(unit, StatusEffectId.Sleep, duration);
        }

        /// <summary>
        /// ターン開始時に発動する状態異常処理を実行する。
        /// </summary>
        public void TickTurnStart(IUnit unit)
        {
            if (unit == null) return;

            var effects = unit.StatusEffects;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (i < effects.Count)
                {
                    OnTurnStart(unit, effects[i]);
                }
            }
            CleanExpiredEffects(unit);
        }

        /// <summary>
        /// ターン終了時に発動する状態異常処理を実行する。
        /// </summary>
        public void TickTurnEnd(IUnit unit)
        {
            if (unit == null) return;

            var effects = unit.StatusEffects;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (i < effects.Count)
                {
                    OnTurnEnd(unit, effects[i]);
                }
            }
            CleanExpiredEffects(unit);
        }

        private void OnTurnStart(IUnit unit, StatusEffectInstance effect)
        {
        }

        private void OnTurnEnd(IUnit unit, StatusEffectInstance effect)
        {
            if (effect.RemainingTurns <= 0)
            {
                return;
            }

            switch (effect.Id)
            {
                case StatusEffectId.Burn:
                    DamageService.DealDamage(null, unit, effect.Value, DamageType.Fire);
                    effect.RemainingTurns--;
                    break;
                case StatusEffectId.Poison:
                    DamageService.DealDamage(null, unit, effect.Value, DamageType.Poison);
                    effect.RemainingTurns--;
                    break;
                case StatusEffectId.Sleep:
                    effect.RemainingTurns--;
                    Debug.Log($"{unit.Name}はおやすみしています（睡眠残り: {effect.RemainingTurns}ターン）。");
                    break;
                case StatusEffectId.Weak:
                    effect.RemainingTurns--;
                    Debug.Log($"{unit.Name}は弱体化しています（弱体化残り: {effect.RemainingTurns}ターン）。");
                    break;
                case StatusEffectId.Shield:
                    effect.RemainingTurns--;
                    break;
                case StatusEffectId.Focus:
                    // 集中はターン経過で減少せず、リソースとして明示的に消費される。
                    break;
                case StatusEffectId.Power:
                    effect.RemainingTurns--;
                    break;
            }
        }

        /// <summary>
        /// 残りターンが尽きた状態異常を取り除く。
        /// </summary>
        private void CleanExpiredEffects(IUnit unit)
        {
            unit.CleanExpiredStatusEffects();
        }
    }
}
