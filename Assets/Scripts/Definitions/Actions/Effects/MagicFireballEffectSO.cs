using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 魔法：ファイアボール効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicFireballEffect", menuName = "Card Game/Effects/Magic Fireball")]
    public class MagicFireballEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 20;
        [SerializeField, Min(1)] private int defaultRange = 4;
        [SerializeField, Min(1)] private int defaultBurnDuration = 3;
        [SerializeField, Min(1)] private int defaultBurnDamage = 5;
        [SerializeField, Min(0)] private int defaultFocusCost = 1;

        /// <summary>
        /// Unity上でカードごとに編集する項目を返す。
        /// </summary>
        public override string[] ParameterFields => new[] { "damage", "range", "duration", "value", "focusCost" };

        /// <summary>
        /// 新規カード効果へファイアボールの初期値を入れる。
        /// </summary>
        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.range = defaultRange;
            parameters.duration = defaultBurnDuration;
            parameters.value = defaultBurnDamage;
            parameters.focusCost = defaultFocusCost;
        }

        /// <summary>
        /// カード側の調整値から実行時ファイアボール効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int range = Mathf.Max(1, parameters?.range ?? defaults.range);
            int burnDuration = Mathf.Max(1, parameters?.duration ?? defaults.duration);
            int burnDamage = Mathf.Max(0, parameters?.value ?? defaults.value);
            int focusCost = Mathf.Max(0, parameters?.focusCost ?? defaults.focusCost);
            return new eMagicFireball(damage, range, burnDuration, burnDamage, focusCost);
        }
    }
}
