using System.Collections.Generic;
using TestCardGame.Character.Enemies;
using TestCardGame.Rewards;
using UnityEngine;

namespace TestCardGame.Stage
{
    [CreateAssetMenu(fileName = "NewStageDefinition", menuName = "Card Game/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        public string stageName;
        public Vector2Int boardSize = new Vector2Int(5, 5);
        public List<EnemySpawnDefinition> enemies;
        public CardRewardPoolSO rewardPool;
    }
}