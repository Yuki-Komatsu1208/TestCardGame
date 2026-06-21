using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 使用者のチャージ演出だけを行う効果。
    /// </summary>
    public sealed class eCharge : ActionEffect
    {
        /// <summary>
        /// チャージしたことをログに出す。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            Debug.Log($"{context.User.Name} は力をチャージしている！強力な攻撃に備えている！");
        }
    }
}
