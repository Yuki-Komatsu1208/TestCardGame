using TestCardGame.Cards.Core;
using UnityEngine;
using TestCardGame.Charactor;

namespace TestCardGame.Cards.Effects
{
    public sealed class eAttack : CardEffect
    {
        private readonly int damage;
        public eAttack(int damage) { this.damage = damage; }

        public override void Execute(CardContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int targetPos = context.TargetPosition;

            // Attack adjacent cell (Manhattan distance == 1)
            int dist = Mathf.Abs(targetPos.x - userPos.x) + Mathf.Abs(targetPos.y - userPos.y);
            if (dist == 1)
            {
                var targetUnit = context.MoveService.GetUnitAt(targetPos);
                if (targetUnit != null)
                {
                    targetUnit.Hp.TakeDamage(damage);
                    Debug.Log($"{context.User.Name} attacked {targetUnit.Name} for {damage} damage! Target HP is now {targetUnit.Hp.CurrentValue}.");
                }
                else
                {
                    Debug.Log("No target at cell to attack.");
                }
            }
            else
            {
                Debug.LogWarning("Attack target must be exactly 1 cell away.");
            }
        }
    }
}