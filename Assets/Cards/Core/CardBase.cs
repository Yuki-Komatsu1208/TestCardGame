using System.Collections.Generic;
using TestCardGame.Cards.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Core
{
    /// <summary>
    /// ゲーム内のカードの基本クラス
    /// </summary>
    public abstract class CardBase
    {
        public string CardName { get; }
        public string Description { get; }
        public int Cost { get; }
        public CardLevel Level { get; }
        public List<CardEffect> Effects { get; private set;}
        public List<CardModifier> Enchants { get; private set;}

        protected CardBase(
            string cardName,
            string description,
            int cost,
            CardLevel level,
            List<CardEffect> effects,
            List<CardModifier> enchants = null)
        {
            CardName = cardName;
            Description = description;
            Cost = cost;
            Level = level;
            Effects = effects ?? new List<CardEffect>();
            Enchants = enchants ?? new List<CardModifier>();
        }

        /// <summary>
        /// カードのレベルを上げる。レベルアップに伴う効果の変更はこのメソッドをオーバーライドして実装する。
        /// </summary>
        public virtual void LevelUp()
        {
            if(Level.CanUpgrade)
            {
                Level.Upgrade();
                // レベルアップ時の効果をここに実装（必要に応じてオーバーライド）
            }
        }

        public virtual void LevelDown()
        {
            if(Level.CanDowngrade)
            {
                Level.Downgrade();
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