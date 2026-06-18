using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPositionAttackEffect", menuName = "Card Game/Effects/Position Attack")]
    public class PositionAttackEffectSO : ActionEffectSO
    {
        public PositionAttackLevelData level1 = new PositionAttackLevelData { damage = 10, maxRange = 1 };
        public PositionAttackLevelData level2 = new PositionAttackLevelData { damage = 10, maxRange = 1 };
        public PositionAttackLevelData level3 = new PositionAttackLevelData { damage = 10, maxRange = 1 };

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            var data = GetDataForLevel(level);
            return new ePositionAttack(data.damage, data.maxRange > 0 ? (int?)data.maxRange : null);
        }

        private PositionAttackLevelData GetDataForLevel(int level)
        {
            switch (ClampLevel(level))
            {
                case 1: return level1;
                case 2: return level2;
                default: return level3;
            }
        }
    }

    [System.Serializable]
    public class PositionAttackLevelData
    {
        [Min(1)]
        public int damage = 1;

        [Min(0)]
        public int maxRange;
    }
}
