using TestCardGame.Definitions.StatusEffects;
using TestCardGame.Controller.Services;

namespace TestCardGame.Character.StatusEffects
{
    public class StatusEffectInstance
    {
        public StatusEffectSO Definition { get; }
        public int RemainingTurns { get; set; }
        public int Value { get; set; } // Generic intensity value (e.g., Burn damage)

        public bool IsExpired => RemainingTurns <= 0;

        public StatusEffectInstance(StatusEffectSO definition, int remainingTurns, int value = 0)
        {
            Definition = definition;
            RemainingTurns = remainingTurns;
            Value = value;
        }

        public void Merge(StatusEffectInstance other)
        {
            Definition.Merge(this, other);
        }

        public void OnTurnStart(IUnit unit, StatusEffectService service)
        {
            Definition.OnTurnStart(unit, this, service);
        }

        public void OnTurnEnd(IUnit unit, StatusEffectService service)
        {
            Definition.OnTurnEnd(unit, this, service);
        }

        public bool CanAct(IUnit unit)
        {
            return Definition.CanAct(unit, this);
        }
    }
}
