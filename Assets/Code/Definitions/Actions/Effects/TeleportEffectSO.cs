using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewTeleportEffect", menuName = "Card Game/Effects/Teleport")]
    public sealed class TeleportEffectSO : ActionEffectSO
    {
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1) => new eTeleport();
    }
}
