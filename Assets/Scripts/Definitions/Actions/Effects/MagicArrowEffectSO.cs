using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：魔法の矢効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicArrowEffect", menuName = "Card Game/Effects/Magic Arrow")]
    public class MagicArrowEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 18;
        [SerializeField, Min(1)] private int defaultRange = 3;

        public override string[] ParameterFields => new[] { "damage", "range" };

        /// <summary>
        /// 新規カード効果へ魔法の矢の初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.range = defaultRange;
        }

        /// <summary>
        /// カード側の調整値から実行時の魔法の矢効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int range = Mathf.Max(1, parameters?.range ?? defaults.range);
            return new eMagicArrow(damage, range);
        }
    }
}
