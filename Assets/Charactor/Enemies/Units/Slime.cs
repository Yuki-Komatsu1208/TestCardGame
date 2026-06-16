using System.Collections.Generic;
using TestCardGame.Actions.Effects;
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
        public IReadOnlyList<EnemyActionPlan> ActionPlans { get; }

        /// <summary>
        /// スライムのコンストラクタ。ID、名前、HP、初期位置、行動を初期化する。
        /// </summary>
        public Slime(
            UnitID id,
            string name,
            StatusVO.HP hp,
            Vector2Int position,
            IReadOnlyList<EnemyActionPlan> actionPlans = null)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            ActionPlans = actionPlans ?? new List<EnemyActionPlan>
            {
                EnemyActionPlan.Target(new ePositionAttack(10, 1)),
                EnemyActionPlan.Target(new eMove(1))
            };
        }

        public void ExecuteTurn(EnemyTurnContext context)
        {
            // プレイヤーに接近し、隣接したら攻撃する。
            foreach (var plan in ActionPlans)
            {
                if (plan.TryExecute(context)) return;
            }
        }

        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
