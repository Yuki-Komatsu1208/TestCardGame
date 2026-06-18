using System.Collections.Generic;
using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class eTeleportTarget : ActionEffect
    {
        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null)
            {
                return;
            }

            if (cell.Occupant != null)
            {
                // Teleport this target unit to a random empty cell
                var targetUnit = cell.Occupant;
                var emptyCells = new List<Vector2Int>();
                // We'll scan the board for empty walkable cells
                // We can search adjacent or general cells. Let's find any empty cell!
                // To keep it simple, we scan the WxH grid (we can assume 5x5 if we don't have board size, 
                // but let's query the moveService's cell coordinates or hardcode up to 10x10)
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        var boardCell = context.MoveService.GetCellAt(new Vector2Int(x, y));
                        if (boardCell != null && boardCell.CanMove)
                        {
                            emptyCells.Add(new Vector2Int(x, y));
                        }
                    }
                }

                if (emptyCells.Count > 0)
                {
                    var randCell = emptyCells[Random.Range(0, emptyCells.Count)];
                    Debug.Log($"Teleport: Teleporting target {targetUnit.Name} from {targetUnit.Position} to random cell {randCell}");
                    context.MoveService.RequestForcedMove(targetUnit.ID, randCell);
                }
                else
                {
                    Debug.LogWarning("Teleport: No empty cell found to teleport target.");
                }
            }
            else
            {
                // Teleport the user to the target empty cell
                Debug.Log($"Teleport: Teleporting user {context.User.Name} from {context.User.Position} to empty cell {context.TargetPosition}");
                context.MoveService.RequestForcedMove(context.User.ID, context.TargetPosition);
            }
        }
    }
}