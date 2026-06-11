using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Cards.Effects
{
    public sealed class eMove : CardEffect
    {
        private readonly int step;
        public eMove(int step) { this.step = step; }

        public override void Execute(CardContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int diff = context.TargetPosition - userPos;
            if (diff == Vector2Int.zero) return;

            Vector2Int dir = Normalize(diff);
            int distance = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? Mathf.Abs(diff.x) : Mathf.Abs(diff.y);
            int actualStep = Mathf.Min(distance, step);

            context.MoveService.RequestMoveRelative(context.User.ID, dir * actualStep);
        }

        private static Vector2Int Normalize(Vector2Int d)
            => Mathf.Abs(d.x) >= Mathf.Abs(d.y)
                ? new Vector2Int(d.x == 0 ? 0 : (d.x > 0 ? 1 : -1), 0)
                : new Vector2Int(0, d.y > 0 ? 1 : -1);
    }
}