using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Character.Enemies;
using UnityEngine;
using TestCardGame.Controller;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者の周囲に敵を1体召喚する効果。
    /// </summary>
    public sealed class eSummonEnemy : ActionEffect
    {
        private readonly EnemyDefinitionSO enemyDefinition;

        /// <summary>
        /// 召喚する敵定義を指定する。
        /// </summary>
        public eSummonEnemy(EnemyDefinitionSO enemyDefinition)
        {
            this.enemyDefinition = enemyDefinition;
        }

        /// <summary>
        /// 周囲の空きマスを探して敵を出現させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (enemyDefinition == null || GameController.Instance == null) return;

            Vector2Int userPos = context.User.Position;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            List<Vector2Int> emptyPositions = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int adjPos = userPos + dir;
                var adjCell = context.MoveService.GetCellAt(adjPos);
                if (adjCell != null && adjCell.CanMove && adjCell.Occupant == null)
                {
                    emptyPositions.Add(adjPos);
                }
            }

            if (emptyPositions.Count > 0)
            {
                Vector2Int spawnPos = emptyPositions[Random.Range(0, emptyPositions.Count)];
                GameController.Instance.SpawnEnemyDuringBattle(enemyDefinition, spawnPos);
            }
        }
    }
}
