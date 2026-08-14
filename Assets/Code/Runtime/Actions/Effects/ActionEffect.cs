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
        /// 指定された対象に対して効果を実行できるか判定する。
        /// </summary>
        public virtual bool CanExecute(ActionContext context) => true;

        /// <summary>
        /// 効果を発動する。
        /// </summary>
        public virtual void Execute(ActionContext context){}

        /// <summary>
        /// 明示的な対象座標を指定して効果を発動する。
        /// </summary>
        public virtual void Execute(ActionContext context, Vector2Int targetPosition){}
    }
}
