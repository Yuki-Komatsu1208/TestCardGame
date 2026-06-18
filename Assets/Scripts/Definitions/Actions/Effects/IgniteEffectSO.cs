using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewIgniteEffect", menuName = "Card Game/Effects/Ignite")]
    public class IgniteEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultDuration = 2;
        [SerializeField, Min(0)] private int defaultDamage = 5;

        public override string[] ParameterFields => new[] { "duration", "damage" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.duration = defaultDuration;
            parameters.damage = defaultDamage;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int duration = Mathf.Max(1, parameters?.duration ?? defaults.duration);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            return new eIgnite(duration, damage);
        }
    }
}
