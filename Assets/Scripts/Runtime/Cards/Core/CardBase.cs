using System;
using System.Collections.Generic;
using System.Linq;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Core.Modifiers;
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
        public CardLevel Level { get; private set; }
        public IReadOnlyList<CardModifier> Enchants => enchants;
        public IReadOnlyList<CardModifierSO> EnchantDefinitions => enchantDefinitions;
        public IReadOnlyList<CardModifierSO> IntrinsicModifierDefinitions => intrinsicModifierDefinitions;
        public IReadOnlyList<ActionEffect> Effects { get; private set; }

        private readonly List<CardModifier> enchants;
        private readonly List<CardModifierSO> intrinsicModifierDefinitions;
        private readonly List<CardModifierSO> enchantDefinitions;
        private bool skipNextTurnEndCooldownTick;

        public string CardName => Definition != null ? Definition.cardName : string.Empty;
        public string Description => Definition != null ? Definition.GetDataForLevel(Level.Level).description : string.Empty;
        public ManaCost Cost => Definition != null ? Definition.GetDataForLevel(Level.Level).Cost : ManaCost.Zero;
        public CardCooldown BaseCooldown => Definition != null ? Definition.GetDataForLevel(Level.Level).Cooldown : CardCooldown.None;
        public CardCooldown RemainingCooldown { get; private set; } = CardCooldown.None;
        public bool IsCoolingDown => RemainingCooldown.IsActive;
        public bool RemovesAfterUse => enchants.Any(enchant => enchant.RemovesCardAfterUse);

        public CardBase(
            CardDefinitionSO definition,
            CardLevel level,
            List<CardModifierSO> enchantDefinitions = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Level = level ?? throw new ArgumentNullException(nameof(level));
            intrinsicModifierDefinitions = new List<CardModifierSO>();
            if (Definition.intrinsicModifiers != null)
            {
                foreach (var modifierDefinition in Definition.intrinsicModifiers)
                {
                    if (modifierDefinition != null)
                    {
                        intrinsicModifierDefinitions.Add(modifierDefinition);
                    }
                }
            }
            this.enchantDefinitions = new List<CardModifierSO>();
            if (enchantDefinitions != null)
            {
                foreach (var enchantDefinition in enchantDefinitions)
                {
                    if (enchantDefinition != null)
                    {
                        this.enchantDefinitions.Add(enchantDefinition);
                    }
                }
            }
            enchants = new List<CardModifier>();
            Effects = BuildEffects(Definition.GetDataForLevel(Level.Level));
            RebuildEnchants();
        }

        public void ChangeLevel(CardLevel nextLevel)
        {
            Level = nextLevel ?? throw new ArgumentNullException(nameof(nextLevel));
            Effects = BuildEffects(Definition.GetDataForLevel(Level.Level));
            RemainingCooldown = RemainingCooldown.ClampTo(BaseCooldown);
            skipNextTurnEndCooldownTick = RemainingCooldown.IsActive && skipNextTurnEndCooldownTick;
        }

        public CardBase Clone()
        {
            return new CardBase(Definition, new CardLevel(Level.Level), new List<CardModifierSO>(enchantDefinitions));
        }

        private List<ActionEffect> BuildEffects(CardLevelData levelData)
        {
            var effects = new List<ActionEffect>();
            if (levelData?.effects == null)
            {
                return effects;
            }

            foreach (var effectEntry in levelData.effects)
            {
                var effect = effectEntry?.CreateRuntimeEffect(Level.Level);
                if (effect != null)
                {
                    effects.Add(effect);
                }
            }

            return effects;
        }

        public ManaCost GetCost(PlayerUnit player)
        {
            var context = new CardModifierContext(this, player);
            var cost = Cost;
            foreach (var enchant in Enchants)
            {
                cost = enchant.ModifyCost(cost, context);
            }

            return cost;
        }

        public CardCooldown GetCooldown(PlayerUnit player)
        {
            var context = new CardModifierContext(this, player);
            var cooldown = BaseCooldown;
            foreach (var enchant in Enchants)
            {
                cooldown = enchant.ModifyCooldown(cooldown, context);
            }

            return cooldown;
        }

        public bool CanBePlayedBy(PlayerUnit player)
        {
            return player != null && !IsCoolingDown && GetCost(player).CanPayWith(player.Mana);
        }

        public void StartCooldown(PlayerUnit player)
        {
            RemainingCooldown = RemainingCooldown.StartFrom(GetCooldown(player));
            skipNextTurnEndCooldownTick = RemainingCooldown.IsActive;
        }

        /// <summary>
        /// ターン進行に合わせて残りクールタイムを1減らす。
        /// </summary>
        public void TickCooldown()
        {
            RemainingCooldown = RemainingCooldown.Tick();
            if (!RemainingCooldown.IsActive)
            {
                skipNextTurnEndCooldownTick = false;
            }
        }

        /// <summary>
        /// ターン終了時のクールタイム更新を行う。使用直後のカードは既定CTを保持する。
        /// </summary>
        public void TickCooldownAtTurnEnd(PlayerUnit player)
        {
            if (!IsCoolingDown)
            {
                skipNextTurnEndCooldownTick = false;
                return;
            }

            if (skipNextTurnEndCooldownTick)
            {
                skipNextTurnEndCooldownTick = false;
                return;
            }

            var wasCoolingDown = IsCoolingDown;
            TickCooldown();
            if (wasCoolingDown && !IsCoolingDown)
            {
                OnCooldownReady(new CardModifierContext(this, player));
            }
        }

        public void OnBeforeCardUse(CardModifierContext context)
        {
            foreach (var enchant in Enchants)
            {
                enchant.OnBeforeCardUse(context);
            }
        }

        public void OnAfterCardUse(CardModifierContext context)
        {
            foreach (var enchant in Enchants)
            {
                enchant.OnAfterCardUse(context);
            }
        }

        public void OnCooldownReady(CardModifierContext context)
        {
            foreach (var enchant in Enchants)
            {
                enchant.OnCooldownReady(context);
            }
        }

        public void AddEnchant(CardModifierSO modifierDefinition)
        {
            if (modifierDefinition == null)
            {
                return;
            }

            enchantDefinitions.Add(modifierDefinition);
            var modifier = modifierDefinition.CreateRuntimeModifier();
            if (modifier != null)
            {
                enchants.Add(modifier);
            }
        }

        public void ResetBattleState()
        {
            RemainingCooldown = CardCooldown.None;
            skipNextTurnEndCooldownTick = false;
        }

        private void RebuildEnchants()
        {
            enchants.Clear();
            foreach (var modifierDefinition in intrinsicModifierDefinitions)
            {
                var modifier = modifierDefinition != null ? modifierDefinition.CreateRuntimeModifier() : null;
                if (modifier != null)
                {
                    enchants.Add(modifier);
                }
            }
            foreach (var modifierDefinition in enchantDefinitions)
            {
                var modifier = modifierDefinition != null ? modifierDefinition.CreateRuntimeModifier() : null;
                if (modifier != null)
                {
                    enchants.Add(modifier);
                }
            }
        }
    }
}
