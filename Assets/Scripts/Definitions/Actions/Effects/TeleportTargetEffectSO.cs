using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewTeleportTargetEffect", menuName = "Card Game/Effects/Teleport Target")]
    public class TeleportTargetEffectSO : ActionEffectSO
    {
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            return new eTeleportTarget();
        }
    }
}