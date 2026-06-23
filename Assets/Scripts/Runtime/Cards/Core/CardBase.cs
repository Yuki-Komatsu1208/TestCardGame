using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;
using TestCardGame.Character.Player;

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
        private List<CardModifier> _cachedDefinitionModifiers;

        public string CardName => Definition != null ? Definition.cardName : string.Empty;
        public string Description => Definition != null ? Definition.GetDataForLevel(Level.Level).description : string.Empty;
        public ManaCost Cost => Definition != null ? Definition.GetDataForLevel(Level.Level).Cost : ManaCost.Zero;
        public CardCooldown Cooldown => Definition != null ? Definition.GetDataForLevel(Level.Level).Cooldown : CardCooldown.None;
        public CardCooldown RemainingCooldown { get; private set; } = CardCooldown.None;
        public bool IsCoolingDown => RemainingCooldown.IsActive;

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

        public IReadOnlyList<CardModifier> Modifiers
        {
            get
            {
                if (_cachedDefinitionModifiers == null)
                {
                    _cachedDefinitionModifiers = new List<CardModifier>();
                    if (Definition != null)
                    {
                        var levelData = Definition.GetDataForLevel(Level.Level);
                        if (levelData?.modifiers != null)
                        {
                            foreach (var modifierDefinition in levelData.modifiers)
                            {
                                var modifier = modifierDefinition != null ? modifierDefinition.CreateRuntimeModifier() : null;
                                if (modifier != null)
                                {
                                    _cachedDefinitionModifiers.Add(modifier);
                                }
                            }
                        }
                    }
                }

                if (Enchants == null || Enchants.Count == 0)
                {
                    return _cachedDefinitionModifiers;
                }

                var merged = new List<CardModifier>(_cachedDefinitionModifiers.Count + Enchants.Count);
                merged.AddRange(_cachedDefinitionModifiers);
                merged.AddRange(Enchants);
                return merged;
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
                _cachedDefinitionModifiers = null;
                RemainingCooldown = RemainingCooldown.ClampTo(Cooldown);
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
                _cachedDefinitionModifiers = null;
                RemainingCooldown = RemainingCooldown.ClampTo(Cooldown);
            }
        }

        public ManaCost GetCost(PlayerUnit player)
        {
            var context = new CardModifierContext(this, player);
            var cost = Cost;
            foreach (var modifier in Modifiers)
            {
                cost = modifier.ModifyCost(cost, context);
            }

            return cost;
        }

        public CardCooldown GetCooldown(PlayerUnit player)
        {
            var context = new CardModifierContext(this, player);
            var cooldown = Cooldown;
            foreach (var modifier in Modifiers)
            {
                cooldown = modifier.ModifyCooldown(cooldown, context);
            }

            return cooldown;
        }

        /// <summary>
        /// カード使用後に、このカードの現在レベルに応じたクールタイムを開始する。
        /// </summary>
        public void StartCooldown()
        {
            RemainingCooldown = RemainingCooldown.StartFrom(Cooldown);
        }

        public void StartCooldown(PlayerUnit player)
        {
            RemainingCooldown = RemainingCooldown.StartFrom(GetCooldown(player));
        }

        /// <summary>
        /// ターン進行に合わせて残りクールタイムを1減らす。
        /// </summary>
        public void TickCooldown()
        {
            RemainingCooldown = RemainingCooldown.Tick();
        }

        public void OnBeforeCardUse(CardModifierContext context)
        {
            foreach (var modifier in Modifiers)
            {
                modifier.OnBeforeCardUse(context);
            }
        }

        public void OnAfterCardUse(CardModifierContext context)
        {
            foreach (var modifier in Modifiers)
            {
                modifier.OnAfterCardUse(context);
            }
        }

        public void OnCooldownReady(CardModifierContext context)
        {
            foreach (var modifier in Modifiers)
            {
                modifier.OnCooldownReady(context);
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
