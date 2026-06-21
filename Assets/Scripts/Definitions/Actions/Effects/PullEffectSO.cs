using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 引き寄せ効果の初期距離を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewPullEffect", menuName = "Card Game/Effects/Pull")]
    public class PullEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultDistance = 1;

        public override string[] ParameterFields => new[] { "distance" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.distance = defaultDistance;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int distance = Mathf.Max(1, parameters?.distance ?? defaults.distance);
            return new ePull(distance);
        }
    }
}
