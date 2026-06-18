using TestCardGame.Actions.Core;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Player;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// プレイヤーがカードを使用するときの検証と効果実行を担当するサービス。
    /// </summary>
    public class CardPlayService
    {
        private readonly UnitMoveService moveService;
        private readonly BoardTargetingService targetingService;
        private readonly StatusEffectService statusEffectService;

        public CardPlayService(UnitMoveService moveService, BoardTargetingService targetingService, StatusEffectService statusEffectService)
        {
            this.moveService = moveService;
            this.targetingService = targetingService;
            this.statusEffectService = statusEffectService;
        }

        /// <summary>
        /// ドロップ位置を対象セルに変換し、カード効果を順番に実行する。
        /// </summary>
        public bool TryPlayCard(CardBase card, PlayerUnit player, Vector2 screenPosition)
        {
            if (card == null || player == null || moveService == null || targetingService == null)
            {
                return false;
            }

            if (!targetingService.TryGetClosestCellPosition(screenPosition, out var targetCellPosition))
            {
                return false;
            }

            var context = new ActionContext(moveService, player, targetCellPosition, statusEffectService);
            foreach (var effect in card.Effects)
            {
                if (!effect.CanExecute(context))
                {
                    return false;
                }
            }

            foreach (var effect in card.Effects)
            {
                effect.Execute(context);
            }

            return true;
        }
    }
}
