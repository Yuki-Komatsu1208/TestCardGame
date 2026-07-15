namespace TestCardGame.Character.StatusEffects
{
    public enum StatusEffectId
    {
        None = 0,
        Burn = 1,
        Poison = 2,
        Shield = 3,
        Sleep = 4,
        Weak = 5,
        Focus = 6,
        Power = 7,
        Frostbite = 8,
        Barrier = 9
    }

    public static class StatusEffectIdExtensions
    {
        public static string GetDisplayName(this StatusEffectId id)
        {
            return id switch
            {
                StatusEffectId.Burn => "炎上",
                StatusEffectId.Poison => "毒",
                StatusEffectId.Shield => "シールド",
                StatusEffectId.Barrier => "バリア",
                StatusEffectId.Sleep => "睡眠",
                StatusEffectId.Weak => "弱体",
                StatusEffectId.Focus => "集中",
                StatusEffectId.Power => "力上昇",
                StatusEffectId.Frostbite => "凍傷",
                _ => "なし"
            };
        }
    }
}
