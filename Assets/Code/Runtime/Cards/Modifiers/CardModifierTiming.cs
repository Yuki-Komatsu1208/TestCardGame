namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カードModifierが主に反応する発動タイミング。
    /// </summary>
    public enum CardModifierTiming
    {
        BeforeCardUse,
        AfterCardUse,
        CooldownStart,
        CooldownReady
    }
}
