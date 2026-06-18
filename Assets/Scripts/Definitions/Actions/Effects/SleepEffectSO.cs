using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewSleepEffect", menuName = "Card Game/Effects/Sleep")]
    public class SleepEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultDuration = 1;

        public override string[] ParameterFields => new[] { "duration" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.duration = defaultDuration;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int duration = Mathf.Max(1, parameters?.duration ?? defaults.duration);
            return new eSleep(duration);
        }
    }
}
