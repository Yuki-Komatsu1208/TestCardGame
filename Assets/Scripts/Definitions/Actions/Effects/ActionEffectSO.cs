using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 実行時のActionEffectを生成する定義アセット。
    /// </summary>
    public abstract class ActionEffectSO : ScriptableObject
    {
        /// <summary>
        /// インスペクタで編集するパラメータ名の一覧。
        /// </summary>
        public virtual string[] ParameterFields => System.Array.Empty<string>();

        /// <summary>
        /// カード側パラメータに、この効果の初期値を設定する。
        /// </summary>
        public virtual void SetDefaultParameters(ActionEffectParameters parameters) { }

        /// <summary>
        /// カード側で編集する初期パラメータを作成する。
        /// </summary>
        public virtual ActionEffectParameters CreateDefaultParameters(int level = 1)
        {
            var parameters = new ActionEffectParameters();
            SetDefaultParameters(parameters);
            return parameters;
        }

        /// <summary>
        /// カード側のパラメータから実行時効果を作成する。
        /// </summary>
        public abstract ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1);

        /// <summary>
        /// デフォルトパラメータから実行時効果を作成する。
        /// </summary>
        public ActionEffect CreateRuntimeEffect(int level = 1)
        {
            return CreateRuntimeEffect(CreateDefaultParameters(level), level);
        }

        /// <summary>
        /// カードレベルを有効範囲の1～3に丸める。
        /// </summary>
        protected static int ClampLevel(int level)
        {
            return Mathf.Clamp(level, 1, 3);
        }
    }
}
