using System.Collections.Generic;
using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 対象ユニットを隣接する空きマスへランダム移動させる。
    /// </summary>
    public sealed class eForcedMove : ActionEffect
    {
        /// <summary>
        /// 対象マスにユニットがいるか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象の周囲から移動先を選び、強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("強制移動効果: 対象マスにユニットがいません。");
                return;
            }

            var targetUnit = cell.Occupant;
            Vector2Int targetPos = targetUnit.Position;

            // 上下左右の候補を調べる。
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            List<Vector2Int> walkablePositions = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int adjPos = targetPos + dir;
                var adjCell = context.MoveService.GetCellAt(adjPos);
                if (adjCell != null && adjCell.CanMove)
                {
                    walkablePositions.Add(adjPos);
                }
            }

            if (walkablePositions.Count > 0)
            {
                int randomIndex = Random.Range(0, walkablePositions.Count);
                Vector2Int chosenPos = walkablePositions[randomIndex];
                Debug.Log($"強制移動効果: {targetUnit.Name} を {targetPos} から隣接の空きマス {chosenPos} へ強制移動します。");
                context.MoveService.RequestForcedMove(targetUnit.ID, chosenPos);
            }
            else
            {
                Debug.LogWarning($"強制移動効果: {targetUnit.Name} の周囲に移動可能な空きマスがありません。");
            }
        }
    }
}
