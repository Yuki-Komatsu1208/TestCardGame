using TestCardGame.Character;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Core
{
    /// <summary>
    /// 行動効果の実行に必要な情報をまとめたクラス。
    /// </summary>
    public class ActionContext
    {
        public UnitMoveService MoveService { get; }
        public IUnit User { get; }
        public Vector2Int TargetPosition { get; }
        public StatusEffectService StatusEffectService { get; }
        public CardTargetSelectionService CardTargetSelectionService { get; }
        public bool HasResolvedHitTarget { get; private set; }
        public IUnit HitTarget { get; private set; }

        /// <summary>実際の命中先をMODへ渡す。nullは対象未解決ではなく空振りを表す。</summary>
        public void SetHitTarget(IUnit target)
        {
            HitTarget = target;
            HasResolvedHitTarget = true;
        }

        public ActionContext(UnitMoveService moveService, IUnit user, Vector2Int targetPosition, StatusEffectService statusEffectService, CardTargetSelectionService cardTargetSelectionService = null)
        {
            MoveService = moveService;
            User = user;
            TargetPosition = targetPosition;
            StatusEffectService = statusEffectService;
            CardTargetSelectionService = cardTargetSelectionService;
        }
    }
}
