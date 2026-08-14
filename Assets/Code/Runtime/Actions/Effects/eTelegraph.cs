using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 予兆や待機などのログ演出のみを行う効果。
    /// </summary>
    public sealed class eTelegraph : ActionEffect
    {
        private readonly string message;

        /// <summary>
        /// 表示するメッセージを初期化する。
        /// </summary>
        public eTelegraph(string message)
        {
            this.message = string.IsNullOrWhiteSpace(message)
                ? "<b><color=orange>[予兆] {user}</color>が次の行動に備えている。</b>"
                : message;
        }

        /// <summary>
        /// 使用者が存在するか確認する。
        /// </summary>
        public override bool CanExecute(ActionContext context) => context.User != null;

        /// <summary>
        /// メッセージ内の使用者名を差し替えてログに出す。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;
            Debug.Log(message.Replace("{user}", context.User.Name));
        }
    }
}
