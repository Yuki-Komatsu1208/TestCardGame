using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 移動効果の初期歩数を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMoveEffect", menuName = "Card Game/Effects/Move")]
    public class MoveEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultStep = 1;

        public override string[] ParameterFields => new[] { "step" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.step = defaultStep;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            return new eMove(Mathf.Max(1, parameters?.step ?? defaults.step));
        }
    }
}
