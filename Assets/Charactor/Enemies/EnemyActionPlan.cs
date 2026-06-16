using TestCardGame.Actions.Core;
using TestCardGame.Actions.Effects;
using TestCardGame.Charactor.Enemies.Targeting;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 敵が実行するActionと、その対象指定ロジックの組み合わせ。
    /// </summary>
    public class EnemyActionPlan
    {
        public ActionEffect Action { get; }
        public IEnemyTargetSelector TargetSelector { get; }

        public EnemyActionPlan(ActionEffect action, IEnemyTargetSelector targetSelector)
        {
            Action = action;
            TargetSelector = targetSelector;
        }

        /// <summary>
        /// 対象を決め、Action側の実行可否を確認してから実行する。
        /// </summary>
        public bool TryExecute(EnemyTurnContext context)
        {
            if (Action == null || TargetSelector == null)
            {
                return false;
            }

            if (!TargetSelector.TrySelectTarget(context, out var targetPosition))
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
    }
}
