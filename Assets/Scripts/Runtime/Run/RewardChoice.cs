using System;
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
    }
}