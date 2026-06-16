using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Core;
using TestCardGame.Character;
using TestCardGame.Character.StatusVO;
using TestCardGame.Character.ValueObjects;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    public class EnemyUnit : IEnemy
    {
        public UnitID ID { get; }
        public string Name { get; }
        public HP Hp { get; }
        public Vector2Int Position { get; set; }
        public IReadOnlyList<EnemyActionPlan> ActionPlans { get; }
        
        private readonly BehaviorPattern behaviorPattern;
        private int nextActionIndex;

        public EnemyUnit(UnitID id, EnemyDefinitionSO definition, Vector2Int position)
        {
            ID = id;
            Name = definition.enemyName;
            Hp = new HP(definition.maxHp);
            Position = position;
            behaviorPattern = definition.behaviorPattern;
            nextActionIndex = 0;

            var plans = new List<EnemyActionPlan>();
            if (definition.actionPlans != null)
            {
                foreach (var config in definition.actionPlans)
                {
                    var effect = EffectFactory.CreateEffect(config.effect);
                    if (config.targetKind == EnemyTargetKind.Self)
                    {
                        plans.Add(EnemyActionPlan.Self(effect));
                    }
                    else
                    {
                        plans.Add(EnemyActionPlan.Target(effect));
                    }
                }
            }
            ActionPlans = plans;
        }

        public void ExecuteTurn(EnemyTurnContext context)
        {
            if (ActionPlans == null || ActionPlans.Count == 0)
            {
                return;
            }

            if (behaviorPattern == BehaviorPattern.FirstExecutable)
            {
                foreach (var plan in ActionPlans)
                {
                    if (plan.TryExecute(context)) return;
                }
            }
            else if (behaviorPattern == BehaviorPattern.SequentialLoop)
            {
                for (int attempts = 0; attempts < ActionPlans.Count; attempts++)
                {
                    var plan = ActionPlans[nextActionIndex];
                    nextActionIndex = (nextActionIndex + 1) % ActionPlans.Count;

                    if (plan.TryExecute(context)) return;
                }
            }
        }

        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
