using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// ダメージを与えつつ対象を後方へ吹き飛ばす効果。
    /// </summary>
    public sealed class eKnockback : ActionEffect
    {
        private readonly int damage;
        private readonly int distance;

        /// <summary>
        /// ダメージ量と吹き飛ばす距離を指定する。
        /// </summary>
        public eKnockback(int damage, int distance)
        {
            this.damage = damage;
            this.distance = distance;
        }

        /// <summary>
        /// 対象マスにユニットがいるか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象へダメージを与え、使用者から遠ざける。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("ノックバック効果: 対象マスにノックバックするユニットがいません。");
                return;
            }

            var targetUnit = cell.Occupant;
            var user = context.User;

            // ダメージ指定がある場合だけ先に与える。
            if (damage > 0)
            {
                if (context.StatusEffectService?.DamageService != null)
                {
                    context.StatusEffectService.DamageService.DealDamage(user, targetUnit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                }
                else
                {
                    targetUnit.Hp.TakeDamage(damage);
                    Debug.Log($"ノックバック効果: {targetUnit.Name} に {damage} ダメージを与えました。残りHP: {targetUnit.Hp.CurrentValue}");
                }
            }

            // 使用者から対象へ向かう向きに吹き飛ばす。
            Vector2Int diff = targetUnit.Position - user.Position;
            if (diff == Vector2Int.zero)
            {
                diff = Vector2Int.up;
            }

            Vector2Int dir = Normalize(diff);
            Vector2Int targetDest = targetUnit.Position + dir * distance;

            Debug.Log($"ノックバック効果: {targetUnit.Name} を {targetUnit.Position} から方向 {dir} に {distance} マス移動し、{targetDest} へノックバックさせます。");
            context.MoveService.RequestForcedMove(targetUnit.ID, targetDest);
        }

        /// <summary>
        /// ベクトルを上下左右の単位方向へ丸める。
        /// </summary>
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
