using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// ノックバック効果の初期値を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewKnockbackEffect", menuName = "Card Game/Effects/Knockback")]
    public class KnockbackEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 0;
        [SerializeField, Min(1)] private int defaultDistance = 1;

        /// <summary>
        /// 既定のダメージ量と距離を更新する。
        /// </summary>
        public void Configure(int damage, int distance)
        {
            defaultDamage = damage;
            defaultDistance = distance;
        }

        public override string[] ParameterFields => new[] { "damage", "distance" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.distance = defaultDistance;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int damage = Mathf.Max(0, parameters?.damage ?? defaults.damage);
            int distance = Mathf.Max(1, parameters?.distance ?? defaults.distance);
            return new eKnockback(damage, distance);
        }
    }
}
