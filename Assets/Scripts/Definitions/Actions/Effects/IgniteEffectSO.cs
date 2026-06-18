using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewIgniteEffect", menuName = "Card Game/Effects/Ignite")]
    public class IgniteEffectSO : ActionEffectSO
    {
        public IgniteLevelData level1 = new IgniteLevelData { duration = 2, damage = 5 };
        public IgniteLevelData level2 = new IgniteLevelData { duration = 3, damage = 10 };
        public IgniteLevelData level3 = new IgniteLevelData { duration = 4, damage = 15 };

        /// <summary>
        /// 指定レベルの炎上効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            var data = GetDataForLevel(level);
            return new eIgnite(data.duration, data.damage);
        }

        /// <summary>
        /// 指定レベルに対応する炎上パラメータを取得する。
        /// </summary>
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

    [System.Serializable]
    public class IgniteLevelData
    {
        [Min(1)]
        public int duration = 1;

        [Min(0)]
        public int damage = 1;
    }
}
