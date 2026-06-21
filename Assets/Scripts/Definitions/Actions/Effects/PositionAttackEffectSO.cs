using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 座標指定攻撃の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewPositionAttackEffect", menuName = "Card Game/Effects/Position Attack")]
    public class PositionAttackEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultDamage = 10;
        [SerializeField, Min(0)] private int defaultMaxRange = 1;

        /// <summary>
        /// 既定のダメージと射程を更新する。
        /// </summary>
        public void Configure(int damage, int maxRange)
        {
            defaultDamage = damage;
            defaultMaxRange = maxRange;
        }

        public override string[] ParameterFields => new[] { "damage", "maxRange" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.maxRange = defaultMaxRange;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(1, parameters?.damage ?? defaults.damage);
            int maxRange = Mathf.Max(0, parameters?.maxRange ?? defaults.maxRange);
            return new ePositionAttack(damage, maxRange > 0 ? (int?)maxRange : null);
        }
    }
}
