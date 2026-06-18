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

        public ActionContext(UnitMoveService moveService, IUnit user, Vector2Int targetPosition, StatusEffectService statusEffectService)
        {
            MoveService = moveService;
            User = user;
            TargetPosition = targetPosition;
            StatusEffectService = statusEffectService;
        }
    }
}
