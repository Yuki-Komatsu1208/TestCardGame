using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 予兆や待機など、行動前演出のみを行う効果の設定アセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewTelegraphEffect", menuName = "Card Game/Effects/Telegraph")]
    public class TelegraphEffectSO : ActionEffectSO
    {
        [SerializeField, TextArea] private string message =
            "<b><color=orange>[予兆] {user}</color>が次の行動に備えている。</b>";

        /// <summary>
        /// 設定済みメッセージを出す実行時効果を作る。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new eTelegraph(message);
        }
    }
}
