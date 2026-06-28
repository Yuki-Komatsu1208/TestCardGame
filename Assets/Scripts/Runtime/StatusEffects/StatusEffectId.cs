namespace TestCardGame.Character.StatusEffects
{
    public enum StatusEffectId
    {
        None = 0,
        Burn = 1,
        Poison = 2,
        Shield = 3,
        Sleep = 4,
        Weak = 5
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
                StatusEffectId.Sleep => "睡眠",
                StatusEffectId.Weak => "弱体",
                _ => "なし"
            };
        }
    }
}
