using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewPullEffect", menuName = "Card Game/Effects/Pull")]
    public class PullEffectSO : ActionEffectSO
    {
        [SerializeField] private int baseDistance = 1;

        /// <summary>
        /// レベルに応じた距離の引き寄せ効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int distance = baseDistance + (level - 1);
            return new ePull(distance);
        }
    }
}
