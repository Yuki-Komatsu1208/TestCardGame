using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 精神統一効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicConcentrateEffect", menuName = "Card Game/Effects/Magic Concentrate")]
    public class MagicConcentrateEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultFocusAmount = 2;

        /// <summary>
        /// Unity上でカードごとに編集する項目を返す。
        /// </summary>
        public override string[] ParameterFields => new[] { "value" };

        /// <summary>
        /// 新規カード効果へ精神統一の初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.value = defaultFocusAmount;
        }

        /// <summary>
        /// カード側の調整値から実行時精神統一効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int focusAmount = Mathf.Max(1, parameters?.value ?? defaults.value);
            return new eMagicConcentrate(focusAmount);
        }
    }
}
