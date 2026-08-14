using System;
using System.Collections.Generic;
using TestCardGame.CommonBase;
using UnityEngine;

namespace TestCardGame.Cards.VOs
{
    /// <summary>
    /// カードのクールタイムターン数を表すValueObject。
    /// </summary>
    [Serializable]
    public class CardCooldown : VOBase
    {
        [SerializeField, Min(0)] private int turns;

        public int Turns => Math.Max(0, turns);
        public bool IsActive => Turns > 0;
        public bool IsReady => !IsActive;

        public CardCooldown(int turns)
        {
            if (turns < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(turns), "クールタイムは0以上でなければなりません。");
            }

            this.turns = turns;
        }

        public CardCooldown StartFrom(CardCooldown baseCooldown)
        {
            return new CardCooldown(baseCooldown?.Turns ?? 0);
        }

        public CardCooldown Tick()
        {
            return Turns <= 0 ? None : new CardCooldown(Turns - 1);
        }

        public CardCooldown ClampTo(CardCooldown maxCooldown)
        {
            return new CardCooldown(Math.Min(Turns, maxCooldown?.Turns ?? 0));
        }

        public CardCooldown IncreaseBy(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "増加量は0以上でなければなりません。");
            }

            return new CardCooldown(Turns + value);
        }

        public CardCooldown DecreaseBy(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "減少量は0以上でなければなりません。");
            }

            return new CardCooldown(Math.Max(0, Turns - value));
        }

        public static CardCooldown None => new CardCooldown(0);

        public static implicit operator int(CardCooldown cooldown)
        {
            return cooldown?.Turns ?? 0;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Turns;
        }
    }
}
