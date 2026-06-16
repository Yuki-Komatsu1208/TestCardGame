using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies.Actions
{
    /// <summary>
    /// 何もせずターンを終える敵行動。
    /// </summary>
    public sealed class SleepEnemyAction : EnemyAction
    {
        public SleepEnemyAction()
            : base(new List<ActionEffect>())
        {
        }

        /// <summary>
        /// おやすみは常に実行可能。
        /// </summary>
        public override bool CanExecute(EnemyTurnContext context)
        {
            return true;
        }

        /// <summary>
        /// 行動せず、ログだけを出してターンを消費する。
        /// </summary>
        public override void Execute(EnemyTurnContext context)
        {
            Debug.Log($"{context.Enemy.Name}はおやすみしています。");
        }

        protected override Vector2Int GetTargetPosition(EnemyTurnContext context)
        {
            return context.Enemy.Position;
        }
    }
}
