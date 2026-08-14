namespace TestCardGame.Run
{
    public sealed class RunEconomyConfig
    {
        public int NormalBattleGoldReward { get; }
        public int BossBattleGoldReward { get; }
        public int OverhuntBattleGoldReward { get; }
        public int TownHealCost { get; }
        public int TownLevelUpCost { get; }
        public int TownModCost { get; }
        public int TownNewCardCost { get; }

        public RunEconomyConfig(
            int normalBattleGoldReward,
            int bossBattleGoldReward,
            int overhuntBattleGoldReward,
            int townHealCost,
            int townLevelUpCost,
            int townModCost,
            int townNewCardCost)
        {
            NormalBattleGoldReward = ClampNonNegative(normalBattleGoldReward);
            BossBattleGoldReward = ClampNonNegative(bossBattleGoldReward);
            OverhuntBattleGoldReward = ClampNonNegative(overhuntBattleGoldReward);
            TownHealCost = ClampNonNegative(townHealCost);
            TownLevelUpCost = ClampNonNegative(townLevelUpCost);
            TownModCost = ClampNonNegative(townModCost);
            TownNewCardCost = ClampNonNegative(townNewCardCost);
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
