using System.Collections.Generic;
using TestCardGame.Charactor.Enemies.Actions;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// スライムの敵キャラクターを表すクラス。
    /// </summary>
    public class Slime : IEnemy
    {
        public static readonly Slime defaultSlime = 
            new Slime(UnitID.slimeUnit, "Slime", new StatusVO.HP(50), new UnityEngine.Vector2Int(0, 0));

        public UnitID ID { get; }
        public string Name { get; }
        public StatusVO.HP Hp { get; }
        public Vector2Int Position { get; set; }
        public IReadOnlyList<EnemyAction> Actions { get; }

        /// <summary>
        /// スライムのコンストラクタ。ID、名前、HP、初期位置、行動を初期化する。
        /// </summary>
        public Slime(
            UnitID id,
            string name,
            StatusVO.HP hp,
            Vector2Int position,
            IReadOnlyList<EnemyAction> actions = null)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            Actions = actions ?? new List<EnemyAction>
            {
                new AdjacentAttackEnemyAction(10),
                new MoveTowardTargetEnemyAction(1)
            };
        }

        public void ExecuteTurn(EnemyTurnContext context)
        {
            // プレイヤーに接近し、隣接したら攻撃する。
            foreach (var action in Actions)
            {
                if (!action.CanExecute(context))
                {
                    continue;
                }

                action.Execute(context);
                return;
            }
        }

        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
