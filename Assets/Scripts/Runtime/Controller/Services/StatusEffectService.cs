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

        public void ApplyStatus(IUnit unit, StatusEffectSO status, int duration, int value = 0)
        {
            if (status == null)
            {
                Debug.LogWarning("StatusEffectService: Status definition is not set.");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(status, duration, value));
        }

        public bool CanAct(IUnit unit)
        {
            if (unit == null) return true;
            return unit.CanAct;
        }

        public void ApplyBurn(IUnit unit, int duration, int damage)
        {
            if (burnDefinition == null)
            {
                Debug.LogWarning("StatusEffectService: BurnDefinition is not set.");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(burnDefinition, duration, damage));
        }

        public void ApplySleep(IUnit unit, int duration)
        {
            if (sleepDefinition == null)
            {
                Debug.LogWarning("StatusEffectService: SleepDefinition is not set.");
                return;
            }
            unit.ApplyStatusEffect(new StatusEffectInstance(sleepDefinition, duration));
        }

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

        private void CleanExpiredEffects(IUnit unit)
        {
            unit.CleanExpiredStatusEffects();
        }
    }
}
