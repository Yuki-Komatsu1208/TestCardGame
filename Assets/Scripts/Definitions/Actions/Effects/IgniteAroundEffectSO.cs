using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewIgniteAroundEffect", menuName = "Card Game/Effects/Ignite Around")]
    public class IgniteAroundEffectSO : ActionEffectSO
    {
        public IgniteLevelData level1 = new IgniteLevelData { duration = 2, damage = 5 };
        public IgniteLevelData level2 = new IgniteLevelData { duration = 2, damage = 5 };
        public IgniteLevelData level3 = new IgniteLevelData { duration = 2, damage = 5 };

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            var data = GetDataForLevel(level);
            return new eIgniteAround(data.duration, data.damage);
        }

        private IgniteLevelData GetDataForLevel(int level)
        {
            switch (ClampLevel(level))
            {
                case 1: return level1;
                case 2: return level2;
                default: return level3;
            }
        }
    }
}
