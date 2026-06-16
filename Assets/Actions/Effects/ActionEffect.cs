using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// ゲーム内の行動効果の基本クラス。
    /// </summary>
    public abstract class ActionEffect
    {
        /// <summary>
        /// 効果を発動。
        /// </summary>
        public virtual void Execute(ActionContext context){}
        public virtual void Execute(ActionContext context, Vector2Int targetPosition){}
    }
}
