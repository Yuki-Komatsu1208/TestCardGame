using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：アイスウォール効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicIceWallEffect", menuName = "Card Game/Effects/Magic Ice Wall")]
    public class MagicIceWallEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultShieldDuration = 2;
        [SerializeField, Min(0)] private int defaultShieldValue = 15;
        [SerializeField, Min(0)] private int defaultDamage = 10;
        [SerializeField, Min(1)] private int defaultFrostbiteDuration = 2;

        public override string[] ParameterFields => new[] { "duration", "value", "damage", "range" };

        /// <summary>
        /// 新規カード効果へアイスウォールの初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.duration = defaultShieldDuration;
            parameters.value = defaultShieldValue;
            parameters.damage = defaultDamage;
            parameters.range = defaultFrostbiteDuration;
        }

        /// <summary>
        /// カード側の調整値から実行時アイスウォール効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int shieldDuration = Mathf.Max(1, parameters?.duration ?? defaults.duration);
            int shieldValue = Mathf.Max(0, parameters?.value ?? defaults.value);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int frostbiteDuration = Mathf.Max(1, parameters?.range ?? defaults.range);
            return new eMagicIceWall(shieldDuration, shieldValue, damage, frostbiteDuration);
        }
    }
}
