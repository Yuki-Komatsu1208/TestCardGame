using System.Collections.Generic;
using TestCardGame.Charactor.Enemies.Actions;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// デフォルトの敵キャラクターを表すクラス。
    /// </summary>
    public class DefaultEnemy : IUnit
    {
        public static readonly DefaultEnemy defaultEnemy = 
            new DefaultEnemy(UnitID.defaultEnemyUnit, "Default Enemy", new StatusVO.HP(50), new UnityEngine.Vector2Int(0, 0));

        public UnitID ID { get; }
        public string Name { get; }
        public StatusVO.HP Hp { get; }
        public Vector2Int Position { get; set; }
        public DefaultEnemyAlgorithm Algorithm { get; }
        public IReadOnlyList<EnemyAction> Actions { get; }

        /// <summary>
        /// デフォルトの敵キャラクターのコンストラクタ。ID、名前、HPを初期化する。
        /// </summary>
        public DefaultEnemy(
            UnitID id,
            string name,
            StatusVO.HP hp,
            Vector2Int position,
            DefaultEnemyAlgorithm algorithm = null,
            IReadOnlyList<EnemyAction> actions = null)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            Algorithm = algorithm ?? new ChaseAndAttackAlgorithm();
            Actions = actions ?? new List<EnemyAction>
            {
                new AdjacentAttackEnemyAction(10),
                new MoveTowardTargetEnemyAction(1)
            };
        }

        public void ExecuteTurn(EnemyTurnContext context)
        {
            Algorithm.Execute(context);
        }

        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
