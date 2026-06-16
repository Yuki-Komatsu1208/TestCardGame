using System;
using TestCardGame.Controller.Services;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 敵の行動判断に必要な情報をまとめたクラス。
    /// </summary>
    public class EnemyTurnContext
    {
        public UnitMoveService MoveService { get; }
        public IEnemy Enemy { get; }
        public IUnit Target { get; }

        public EnemyTurnContext(UnitMoveService moveService, IEnemy enemy, IUnit target)
        {
            MoveService = moveService ?? throw new ArgumentNullException(nameof(moveService));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
