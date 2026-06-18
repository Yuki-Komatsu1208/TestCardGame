using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class ePush : ActionEffect
    {
        private readonly int distance;

        public ePush(int distance)
        {
            this.distance = distance;
        }

        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("Push: No occupant to push.");
                return;
            }

            var targetUnit = cell.Occupant;
            var user = context.User;

            Vector2Int diff = targetUnit.Position - user.Position;
            if (diff == Vector2Int.zero)
            {
                // Fallback cardinal direction
                diff = Vector2Int.up;
            }

            Vector2Int dir = Normalize(diff);
            Vector2Int targetDest = targetUnit.Position + dir * distance;

            Debug.Log($"Push: Pushing {targetUnit.Name} from {targetUnit.Position} in direction {dir} by {distance} cells to {targetDest}");
            context.MoveService.RequestForcedMove(targetUnit.ID, targetDest);
        }

        private static Vector2Int Normalize(Vector2Int d)
        {
            if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            {
                return new Vector2Int(d.x == 0 ? 0 : (d.x > 0 ? 1 : -1), 0);
            }
            else
            {
                return new Vector2Int(0, d.y == 0 ? 0 : (d.y > 0 ? 1 : -1));
            }
        }
    }
}