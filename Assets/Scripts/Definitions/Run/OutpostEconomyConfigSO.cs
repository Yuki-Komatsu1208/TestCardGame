using UnityEngine;

namespace TestCardGame.Run
{
    [CreateAssetMenu(fileName = "OutpostEconomy", menuName = "Card Game/Outpost Economy")]
    public sealed class OutpostEconomyConfigSO : ScriptableObject
    {
        [Min(0f)] public float innBaseMultiplier = 2f;
        [Min(0f)] public float innExpeditionMultiplier = 0.5f;

        public int GetInnCost(int missingHp, int expeditionIndex)
        {
            return Mathf.CeilToInt(Mathf.Max(0, missingHp) * (innBaseMultiplier + Mathf.Max(0, expeditionIndex) * innExpeditionMultiplier));
        }
    }
}
