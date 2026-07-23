namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// 使用後、または戦闘終了時にデッキから取り除かれる一時カード用Modifier。
    /// 戦闘終了時の削除はRunProgressServiceが行う。
    /// </summary>
    public sealed class InstantModifier : CardModifier
    {
        public override bool RemovesCardAfterUse => true;
    }
}
