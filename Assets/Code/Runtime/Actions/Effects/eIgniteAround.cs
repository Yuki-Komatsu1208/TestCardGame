using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者の上下左右4マスに炎上効果を付与する効果。
    /// </summary>
    public sealed class eIgniteAround : ActionEffect, IActionPreviewProvider
    {
        private static readonly Vector2Int[] AroundOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private readonly eIgnite ignite;

        /// <summary>
        /// 周囲に付与する炎上の持続ターンとダメージを指定して効果を作成する。
        /// </summary>
        public eIgniteAround(int duration, int damage)
        {
            ignite = new eIgnite(duration, damage);
        }

        /// <summary>
        /// 使用者の上下左右に盤面セルが1つ以上あるか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                if (context.MoveService.GetCellAt(context.User.Position + offset) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 使用者の上下左右4マスへ炎上効果を順番に適用する。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            foreach (var offset in AroundOffsets)
            {
                var targetPosition = context.User.Position + offset;
                var actionContext = new ActionContext(context.MoveService, context.User, targetPosition, context.StatusEffectService);
                if (ignite.CanExecute(actionContext))
                {
                    ignite.Execute(actionContext);
                }
            }
        }

        /// <summary>使用者の上下左右に存在するセルを列挙する。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            if (context?.User == null || context.MoveService == null) return ActionPreviewResult.None;
            var affected = new List<Vector2Int>();
            foreach (Vector2Int offset in AroundOffsets)
            {
                Vector2Int position = context.User.Position + offset;
                if (context.MoveService.GetCellAt(position) != null) affected.Add(position);
            }
            return new ActionPreviewResult(true, new[] { context.User.Position }, affected);
        }
    }
}
