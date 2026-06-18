using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewTeleportTargetEffect", menuName = "Card Game/Effects/Teleport Target")]
    public class TeleportTargetEffectSO : ActionEffectSO
    {
        /// <summary>
        /// 対象転移効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new eTeleportTarget();
        }
    }
}
