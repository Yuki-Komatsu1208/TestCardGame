using System;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    [Serializable]
    public class EnemySpawnDefinition
    {
        public EnemyDefinitionSO enemy;
        public Vector2Int position;
    }
}