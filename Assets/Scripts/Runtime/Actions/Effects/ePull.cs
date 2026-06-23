using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 対象ユニットを使用者の近くへ引き寄せる効果。
    /// </summary>
    public sealed class ePull : ActionEffect
    {
        private readonly int distance;

        /// <summary>
        /// 引き寄せる距離を指定して効果を作成する。
        /// </summary>
        public ePull(int distance)
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
        /// 対象ユニットを使用者に近づく方向へ強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null || cell.Occupant == null)
            {
                Debug.LogWarning("引き寄せ効果: 対象マスに引き寄せるユニットがいません。");
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

            // 使用者を通り越さない範囲で引き寄せる。
            int pullSteps = Mathf.Min(distance, currentDist - 1);
            if (pullSteps <= 0)
            {
                Debug.Log("引き寄せ効果: 対象はすでに隣接しているため移動しません。");
                return;
            }

            Vector2Int targetDest = targetUnit.Position + dir * pullSteps;

            Debug.Log($"引き寄せ効果: {targetUnit.Name} を {targetUnit.Position} から方向 {dir} に {pullSteps} マス移動し、{targetDest} へ引き寄せます。");
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
