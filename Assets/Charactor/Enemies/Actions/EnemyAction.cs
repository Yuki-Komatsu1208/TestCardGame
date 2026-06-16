using System;
using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Actions
{
    /// <summary>
    /// 敵が内部的に実行する行動。
    /// </summary>
    public abstract class EnemyAction
    {
        protected IReadOnlyList<ActionEffect> Effects { get; }

        protected EnemyAction(IReadOnlyList<ActionEffect> effects)
        {
            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            Effects = effects;
        }

        public abstract bool CanExecute(EnemyTurnContext context);

        public virtual void Execute(EnemyTurnContext context)
        {
            var actionContext = new ActionContext(context.MoveService, context.Enemy, GetTargetPosition(context));
            foreach (var effect in Effects)
            {
                effect.Execute(actionContext);
            }
        }

        protected abstract Vector2Int GetTargetPosition(EnemyTurnContext context);
    }
}
