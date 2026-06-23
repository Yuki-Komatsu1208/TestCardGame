using System;
using System.Collections.Generic;
using TestCardGame.CommonBase;
using UnityEngine;

namespace TestCardGame.Cards.VOs
{
    /// <summary>
    /// カード使用時に必要なマナコストを表すValueObject。
    /// </summary>
    [Serializable]
    public class ManaCost : VOBase
    {
        [SerializeField, Min(0)] private int amount;

        public int Amount => Math.Max(0, amount);

        public ManaCost(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "マナコストは0以上でなければなりません。");
            }

            this.amount = amount;
        }

        public bool CanPayWith(int mana)
        {
            return mana >= Amount;
        }

        public int PayFrom(int mana)
        {
            if (!CanPayWith(mana))
            {
                throw new InvalidOperationException("マナが足りません。");
            }

            return mana - Amount;
        }

        public ManaCost IncreaseBy(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "増加量は0以上でなければなりません。");
            }

            return new ManaCost(Amount + value);
        }

        public ManaCost DecreaseBy(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "減少量は0以上でなければなりません。");
            }

            return new ManaCost(Math.Max(0, Amount - value));
        }

        public static ManaCost Zero => new ManaCost(0);

        public static implicit operator int(ManaCost cost)
        {
            return cost?.Amount ?? 0;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }
    }
}
