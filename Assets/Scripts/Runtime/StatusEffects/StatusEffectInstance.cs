using UnityEngine;

namespace TestCardGame.Character.StatusEffects
{
    /// <summary>
    /// バトル中にユニットへ付与されている状態異常の実体。
    /// </summary>
    public class StatusEffectInstance
    {
        public StatusEffectId Id { get; }
        public int RemainingTurns { get; set; }
        public int Value { get; set; } // 炎上ダメージなどに使う汎用の強度値。
        public int MaxValue { get; private set; }

        public bool IsExpired => RemainingTurns <= 0;
        public string DisplayName => Id.GetDisplayName();

        /// <summary>
        /// 状態異常ID、残りターン、強度値を指定して実体を作成する。
        /// </summary>
        public StatusEffectInstance(StatusEffectId id, int remainingTurns, int value = 0)
        {
            Id = id;
            RemainingTurns = remainingTurns;
            Value = value;
            MaxValue = IsPersistentShield ? value : 0;
        }

        /// <summary>
        /// 同種の状態異常をこのインスタンスへ統合する。
        /// </summary>
        public void Merge(StatusEffectInstance other)
        {
            if (other == null || other.Id != Id)
            {
                return;
            }

            if (IsPersistentShield)
            {
                ApplyPersistentShield(other.Value);
                return;
            }

            RemainingTurns = Mathf.Max(RemainingTurns, other.RemainingTurns);
            if (Id == StatusEffectId.Barrier || Id == StatusEffectId.Focus || Id == StatusEffectId.Power)
            {
                Value += other.Value;
            }
            else
            {
                Value = Mathf.Max(Value, other.Value);
            }
        }

        /// <summary>
        /// ダメージをシールド系状態異常で吸収し、残りダメージを返す。
        /// </summary>
        public int AbsorbDamage(int incomingDamage)
        {
            if (incomingDamage <= 0 || !CanAbsorbDamage())
            {
                return incomingDamage;
            }

            int absorbedAmount = Mathf.Min(Value, incomingDamage);
            Value -= absorbedAmount;

            if (Value <= 0)
            {
                Value = 0;
                RemainingTurns = 0;
            }

            return incomingDamage - absorbedAmount;
        }

        /// <summary>
        /// ターン経過で減少する状態異常なら残りターンを減らす。
        /// </summary>
        public void TickDuration()
        {
            if (!ConsumesTurns() || RemainingTurns <= 0)
            {
                return;
            }

            RemainingTurns--;
        }

        /// <summary>
        /// 永続シールドの表示用上限値を返す。
        /// </summary>
        public int GetDisplayMaxValue()
        {
            return IsPersistentShield ? MaxValue : Value;
        }

        /// <summary>
        /// この状態異常下でユニットが行動可能か判定する。
        /// </summary>
        public bool CanAct()
        {
            return Id != StatusEffectId.Sleep;
        }

        private bool IsPersistentShield => Id == StatusEffectId.Shield;

        private bool CanAbsorbDamage()
        {
            return (Id == StatusEffectId.Shield || Id == StatusEffectId.Barrier) && Value > 0 && RemainingTurns > 0;
        }

        private bool ConsumesTurns()
        {
            return Id != StatusEffectId.Shield && Id != StatusEffectId.Focus;
        }

        private void ApplyPersistentShield(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (amount > MaxValue)
            {
                MaxValue = amount;
                Value = amount;
            }
            else
            {
                Value = Mathf.Min(MaxValue, Value + amount);
            }

            RemainingTurns = Mathf.Max(1, RemainingTurns);
        }
    }
}
