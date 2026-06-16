using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Core
{
    /// <summary>
    /// ゲーム内のカードの基本クラス
    /// </summary>
    public abstract class CardBase
    {
        public string CardName { get; protected set; }
        public string Description { get; protected set; }
        public int Cost { get; protected set; }
        public CardLevel Level { get; protected set; }
        public List<ActionEffect> Effects { get; private set;}
        public List<CardModifier> Enchants { get; private set;}

        protected CardBase(
            string cardName,
            string description,
            int cost,
            CardLevel level,
            List<ActionEffect> effects,
            List<CardModifier> enchants = null)
        {
            if (string.IsNullOrEmpty(cardName))
            {
                throw new ArgumentNullException(nameof(cardName));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            if (effects.Count == 0)
            {
                throw new ArgumentException("カードには1つ以上の効果が必要です。", nameof(effects));
            }

            if (effects.Exists(effect => effect == null))
            {
                throw new ArgumentNullException(nameof(effects), "カード効果にnullを指定することはできません。");
            }

            CardName = cardName;
            Description = description;
            Cost = cost;
            Level = level;
            Effects = effects;
            Enchants = enchants ?? new List<CardModifier>();
        }

        /// <summary>
        /// カードのレベルを上げる。レベルアップに伴う効果の変更はこのメソッドをオーバーライドして実装する。
        /// </summary>
        public virtual void LevelUp()
        {
            if(Level.CanUpgrade)
            {
                Level = Level.Upgrade();
                // レベルアップ時の効果をここに実装（必要に応じてオーバーライド）
            }
        }

        public virtual void LevelDown()
        {
            if(Level.CanDowngrade)
            {
                Level = Level.Downgrade();
                // レベルダウン時の効果をここに実装（必要に応じてオーバーライド）
            }
        }   

        /// <summary>
        /// カードにエンチャントを追加する
        /// </summary>
        /// <param name="modifier"></param>
        public void AddEnchant(CardModifier modifier)
        {
            Enchants.Add(modifier);
        }
        
        /// <summary>
        /// カードからエンチャントを削除する
        /// </summary>
        /// <param name="modifier"></param>
        public void RemoveEnchant(CardModifier modifier)
        {
            Enchants.Remove(modifier);
        }
    }
}
