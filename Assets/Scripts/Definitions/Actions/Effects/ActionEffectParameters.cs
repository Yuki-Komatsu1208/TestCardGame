using System;
using TestCardGame.Definitions.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [Serializable]
    public class ActionEffectParameters
    {
        [Min(1)]
        public int step = 1;

        [Min(0)]
        public int damage = 1;

        [Min(1)]
        public int duration = 1;

        [Min(1)]
        public int range = 1;

        public HitType hitType = HitType.FirstTargetOnly;

        [Min(0)]
        public int radius = 1;

        [Min(1)]
        public int distance = 1;

        [Min(0)]
        public int maxRange;

        public StatusEffectSO statusEffect;

        [Min(0)]
        public int value;
    }
}
