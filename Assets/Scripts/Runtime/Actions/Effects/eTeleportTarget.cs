using System.Collections.Generic;
using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 対象ユニット、または使用者を別のマスへ転移させる。
    /// </summary>
    public sealed class eTeleportTarget : ActionEffect
    {
        /// <summary>
        /// 対象マスが盤面上に存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        /// <summary>
        /// 対象マスのユニット、または使用者自身を空きセルへ強制移動させる。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var cell = context.MoveService.GetCellAt(context.TargetPosition);
            if (cell == null)
            {
                return;
            }

            if (cell.Occupant != null)
            {
                var targetUnit = cell.Occupant;
                var emptyCells = new List<Vector2Int>();
                // 現状は最大10x10を走査して空きセルを探す。
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
                    Debug.Log($"転移効果: 対象 {targetUnit.Name} を {targetUnit.Position} からランダムな空きセル {randCell} へ転移させます。");
                    context.MoveService.RequestForcedMove(targetUnit.ID, randCell);
                }
                else
                {
                    Debug.LogWarning("転移効果: 対象を転移できる空きセルが見つかりませんでした。");
                }
            }
            else
            {
                Debug.Log($"転移効果: 使用者 {context.User.Name} を {context.User.Position} から空きセル {context.TargetPosition} へ転移させます。");
                context.MoveService.RequestForcedMove(context.User.ID, context.TargetPosition);
            }
        }
    }
}
