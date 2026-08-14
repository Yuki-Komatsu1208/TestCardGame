using TestCardGame.Actions.Core;
using TestCardGame.Cards.Core;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// Modifier発動時に参照するカード、使用者、対象情報。
    /// </summary>
    public sealed class CardModifierContext
    {
        public CardBase Card { get; }
        public PlayerUnit Player { get; }
        public ActionContext ActionContext { get; }

        public CardModifierContext(CardBase card, PlayerUnit player, ActionContext actionContext = null)
        {
            Card = card;
            Player = player;
            ActionContext = actionContext;
        }

        public Vector2Int TargetPosition => ActionContext?.TargetPosition ?? default;

        public IUnit TargetUnit
        {
            get
            {
                if (ActionContext?.MoveService == null)
                {
                    return null;
                }

                return ActionContext.MoveService.GetUnitAt(ActionContext.TargetPosition);
            }
        }
    }
}
