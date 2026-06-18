using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPullEffect", menuName = "Card Game/Effects/Pull")]
    public class PullEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDistance = 1;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int distance = baseDistance + (level - 1);
            return new ePull(distance);
        }
    }
}