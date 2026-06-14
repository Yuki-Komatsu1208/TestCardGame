using TestCardGame.Cards.Core;
using UnityEngine;
using TestCardGame.Charactor;

namespace TestCardGame.Cards.Effects
{
    /// <summary>
    /// 近接攻撃効果。使用者に隣接する指定座標へダメージを与える。
    /// </summary>
    public sealed class eMeleeAttack : CardEffect
    {
        private readonly int damage;
        public eMeleeAttack(int damage) { this.damage = damage; }

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
                    Debug.Log($"{context.User.Name} は{targetUnit.Name} に {damage} ポイントのダメージを与えた。");
                }
                else
                {
                    Debug.Log("近接攻撃の対象がいません。");
                }
            }
            else
            {
                Debug.LogWarning("近接攻撃は隣接マスにしか使用できません。");
            }
        }
    }
}
