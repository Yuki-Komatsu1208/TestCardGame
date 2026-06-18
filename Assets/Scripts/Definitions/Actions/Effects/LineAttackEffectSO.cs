using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewLineAttackEffect", menuName = "Card Game/Effects/Line Attack")]
    public class LineAttackEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 15;
        [SerializeField, Min(1)] private int defaultRange = 1;
        [SerializeField] private HitType defaultHitType = HitType.FirstTargetOnly;

        public override string[] ParameterFields => new[] { "damage", "range", "hitType" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.range = defaultRange;
            parameters.hitType = defaultHitType;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int range = Mathf.Max(1, parameters?.range ?? defaults.range);
            var hitType = parameters?.hitType ?? defaults.hitType;
            return new eLineAttack(damage, range, hitType);
        }
    }
}
