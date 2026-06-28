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
        }

        /// <summary>
        /// 同種の状態異常をこのインスタンスへ統合する。
        /// </summary>
        public void Merge(StatusEffectInstance other)
        {
            RemainingTurns = Mathf.Max(RemainingTurns, other.RemainingTurns);
            if (Id == StatusEffectId.Shield)
            {
                Value += other.Value;
            }
            else
            {
                Value = Mathf.Max(Value, other.Value);
            }
        }

        /// <summary>
        /// この状態異常下でユニットが行動可能か判定する。
        /// </summary>
        public bool CanAct()
        {
            return Id != StatusEffectId.Sleep;
        }
    }
}
