using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewLineAttackEffect", menuName = "Card Game/Effects/Line Attack")]
    public class LineAttackEffectSO : ActionEffectSO
    {
        public LineAttackLevelData level1 = new LineAttackLevelData { damage = 15, range = 1 };
        public LineAttackLevelData level2 = new LineAttackLevelData { damage = 20, range = 1 };
        public LineAttackLevelData level3 = new LineAttackLevelData { damage = 25, range = 1 };

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            var data = GetDataForLevel(level);
            return new eLineAttack(data.damage, data.range, data.hitType);
        }

        private LineAttackLevelData GetDataForLevel(int level)
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
    public class LineAttackLevelData
    {
        [Min(0)]
        public int damage = 1;

        [Min(1)]
        public int range = 1;

        public HitType hitType = HitType.FirstTargetOnly;
    }
}
