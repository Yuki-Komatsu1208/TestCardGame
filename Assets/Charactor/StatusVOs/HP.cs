using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Charactor.StatusVO
{
    public class HP:VOBase
    {
        /// <summary>
        /// 最大HP
        /// </summary>
        public int InitialValue;
        /// <summary>
        /// 現在のHP
        /// </summary>
        public int CurrentValue { get; private set; }
        private int _minValue = 0;
        private int _maxValue = int.MaxValue;

        /// <summary>
        /// キャラクタのHPの初期設定を行う。
        /// </summary>
        /// <param name="initialValue"></param>
        public HP(int initialValue)
        {
            if (initialValue < 0)
            {
                Debug.LogWarning($"初期HPは0未満に設定できません。初期値を0に設定します。");
                initialValue = 0;
            }
            else if (initialValue > _maxValue)
            {
                Debug.LogWarning($"初期HPは{_maxValue}を超えることはできません。初期値を{_maxValue}に設定します。");
                initialValue = _maxValue;
            }
            InitialValue = initialValue;
            CurrentValue = initialValue;
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning($"ダメージは負の値に設定できません。ダメージを0に設定します。");
                damage = 0;
            }
            CurrentValue = Mathf.Max(CurrentValue - damage, _minValue);
        }
        /// <summary>
        /// HPを回復する
        /// </summary>
        /// <param name="amount"></param>
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"回復量は負の値に設定できません。回復量を0に設定します。");
                amount = 0;
            }
            CurrentValue = Mathf.Min(CurrentValue + amount, _maxValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return new object[] { InitialValue,CurrentValue};
        }
    }
}