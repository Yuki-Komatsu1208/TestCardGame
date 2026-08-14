using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// チャージ効果を生成するアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewChargeEffect", menuName = "Card Game/Effects/Charge")]
    public class ChargeEffectSO : ActionEffectSO
    {
        /// <summary>
        /// 実行時のチャージ効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new eCharge();
        }
    }
}
