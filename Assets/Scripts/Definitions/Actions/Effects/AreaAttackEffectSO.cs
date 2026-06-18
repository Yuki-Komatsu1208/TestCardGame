using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewAreaAttackEffect", menuName = "Card Game/Effects/Area Attack")]
    public class AreaAttackEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 10;
        [SerializeField, Min(0)] private int defaultRadius = 1;

        public override string[] ParameterFields => new[] { "damage", "radius" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.radius = defaultRadius;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int radius = Mathf.Max(0, parameters?.radius ?? defaults.radius);
            return new eAreaAttack(damage, radius);
        }
    }
}
