using System.Collections.Generic;
using TestCardGame.Cards.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.設計試案用
{
    /// <summary>
    /// ゲーム内のカードの基本クラス
    /// </summary>
    public abstract class CardBase
    {
        public CardAssetBase asset;
        public int Id => asset.Id;
        public string CardName => asset.CardName;
        public string Description => asset.Description;
        public int Cost => asset.Cost;
        public CardLevel Level { get; }
        public List<CardEffect> Effects { get; private set;}
        public List<CardModifier> Enchants { get; private set;}



        /// <summary>
        /// アセットからカードを生成するコンストラクタ。
        /// </summary>
        /// <param name="data"></param>
        protected CardBase(CardAssetBase data)
        {
            asset = data;
            Level = new CardLevel(data.InitialLevel);
            Enchants = new List<CardModifier>();
            Effects = new List<CardEffect>(); 
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