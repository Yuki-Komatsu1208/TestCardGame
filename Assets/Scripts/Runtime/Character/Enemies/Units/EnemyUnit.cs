using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Core;
using TestCardGame.Character;
using TestCardGame.Character.StatusVO;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    /// <summary>
    /// バトル中に存在する敵ユニットの実体。
    /// </summary>
    public class EnemyUnit : IEnemy
    {
        public UnitID ID { get; }
        public string Name { get; }
        public HP Hp { get; }
        public Vector2Int Position { get; set; }
        public IReadOnlyList<EnemyActionPlan> ActionPlans { get; }
        
        private readonly List<StatusEffectInstance> statusEffects = new();
        public IReadOnlyList<StatusEffectInstance> StatusEffects => statusEffects;

        /// <summary>
        /// 状態異常を付与する。同種の状態異常がある場合は既存効果へマージする。
        /// </summary>
        public void ApplyStatusEffect(StatusEffectInstance effect)
        {
            if (effect == null) return;

            var existing = statusEffects.Find(e => e.Definition.EffectId == effect.Definition.EffectId);
            if (existing != null)
            {
                existing.Merge(effect);
                Debug.Log($"{Name}の既存の状態異常 {effect.Definition.DisplayName} が更新されました（残り持続: {existing.RemainingTurns}ターン）。");
            }
            else
            {
                statusEffects.Add(effect);
                Debug.Log($"{Name}に状態異常 {effect.Definition.DisplayName} が付与されました（持続: {effect.RemainingTurns}ターン）。");
            }
        }

        /// <summary>
        /// 持続ターンが切れた状態異常を削除する。
        /// </summary>
        public void CleanExpiredStatusEffects()
        {
            for (int i = statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = statusEffects[i];
                if (effect.IsExpired)
                {
                    statusEffects.RemoveAt(i);
                    Debug.Log($"{Name}の状態異常 {effect.Definition.DisplayName} が終了しました。");
                }
            }
        }

        /// <summary>
        /// 付与中の状態異常をもとに、現在行動できるかを返す。
        /// </summary>
        public bool CanAct
        {
            get
            {
                foreach (var effect in statusEffects)
                {
                    if (!effect.CanAct(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        
        private readonly BehaviorPattern behaviorPattern;
        private int nextActionIndex;

        /// <summary>
        /// 敵定義からHP、行動パターン、行動計画を作成する。
        /// </summary>
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
                    if (config.effect == null)
                    {
                        continue;
                    }

                    var effect = config.effect.CreateRuntimeEffect();
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

        /// <summary>
        /// 行動パターンに従って、この敵のターン行動を実行する。
        /// </summary>
        public void ExecuteTurn(EnemyTurnContext context)
        {
            if (!CanAct)
            {
                Debug.Log($"{Name}は行動不能状態（睡眠など）のため、ターンをスキップします。");
                return;
            }

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

        /// <summary>
        /// 敵の座標を直接更新する。
        /// </summary>
        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}
