using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Charactor.Enemies.Targeting;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// 火を扱うスライムの敵キャラクター。
    /// 移動、周囲4マス炎上、おやすみを順番に繰り返す。
    /// </summary>
    public class FireSlime : IEnemy
    {
        public static readonly FireSlime defaultFireSlime =
            new FireSlime(UnitID.fireSlimeUnit, "Fire Slime", new StatusVO.HP(60), new Vector2Int(0, 0));

        public UnitID ID { get; }
        public string Name { get; }
        public StatusVO.HP Hp { get; }
        public Vector2Int Position { get; set; }
        public IReadOnlyList<EnemyActionPlan> ActionPlans { get; }

        private int nextActionIndex;

        /// <summary>
        /// ファイアスライムのコンストラクタ。ID、名前、HP、初期位置、行動を初期化する。
        /// </summary>
        public FireSlime(
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
                new EnemyActionPlan(new eMove(1), new TargetUnitPositionSelector()),
                new EnemyActionPlan(new eIgniteAround(2, 5), new SelfPositionSelector()),
                new EnemyActionPlan(new eSleep(), new SelfPositionSelector())
            };
        }

        /// <summary>
        /// 移動、周囲4マス炎上、おやすみの順番で行動する。
        /// 実行できない行動は飛ばし、次の行動を試す。
        /// </summary>
        public void ExecuteTurn(EnemyTurnContext context)
        {
            if (ActionPlans == null || ActionPlans.Count == 0)
            {
                return;
            }

            for (int attempts = 0; attempts < ActionPlans.Count; attempts++)
            {
                var plan = ActionPlans[nextActionIndex];
                nextActionIndex = (nextActionIndex + 1) % ActionPlans.Count;

                if (plan.TryExecute(context)) return;
            }
        }

        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
