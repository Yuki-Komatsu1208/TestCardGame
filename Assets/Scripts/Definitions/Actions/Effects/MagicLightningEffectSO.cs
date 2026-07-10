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
        [SerializeField, Min(0)] private int defaultFocusCost = 3;

        /// <summary>
        /// Unity上でカードごとに編集する項目を返す。
        /// </summary>
        public override string[] ParameterFields => new[] { "damage", "range", "focusCost" };

        /// <summary>
        /// 新規カード効果へライトニングの初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamagePerStrike;
            parameters.range = defaultRange;
            parameters.focusCost = defaultFocusCost;
        }

        /// <summary>
        /// カード側の調整値から実行時ライトニング効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int range = Mathf.Max(1, parameters?.range ?? defaults.range);
            int focusCost = Mathf.Max(0, parameters?.focusCost ?? defaults.focusCost);
            return new eMagicLightning(damage, range, focusCost);
        }
    }
}
