using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPushEffect", menuName = "Card Game/Effects/Push")]
    public class PushEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDistance = 1;

        /// <summary>
        /// レベルに応じた距離の押し出し効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int distance = baseDistance + (level - 1);
            return new ePush(distance);
        }
    }
}
