using TestCardGame.CommonBase;
using UnityEngine;

namespace TestCardGame.Character.StatusVO
{
    /// <summary>
    /// スタン耐性の現在値と最大値を管理する値オブジェクト。
    /// </summary>
    public sealed class STUNResist : VOBase
    {
        public int CurrentValue { get; private set; }
        public int MaxValue { get; }
        public bool IsDepleted => CurrentValue <= 0;

        public STUNResist(int currentValue, int maxValue)
        {
            MaxValue = Mathf.Max(1, maxValue);
            CurrentValue = Mathf.Clamp(currentValue, 0, MaxValue);
        }

        public void TakeDamage(int amount)
        {
            CurrentValue = Mathf.Max(0, CurrentValue - Mathf.Max(0, amount));
        }

        public void RestoreToRatio(float ratio)
        {
            CurrentValue = Mathf.Clamp(Mathf.RoundToInt(MaxValue * Mathf.Clamp01(ratio)), 0, MaxValue);
        }

        protected override System.Collections.Generic.IEnumerable<object> GetEqualityComponents()
        {
            yield return CurrentValue;
            yield return MaxValue;
        }
    }
}
