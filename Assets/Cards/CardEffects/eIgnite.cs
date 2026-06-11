using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Cards.Effects
{
    public sealed class eIgnite : CardEffect
    {
        private readonly int duration;
        private readonly int damage;

        public eIgnite(int duration, int damage)
        {
            this.duration = duration;
            this.damage = damage;
        }

        public override void Execute(CardContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int targetPos = context.TargetPosition;

            // Check if within 2 cells (Manhattan distance <= 2)
            int dist = Mathf.Abs(targetPos.x - userPos.x) + Mathf.Abs(targetPos.y - userPos.y);
            if (dist <= 2)
            {
                var cell = context.MoveService.GetCellAt(targetPos);
                if (cell != null)
                {
                    cell.ApplyFire(duration, damage);
                    Debug.Log($"Ignited cell ({targetPos.x}, {targetPos.y}) for {duration} turns dealing {damage} damage/turn.");
                }
            }
            else
            {
                Debug.LogWarning("Ignite target is too far away (max 2 cells).");
            }
        }
    }
}