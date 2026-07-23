using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPiercingPositionAttackEffect", menuName = "Card Game/Effects/Piercing Position Attack")]
    public sealed class PiercingPositionAttackEffectSO : ActionEffectSO
    {
        public override string[] ParameterFields => new[] { "damage", "maxRange" };

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            int damage = Mathf.Max(1, parameters?.damage ?? 1);
            int maxRange = Mathf.Max(1, parameters?.maxRange ?? 2);
            return new ePiercingPositionAttack(damage, maxRange);
        }
    }
}
