using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Core
{
    /// <summary>
    /// ゲーム内のカードの基本クラス（データ駆動型）
    /// </summary>
    public class CardBase
    {
        public CardDefinitionSO Definition { get; private set; }
        public CardLevel Level { get; protected set; }
        public List<CardModifier> Enchants { get; private set; }

        private List<ActionEffect> _cachedEffects;

        public string CardName => Definition != null ? Definition.cardName : string.Empty;
        public string Description => Definition != null ? Definition.GetDataForLevel(Level.Level).description : string.Empty;
        public int Cost => Definition != null ? Definition.GetDataForLevel(Level.Level).cost : 0;

        public List<ActionEffect> Effects
        {
            get
            {
                if (_cachedEffects == null)
                {
                    _cachedEffects = new List<ActionEffect>();
                    if (Definition != null)
                    {
                        var levelData = Definition.GetDataForLevel(Level.Level);
                        if (levelData != null && levelData.effects != null)
                        {
                            foreach (var effectEntry in levelData.effects)
                            {
                                var effect = effectEntry?.CreateRuntimeEffect(Level.Level);
                                if (effect != null)
                                {
                                    _cachedEffects.Add(effect);
                                }
                            }
                        }
                    }
                }
                return _cachedEffects;
            }
        }

        public CardBase(
            CardDefinitionSO definition,
            CardLevel level,
            List<CardModifier> enchants = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Level = level ?? throw new ArgumentNullException(nameof(level));
            Enchants = enchants ?? new List<CardModifier>();
        }

        /// <summary>
        /// カードのレベルを上げる。
        /// </summary>
        public virtual void LevelUp()
        {
            if (Level.CanUpgrade)
            {
                Level = Level.Upgrade();
                _cachedEffects = null;
            }
        }

        /// <summary>
        /// カードのレベルを下げる。
        /// </summary>
        public virtual void LevelDown()
        {
            if (Level.CanDowngrade)
            {
                Level = Level.Downgrade();
                _cachedEffects = null;
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
