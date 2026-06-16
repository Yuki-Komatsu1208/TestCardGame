namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// DefaultEnemyの行動アルゴリズムの基底クラス。
    /// </summary>
    public abstract class DefaultEnemyAlgorithm
    {
        public abstract void Execute(EnemyTurnContext context);
    }
}
