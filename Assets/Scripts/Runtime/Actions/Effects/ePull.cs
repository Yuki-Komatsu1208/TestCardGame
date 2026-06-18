using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class ePull : ActionEffect
    {
        private readonly int distance;

        public ePull(int distance)
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
                Debug.LogWarning("Pull: No occupant to pull.");
                return;
            }

            var targetUnit = cell.Occupant;
            var user = context.User;

            Vector2Int diff = user.Position - targetUnit.Position;
            if (diff == Vector2Int.zero)
            {
                return;
            }

            Vector2Int dir = Normalize(diff);
            int currentDist = Mathf.Abs(diff.x) + Mathf.Abs(diff.y);

            // Do not pull past the user
            int pullSteps = Mathf.Min(distance, currentDist - 1);
            if (pullSteps <= 0)
            {
                Debug.Log("Pull: Already adjacent, no need to pull.");
                return;
            }

            Vector2Int targetDest = targetUnit.Position + dir * pullSteps;

            Debug.Log($"Pull: Pulling {targetUnit.Name} from {targetUnit.Position} in direction {dir} by {pullSteps} cells to {targetDest}");
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