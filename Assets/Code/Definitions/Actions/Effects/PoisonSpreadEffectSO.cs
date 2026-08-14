using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 毒拡散効果の設定を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoisonSpreadEffect", menuName = "Card Game/Effects/Poison Spread")]
    public class PoisonSpreadEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int duration = 3;
        [SerializeField, Min(1)] private int value = 2;

        /// <summary>
        /// 毒の定義と付与量をまとめて更新する。
        /// </summary>
        public void Configure(int duration, int value)
        {
            this.duration = duration;
            this.value = value;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new ePoisonSpread(StatusEffectId.Poison, duration, value);
        }
    }
}
