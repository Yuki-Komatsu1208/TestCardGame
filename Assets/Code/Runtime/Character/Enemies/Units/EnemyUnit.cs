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
        public STUNResist StunResist { get; }
        public Vector2Int Position { get; set; }
        public IReadOnlyList<EnemyActionPlan> ActionPlans { get; }
        
        private readonly StatusEffectCollection statusEffects = new();
        public IReadOnlyList<StatusEffectInstance> StatusEffects => statusEffects.Effects;

        /// <summary>
        /// 状態異常を付与する。同種の状態異常がある場合は既存効果へマージする。
        /// </summary>
        public void ApplyStatusEffect(StatusEffectInstance effect)
        {
            statusEffects.Apply(this, effect);
        }

        /// <summary>
        /// 持続ターンが切れた状態異常を削除する。
        /// </summary>
        public void CleanExpiredStatusEffects()
        {
            statusEffects.CleanExpired(this);
        }

        /// <summary>
        /// 付与中の状態異常をもとに、現在行動できるかを返す。
        /// </summary>
        public bool CanAct
        {
            get { return statusEffects.CanAct(this); }
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
            int maxStunResist = definition.maxStunResist > 0 ? definition.maxStunResist : 100;
            StunResist = new STUNResist(maxStunResist, maxStunResist);
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

                    var effect = config.useCustomParameters
                        ? config.effect.CreateRuntimeEffect(config.parameters)
                        : config.effect.CreateRuntimeEffect();

                    if (config.targetKind == EnemyTargetKind.Self)
                    {
                        plans.Add(EnemyActionPlan.Self(effect, config.intentKind));
                    }
                    else
                    {
                        plans.Add(EnemyActionPlan.Target(effect, config.intentKind));
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

            if (!TrySelectExecutablePlan(context, out int selectedIndex, out int nextIndex)) return;

            if (ActionPlans[selectedIndex].TryExecute(context) && behaviorPattern == BehaviorPattern.SequentialLoop)
            {
                nextActionIndex = nextIndex;
            }
        }

        /// <summary>
        /// 次の実行候補と予告範囲を返す。行動順インデックスを含む状態は変更しない。
        /// </summary>
        public bool TryGetNextIntent(EnemyTurnContext context, out EnemyIntentPreview preview)
        {
            if (!CanAct)
            {
                preview = EnemyIntentPreview.Unavailable(ID, "行動不能です。");
                return true;
            }

            if (ActionPlans == null || ActionPlans.Count == 0)
            {
                preview = EnemyIntentPreview.Unavailable(ID, "行動計画がありません。");
                return true;
            }

            if (TrySelectExecutablePlan(context, out int selectedIndex, out _)
                && ActionPlans[selectedIndex].TryPreview(context, out preview)) return true;

            preview = EnemyIntentPreview.Unavailable(ID, "実行可能な行動計画がありません。");
            return true;
        }

        /// <summary>
        /// Previewと実行が同じ候補を選ぶよう、現在状態から候補インデックスを純粋に計算する。
        /// </summary>
        private bool TrySelectExecutablePlan(EnemyTurnContext context, out int selectedIndex, out int indexAfterSuccess)
        {
            selectedIndex = -1;
            indexAfterSuccess = nextActionIndex;
            if (ActionPlans == null || ActionPlans.Count == 0) return false;

            int start = behaviorPattern == BehaviorPattern.SequentialLoop ? nextActionIndex : 0;
            for (int attempts = 0; attempts < ActionPlans.Count; attempts++)
            {
                int candidate = behaviorPattern == BehaviorPattern.SequentialLoop
                    ? (start + attempts) % ActionPlans.Count
                    : attempts;
                if (!ActionPlans[candidate].TryPreview(context, out _)) continue;
                selectedIndex = candidate;
                indexAfterSuccess = (candidate + 1) % ActionPlans.Count;
                return true;
            }
            return false;
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
