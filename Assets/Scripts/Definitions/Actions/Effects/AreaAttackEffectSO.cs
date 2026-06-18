using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewAreaAttackEffect", menuName = "Card Game/Effects/Area Attack")]
    public class AreaAttackEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int baseRadius = 1;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int damage = baseDamage + (level - 1) * 5;
            int radius = baseRadius;
            return new eAreaAttack(damage, radius);
        }
    }
}