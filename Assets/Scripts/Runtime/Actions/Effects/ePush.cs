using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class ePush : ActionEffect
    {
        private readonly int distance;

        /// <summary>
        /// 押し出す距離を指定して効果を作成する。
        /// </summary>
        public ePush(int distance)
        {
            this.distance = distance;
        }

        /// <summary>
        /// 対象マスにユニットが存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            return cell != null && cell.Occupant != null;
        }

        /// <summary>
        /// 対象ユニットを使用者から離れる方向へ強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("押し出し効果: 対象マスに押し出すユニットがいません。");
                return;
            }

            var targetUnit = cell.Occupant;
            var user = context.User;

            Vector2Int diff = targetUnit.Position - user.Position;
            if (diff == Vector2Int.zero)
            {
                // 同じ座標の場合は予備方向として上方向を使う。
                diff = Vector2Int.up;
            }

            Vector2Int dir = Normalize(diff);
            Vector2Int targetDest = targetUnit.Position + dir * distance;

            Debug.Log($"押し出し効果: {targetUnit.Name} を {targetUnit.Position} から方向 {dir} に {distance} マス移動し、{targetDest} へ押し出します。");
            context.MoveService.RequestForcedMove(targetUnit.ID, targetDest);
        }

        /// <summary>
        /// 指定ベクトルを上下左右の単位方向に正規化する。
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
