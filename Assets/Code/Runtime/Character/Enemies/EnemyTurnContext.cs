using System;
using TestCardGame.Controller.Services;
using TestCardGame.Character;

namespace TestCardGame.Character.Enemies
{
    /// <summary>
    /// 敵の行動判断に必要な情報をまとめたクラス。
    /// </summary>
    public class EnemyTurnContext
    {
        public UnitMoveService MoveService { get; }
        public IEnemy Enemy { get; }
        public IUnit Target { get; }
        public StatusEffectService StatusEffectService { get; }

        public EnemyTurnContext(UnitMoveService moveService, IEnemy enemy, IUnit target, StatusEffectService statusEffectService)
        {
            MoveService = moveService ?? throw new ArgumentNullException(nameof(moveService));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            StatusEffectService = statusEffectService;
        }
    }
}
