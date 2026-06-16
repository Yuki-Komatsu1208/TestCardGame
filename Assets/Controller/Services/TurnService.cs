using TestCardGame.BoardManage;
using TestCardGame.Cards.Core;
using TestCardGame.Charactor.Enemies;
using TestCardGame.Charactor.Player;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// プレイヤーターンと敵ターンの進行を管理するサービス。
    /// </summary>
    public class TurnService
    {
        private readonly Board board;
        private readonly UnitMoveService moveService;
        private readonly CellEffectService cellEffectService;

        public bool IsPlayerTurn { get; private set; } = true;
        public bool HasPlayedCardThisTurn { get; private set; }

        public TurnService(Board board, UnitMoveService moveService, CellEffectService cellEffectService)
        {
            this.board = board;
            this.moveService = moveService;
            this.cellEffectService = cellEffectService;
        }

        /// <summary>
        /// 現在のターン状態とプレイヤーのマナから、カードを使用できるか判定する。
        /// </summary>
        public bool CanPlayCard(CardBase card, PlayerUnit player)
        {
            if (!IsPlayerTurn)
            {
                Debug.LogWarning("It is not the player's turn.");
                return false;
            }

            if (HasPlayedCardThisTurn)
            {
                Debug.LogWarning("You can only play one card per turn.");
                return false;
            }

            if (card == null || player == null)
            {
                return false;
            }

            if (player.Mana < card.Cost)
            {
                Debug.LogWarning("Not enough Mana!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// カード使用後のターン内状態とマナ消費を反映する。
        /// </summary>
        public void MarkCardPlayed(CardBase card, PlayerUnit player)
        {
            if (card == null || player == null)
            {
                return;
            }

            player.Mana -= card.Cost;
            HasPlayedCardThisTurn = true;
        }

        /// <summary>
        /// プレイヤーターンを終了し、敵行動とセル効果を処理して次のプレイヤーターンへ進める。
        /// </summary>
        public bool EndPlayerTurn(DefaultEnemy enemy, PlayerUnit player)
        {
            if (!IsPlayerTurn)
            {
                return false;
            }

            IsPlayerTurn = false;
            ExecuteEnemyTurn(enemy, player);
            cellEffectService?.TickCellEffects(board);
            StartPlayerTurn(player);
            return true;
        }

        /// <summary>
        /// 敵のターン行動を実行する。
        /// </summary>
        private void ExecuteEnemyTurn(DefaultEnemy enemy, PlayerUnit player)
        {
            if (enemy == null || player == null || moveService == null)
            {
                return;
            }

            enemy.ExecuteTurn(new EnemyTurnContext(moveService, enemy, player));
        }

        /// <summary>
        /// プレイヤーターン開始時の状態更新を行う。
        /// </summary>
        private void StartPlayerTurn(PlayerUnit player)
        {
            IsPlayerTurn = true;
            HasPlayedCardThisTurn = false;

            if (player != null)
            {
                player.Mana = Mathf.Min(player.Mana + 1, player.MaxMana);
            }
        }
    }
}
