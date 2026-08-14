using System;
using System.Collections.Generic;
using TestCardGame.CommonBase;

namespace TestCardGame.Character.StatusVO
{
    /// <summary>
    /// プレイヤーのマナ状態と、その増減ルールを管理する。
    /// </summary>
    public sealed class Mana : VOBase
    {
        public const int DefaultCurrentValue = 3;
        public const int DefaultMaxValue = 5;
        public const int DefaultRecoveryPerTurn = 3;

        public int CurrentValue { get; private set; }
        public int MaxValue { get; private set; }
        public int RecoveryPerTurn { get; private set; }

        public Mana(int currentValue, int maxValue, int recoveryPerTurn)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "最大マナは0以上でなければなりません。");
            }

            if (currentValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentValue), "現在マナは0以上でなければなりません。");
            }

            if (recoveryPerTurn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(recoveryPerTurn), "マナ回復値は0以上でなければなりません。");
            }

            MaxValue = maxValue;
            CurrentValue = Math.Min(currentValue, MaxValue);
            RecoveryPerTurn = recoveryPerTurn;
        }

        public bool CanPay(int amount)
        {
            return amount >= 0 && CurrentValue >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanPay(amount))
            {
                return false;
            }

            CurrentValue -= amount;
            return true;
        }

        public void Recover(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "マナ回復量は0以上でなければなりません。");
            }

            CurrentValue = (int)Math.Min((long)MaxValue, (long)CurrentValue + amount);
        }

        public void RecoverAtTurnStart()
        {
            Recover(RecoveryPerTurn);
        }

        public void IncreaseMaxValue(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "最大マナの増加量は0以上でなければなりません。");
            }

            MaxValue = (int)Math.Min(int.MaxValue, (long)MaxValue + amount);
        }

        public void IncreaseRecoveryPerTurn(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "マナ回復値の増加量は0以上でなければなりません。");
            }

            RecoveryPerTurn = (int)Math.Min(int.MaxValue, (long)RecoveryPerTurn + amount);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CurrentValue;
            yield return MaxValue;
            yield return RecoveryPerTurn;
        }
    }
}
