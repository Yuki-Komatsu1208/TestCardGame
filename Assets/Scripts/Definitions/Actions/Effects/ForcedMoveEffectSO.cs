using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 強制移動効果を生成するアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewForcedMoveEffect", menuName = "Card Game/Effects/Forced Move")]
    public class ForcedMoveEffectSO : ActionEffectSO
    {
        /// <summary>
        /// 実行時の強制移動効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new eForcedMove();
        }
    }
}
