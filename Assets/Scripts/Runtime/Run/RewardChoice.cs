using TestCardGame.Cards.Core;
using TestCardGame.Cards.Core.Modifiers;

namespace TestCardGame.Rewards
{
    public enum RewardType
    {
        Mod,
        Heal,
        LevelUp
    }

    public class RewardChoice
    {
        public RewardType Type { get; private set; }
        public CardModifierSO Modifier { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public RewardChoice(RewardType type, CardModifierSO modifier = null)
        {
            Type = type;
            Modifier = modifier;

            if (type == RewardType.Mod)
            {
                Title = modifier != null ? modifier.DisplayName : "MOD";
                Description = modifier != null ? modifier.Description : "カードにMODを付与します。";
                if (string.IsNullOrEmpty(Title) && modifier != null)
                {
                    Title = modifier.name;
                }
            }
            else if (type == RewardType.Heal)
            {
                Title = "HP回復";
                Description = "最大HPの25%分、プレイヤーのHPを回復します。";
            }
            else if (type == RewardType.LevelUp)
            {
                Title = "レベルアップ";
                Description = "選択したカードのレベルを1上げます。";
            }
        }

        public bool CanApplyTo(CardBase card)
        {
            if (card == null)
            {
                return false;
            }

            return Type switch
            {
                RewardType.Mod => Modifier != null,
                RewardType.LevelUp => card.Level.CanUpgrade,
                _ => false
            };
        }

        public CardBase CreatePreview(CardBase card)
        {
            if (card == null)
            {
                return null;
            }

            var preview = card.Clone();
            ApplyTo(preview);
            return preview;
        }

        public void ApplyTo(CardBase card)
        {
            if (card == null)
            {
                return;
            }

            if (Type == RewardType.Mod && Modifier != null)
            {
                card.AddEnchant(Modifier);
            }
            else if (Type == RewardType.LevelUp && card.Level.CanUpgrade)
            {
                card.ChangeLevel(card.Level.Upgrade());
            }
        }
    }
}
