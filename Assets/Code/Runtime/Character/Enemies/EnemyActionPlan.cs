using TestCardGame.Actions.Core;
using TestCardGame.Actions.Effects;
using TestCardGame.Actions.Preview;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    /// <summary>
    /// 敵が実行するActionと、その対象指定方法の組み合わせ。
    /// </summary>
    public class EnemyActionPlan
    {
        public ActionEffect Action { get; }
        public EnemyIntentKind IntentKind { get; }
        private readonly TargetKind targetKind;

        private enum TargetKind
        {
            Self,
            Target
        }

        private EnemyActionPlan(ActionEffect action, TargetKind targetKind, EnemyIntentKind intentKind)
        {
            Action = action;
            this.targetKind = targetKind;
            IntentKind = intentKind;
        }

        /// <summary>
        /// 自分自身を対象にするActionPlanを作成する。
        /// </summary>
        public static EnemyActionPlan Self(ActionEffect action, EnemyIntentKind intentKind)
        {
            return new EnemyActionPlan(action, TargetKind.Self, intentKind);
        }

        /// <summary>
        /// 敵が狙っている対象ユニットを対象にするActionPlanを作成する。
        /// </summary>
        public static EnemyActionPlan Target(ActionEffect action, EnemyIntentKind intentKind)
        {
            return new EnemyActionPlan(action, TargetKind.Target, intentKind);
        }

        /// <summary>
        /// 対象解決と実行可否判定だけを行い、行動や盤面状態を変更しない。
        /// </summary>
        public bool TryPreview(EnemyTurnContext context, out EnemyIntentPreview preview)
        {
            preview = null;
            if (Action == null || !TryResolveTarget(context, out Vector2Int targetPosition)) return false;

            var actionContext = new ActionContext(context.MoveService, context.Enemy, targetPosition, context.StatusEffectService);
            if (!Action.CanExecute(actionContext)) return false;

            System.Collections.Generic.IReadOnlyList<Vector2Int> affectedCells = System.Array.Empty<Vector2Int>();
            if (Action is IActionPreviewProvider provider)
            {
                ActionPreviewResult result = provider.GetPreview(new ActionPreviewContext(
                    context.MoveService, context.Enemy, targetPosition, context.StatusEffectService));
                affectedCells = result?.AffectedCells ?? System.Array.Empty<Vector2Int>();
                if (affectedCells.Count == 0 && result?.MoveCandidateCells != null)
                {
                    affectedCells = result.MoveCandidateCells;
                }
            }

            preview = EnemyIntentPreview.Available(context.Enemy.ID, IntentKind, targetPosition, affectedCells);
            return true;
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

            var actionContext = new ActionContext(context.MoveService, context.Enemy, targetPosition, context.StatusEffectService);
            if (!Action.CanExecute(actionContext))
            {
                return false;
            }

            Action.Execute(actionContext);

            // アクションが成功したら、攻撃アニメーションを再生する
            if (TestCardGame.Controller.GameController.Instance != null)
            {
                TestCardGame.Controller.GameController.Instance.PlayUnitAttackAnimation(context.Enemy.ID);
            }

            return true;
        }

        /// <summary>
        /// ActionPlanの対象指定方法から、実際の対象座標を決める。
        /// 新しい対象指定が必要になったらここへ追加する。
        /// </summary>
        internal bool TryResolveTarget(EnemyTurnContext context, out Vector2Int targetPosition)
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
