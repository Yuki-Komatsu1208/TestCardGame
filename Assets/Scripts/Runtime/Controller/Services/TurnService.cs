using TestCardGame.BoardManage;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Enemies;
using TestCardGame.Character.Player;
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
        private readonly StatusEffectService statusEffectService;

        public bool IsPlayerTurn { get; private set; } = true;

        public TurnService(Board board, UnitMoveService moveService, CellEffectService cellEffectService, StatusEffectService statusEffectService)
        {
            this.board = board;
            this.moveService = moveService;
            this.cellEffectService = cellEffectService;
            this.statusEffectService = statusEffectService;
        }

        /// <summary>
        /// 現在のターン状態とプレイヤーのマナから、カードを使用できるか判定する。
        /// </summary>
        public bool CanPlayCard(CardBase card, PlayerUnit player)
        {
            if (!IsPlayerTurn)
            {
                Debug.LogWarning("現在はプレイヤーのターンではありません。");
                return false;
            }

            if (card == null || player == null)
            {
                return false;
            }

            if (!card.CanBePlayedBy(player))
            {
                if (card.IsCoolingDown)
                {
                    Debug.LogWarning($"カード「{card.CardName}」はクールタイム中です。残り{card.RemainingCooldown.Turns}ターン。");
                }
                else
                {
                    Debug.LogWarning("マナが足りません。");
                }
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

            player.Mana = card.GetCost(player).PayFrom(player.Mana);
            card.StartCooldown(player);
        }

        /// <summary>
        /// プレイヤーターンを終了し、複数の敵行動とセル効果を処理して次のプレイヤーターンへ進める。
        /// </summary>
        public bool EndPlayerTurn(System.Collections.Generic.IReadOnlyList<IEnemy> enemies, PlayerUnit player)
        {
            if (!IsPlayerTurn)
            {
                return false;
            }

            IsPlayerTurn = false;

            // プレイヤーターンの終了時にプレイヤーの状態異常を処理する
            statusEffectService?.TickTurnEnd(player);
            TickPlayerCardCooldowns(player);

            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy == null || enemy.Hp.CurrentValue <= 0)
                    {
                        continue;
                    }

                    ExecuteEnemyTurn(enemy, player);

                    // 敵ターンの終了時に敵の状態異常を処理する
                    statusEffectService?.TickTurnEnd(enemy);
                }
            }

            cellEffectService?.TickCellEffects(board, statusEffectService);
            StartPlayerTurn(player);
            return true;
        }

        /// <summary>
        /// プレイヤーターンを終了し、敵行動とセル効果を処理して次のプレイヤーターンへ進める（単体互換用）。
        /// </summary>
        public bool EndPlayerTurn(IEnemy enemy, PlayerUnit player)
        {
            var list = new System.Collections.Generic.List<IEnemy>();
            if (enemy != null) list.Add(enemy);
            return EndPlayerTurn(list, player);
        }

        /// <summary>
        /// 敵のターン行動を実行する。
        /// </summary>
        private void ExecuteEnemyTurn(IEnemy enemy, PlayerUnit player)
        {
            if (enemy == null || player == null || moveService == null)
            {
                return;
            }

            // 敵ターン開始時の状態異常を処理する
            statusEffectService?.TickTurnStart(enemy);

            enemy.ExecuteTurn(new EnemyTurnContext(moveService, enemy, player, statusEffectService));
        }

        /// <summary>
        /// プレイヤーターン開始時の状態更新を行う。
        /// </summary>
        private void StartPlayerTurn(PlayerUnit player)
        {
            IsPlayerTurn = true;

            if (player != null)
            {
                player.Mana = Mathf.Min(player.Mana + 1, player.MaxMana);
                // プレイヤーターン開始時の状態異常を処理する
                statusEffectService?.TickTurnStart(player);
            }
        }

        /// <summary>
        /// プレイヤーの所持カードの残りクールタイムを進める。
        /// </summary>
        private static void TickPlayerCardCooldowns(PlayerUnit player)
        {
            if (player?.Cards == null)
            {
                return;
            }

            foreach (var card in player.Cards)
            {
                card?.TickCooldownAtTurnEnd(player);
            }
        }
    }
}
