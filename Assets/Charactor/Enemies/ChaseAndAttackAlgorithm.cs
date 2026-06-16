namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 実行可能な敵行動を優先順に選ぶアルゴリズム。
    /// </summary>
    public sealed class ChaseAndAttackAlgorithm : DefaultEnemyAlgorithm
    {
        public override void Execute(EnemyTurnContext context)
        {
            foreach (var action in context.Enemy.Actions)
            {
                if (!action.CanExecute(context))
                {
                    continue;
                }

                action.Execute(context);
                return;
            }
        }
    }
}
