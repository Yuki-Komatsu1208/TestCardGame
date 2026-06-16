using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 何もせず行動を消費する効果。
    /// </summary>
    public sealed class eSleep : ActionEffect
    {
        public override bool CanExecute(ActionContext context) => true;

        public override void Execute(ActionContext context)
        {
            Debug.Log($"{context.User.Name}はおやすみしています。");
        }
    }
}
