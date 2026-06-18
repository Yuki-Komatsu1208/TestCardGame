using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewSleepEffect", menuName = "Card Game/Effects/Sleep")]
    public class SleepEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDuration = 1;

        /// <summary>
        /// レベルに応じた持続ターンの睡眠効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int duration = baseDuration + (level - 1);
            return new eSleep(duration);
        }
    }
}
