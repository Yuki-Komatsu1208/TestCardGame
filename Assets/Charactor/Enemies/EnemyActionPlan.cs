using TestCardGame.Actions.Core;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 敵が実行するActionと、その対象指定方法の組み合わせ。
    /// </summary>
    public class EnemyActionPlan
    {
        public ActionEffect Action { get; }
        private readonly TargetKind targetKind;

        private enum TargetKind
        {
            Self,
            Target
        }

        private EnemyActionPlan(ActionEffect action, TargetKind targetKind)
        {
            Action = action;
            this.targetKind = targetKind;
        }

        /// <summary>
        /// 自分自身を対象にするActionPlanを作成する。
        /// </summary>
        public static EnemyActionPlan Self(ActionEffect action)
        {
            return new EnemyActionPlan(action, TargetKind.Self);
        }

        /// <summary>
        /// 敵が狙っている対象ユニットを対象にするActionPlanを作成する。
        /// </summary>
        public static EnemyActionPlan Target(ActionEffect action)
        {
            return new EnemyActionPlan(action, TargetKind.Target);
        }

        /// <summary>
        /// 対象を決め、Action側の実行可否を確認してから実行する。
        /// </summary>
        public bool TryExecute(EnemyTurnContext context)
        {
            if (Action == null)
            {
                return false;
            }

            if (!TryResolveTarget(context, out var targetPosition))
            {
                return false;
            }

            var actionContext = new ActionContext(context.MoveService, context.Enemy, targetPosition);
            if (!Action.CanExecute(actionContext))
            {
                return false;
            }

            Action.Execute(actionContext);
            return true;
        }

        /// <summary>
        /// ActionPlanの対象指定方法から、実際の対象座標を決める。
        /// 新しい対象指定が必要になったらここへ追加する。
        /// </summary>
        private bool TryResolveTarget(EnemyTurnContext context, out Vector2Int targetPosition)
        {
            targetPosition = default;

            switch (targetKind)
            {
                case TargetKind.Self:
                    targetPosition = context.Enemy.Position;
                    return true;
                case TargetKind.Target:
                    if (context.Target == null)
                    {
                        return false;
                    }

                    targetPosition = context.Target.Position;
                    return true;
                default:
                    return false;
            }
        }
    }
}
