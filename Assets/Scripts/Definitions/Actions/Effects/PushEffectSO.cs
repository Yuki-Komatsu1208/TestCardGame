using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPushEffect", menuName = "Card Game/Effects/Push")]
    public class PushEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDistance = 1;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int distance = baseDistance + (level - 1);
            return new ePush(distance);
        }
    }
}