using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public abstract class ActionEffectSO : ScriptableObject
    {
        public abstract ActionEffect CreateRuntimeEffect(int level = 1);

        protected static int ClampLevel(int level)
        {
            return Mathf.Clamp(level, 1, 3);
        }
    }
}
