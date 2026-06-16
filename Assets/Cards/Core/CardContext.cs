using TestCardGame.Actions.Core;
using TestCardGame.Charactor;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Cards.Core
{
    /// <summary>
    /// カード効果の実行に必要な情報をまとめたクラス。
    /// </summary>
    public class CardContext : ActionContext
    {
        public CardContext(UnitMoveService service, IUnit user, Vector2Int targetPosition)
            : base(service, user, targetPosition)
        {
        }
    }
}
