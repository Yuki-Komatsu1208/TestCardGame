using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewAreaAttackEffect", menuName = "Card Game/Effects/Area Attack")]
    public class AreaAttackEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int baseRadius = 1;

        /// <summary>
        /// レベルに応じたダメージを持つ範囲攻撃効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int damage = baseDamage + (level - 1) * 5;
            int radius = baseRadius;
            return new eAreaAttack(damage, radius);
        }
    }
}
