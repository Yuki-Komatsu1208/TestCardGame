using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Definitions.StatusEffects;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// ユニットに付与されているすべての状態異常をタイミングごとに実行・管理するサービス。
    /// </summary>
    public class StatusEffectService
    {
        public DamageService DamageService { get; }
        private readonly StatusEffectSO burnDefinition;
        private readonly StatusEffectSO sleepDefinition;
        private readonly StatusEffectSO weakDefinition;

        public StatusEffectSO BurnDefinition => burnDefinition;
        public StatusEffectSO SleepDefinition => sleepDefinition;
        public StatusEffectSO WeakDefinition => weakDefinition;

        public StatusEffectService(DamageService damageService, StatusEffectSO burnDefinition, StatusEffectSO sleepDefinition, StatusEffectSO weakDefinition = null)
        {
            DamageService = damageService;
            this.burnDefinition = burnDefinition;
            this.sleepDefinition = sleepDefinition;
            this.weakDefinition = weakDefinition;
        }

        /// <summary>
        /// 任意の状態異常を対象ユニットへ付与する。
        /// </summary>
        public void ApplyStatus(IUnit unit, StatusEffectSO status, int duration, int value = 0)
        {
            if (status == null)
            {
                Debug.LogWarning("StatusEffectService: 状態異常定義が設定されていません。");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(status, duration, value));
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
            if (burnDefinition == null)
            {
                Debug.LogWarning("StatusEffectService: 炎上定義が設定されていません。");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(burnDefinition, duration, damage));
        }

        /// <summary>
        /// 睡眠状態を対象ユニットへ付与する。
        /// </summary>
        public void ApplySleep(IUnit unit, int duration)
        {
            if (sleepDefinition == null)
            {
                Debug.LogWarning("StatusEffectService: 睡眠定義が設定されていません。");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(sleepDefinition, duration));
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
                    effects[i].OnTurnStart(unit, this);
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
                    effects[i].OnTurnEnd(unit, this);
                }
            }
            CleanExpiredEffects(unit);
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
