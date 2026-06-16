using System.Collections.Generic;
using TestCardGame.Charactor.Enemies.Actions;
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
        public IReadOnlyList<EnemyAction> Actions { get; }

        private int nextActionIndex;

        /// <summary>
        /// ファイアスライムのコンストラクタ。ID、名前、HP、初期位置、行動を初期化する。
        /// </summary>
        public FireSlime(
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
                new MoveTowardTargetEnemyAction(1),
                new IgniteAroundEnemyAction(2, 5),
                new SleepEnemyAction()
            };
        }

        /// <summary>
        /// 移動、周囲4マス炎上、おやすみの順番で行動する。
        /// 実行できない行動は飛ばし、次の行動を試す。
        /// </summary>
        public void ExecuteTurn(EnemyTurnContext context)
        {
            if (Actions == null || Actions.Count == 0)
            {
                return;
            }

            for (int attempts = 0; attempts < Actions.Count; attempts++)
            {
                var action = Actions[nextActionIndex];
                nextActionIndex = (nextActionIndex + 1) % Actions.Count;

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
