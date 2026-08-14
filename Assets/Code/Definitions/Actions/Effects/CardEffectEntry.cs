using System;

namespace TestCardGame.Actions.Effects
{
    [Serializable]
    public class CardEffectEntry
    {
        public ActionEffectSO effect;
        public ActionEffectParameters parameters = new ActionEffectParameters();

        public ActionEffect CreateRuntimeEffect(int level = 1)
        {
            return effect != null ? effect.CreateRuntimeEffect(parameters, level) : null;
        }
    }
}
