using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewSleepEffect", menuName = "Card Game/Effects/Sleep")]
    public class SleepEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDuration = 1;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            // Level 1 = baseDuration, Level 2 = baseDuration + 1, Level 3 = baseDuration + 2
            int duration = baseDuration + (level - 1);
            return new eSleep(duration);
        }
    }
}
