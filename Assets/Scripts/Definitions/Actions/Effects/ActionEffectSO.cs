using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public abstract class ActionEffectSO : ScriptableObject
    {
        /// <summary>
        /// 指定レベルに応じた実行時効果を作成する。
        /// </summary>
        public abstract ActionEffect CreateRuntimeEffect(int level = 1);

        /// <summary>
        /// カードレベルを有効範囲の1～3に丸める。
        /// </summary>
        protected static int ClampLevel(int level)
        {
            return Mathf.Clamp(level, 1, 3);
        }
    }
}
