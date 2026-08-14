using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：ライトニング効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicLightningEffect", menuName = "Card Game/Effects/Magic Lightning")]
    public class MagicLightningEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamagePerStrike = 15;
        [SerializeField, Min(1)] private int defaultRange = 3;
        [SerializeField, Min(1)] private int defaultStrikeCount = 3;

        /// <summary>
        /// Unity上でカードごとに編集する項目を返す。
        /// </summary>
        public override string[] ParameterFields => new[] { "damage", "range", "count" };

        /// <summary>
        /// 新規カード効果へライトニングの初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamagePerStrike;
            parameters.range = defaultRange;
            parameters.count = defaultStrikeCount;
        }

        /// <summary>
        /// カード側の調整値から実行時ライトニング効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int range = Mathf.Max(1, parameters?.range ?? defaults.range);
            int strikeCount = Mathf.Max(1, parameters?.count ?? defaults.count);
            return new eMagicLightning(damage, range, strikeCount);
        }
    }
}
