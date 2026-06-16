using TestCardGame.Charactor;
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

        public ActionContext(UnitMoveService moveService, IUnit user, Vector2Int targetPosition)
        {
            MoveService = moveService;
            User = user;
            TargetPosition = targetPosition;
        }
    }
}
