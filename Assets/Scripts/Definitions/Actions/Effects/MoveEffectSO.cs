using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewMoveEffect", menuName = "Card Game/Effects/Move")]
    public class MoveEffectSO : ActionEffectSO
    {
        [Min(1)]
        public int level1Step = 1;

        [Min(1)]
        public int level2Step = 2;

        [Min(1)]
        public int level3Step = 3;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            var step = GetStepForLevel(level);
            return new eMove(step);
        }

        private int GetStepForLevel(int level)
        {
            switch (ClampLevel(level))
            {
                case 1: return level1Step;
                case 2: return level2Step;
                default: return level3Step;
            }
        }
    }
}
