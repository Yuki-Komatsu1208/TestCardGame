using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Controller;
using TestCardGame.Run;

namespace TestCardGame.UIs.Battle
{
    /// <summary>戦闘状態を読み取り専用のHUD表示モデルへ変換する。</summary>
    public sealed class BattleHudPresenter
    {
        private readonly GameController gameController;
        private readonly RunController runController;

        public BattleHudPresenter(GameController gameController, RunController runController)
        {
            this.gameController = gameController;
            this.runController = runController;
        }

        /// <summary>現在の盤面とRUN進行からHUD表示モデルを作成する。</summary>
        public BattleHudViewModel CreateViewModel(bool interactionEnabled)
        {
            var state = runController?.RunState;
            var enemies = new List<UnitHudViewModel>();
            if (gameController?.Enemies != null)
            {
                foreach (var enemy in gameController.Enemies)
                {
                    if (enemy != null) enemies.Add(CreateUnit(enemy));
                }
            }

            bool isOverHunt = state != null && (state.isInOverhunt || state.phase == RunProgressPhase.OverHunt
                || state.phase == RunProgressPhase.AwaitingOverhuntDecision);
            return new BattleHudViewModel
            {
                Player = CreatePlayer(gameController?.PlayerUnitInstance),
                Enemies = enemies,
                TurnLabel = gameController != null
                    ? $"TURN {gameController.CurrentTurnNumber}  {(gameController.IsPlayerTurn ? "PLAYER" : "ENEMY")}" : "TURN --",
                ExpeditionLabel = BuildExpeditionLabel(state),
                StageLabel = gameController?.CurrentStageName ?? "ステージ未設定",
                IsOverHunt = isOverHunt,
                OverHuntDepth = state?.overhuntDepth ?? 0,
                PendingGold = state?.pendingGold ?? 0,
                CanEndTurn = gameController != null && gameController.IsPlayerTurn && interactionEnabled,
                CanOpenDeck = interactionEnabled || (gameController != null && gameController.CurrentBattleResult == BattleResult.None)
            };
        }

        private static UnitHudViewModel CreatePlayer(PlayerUnit player)
        {
            if (player == null) return null;
            return new UnitHudViewModel(player.ID, player.Name, player.Hp.CurrentValue, player.Hp.InitialValue,
                player.StunResist.CurrentValue, player.StunResist.MaxValue, CreateStatuses(player),
                player.Hp.CurrentValue <= 0, player.Mana.CurrentValue, player.Mana.MaxValue);
        }

        private static UnitHudViewModel CreateUnit(IUnit unit)
        {
            return new UnitHudViewModel(unit.ID, unit.Name, unit.Hp.CurrentValue, unit.Hp.InitialValue,
                unit.StunResist.CurrentValue, unit.StunResist.MaxValue, CreateStatuses(unit), unit.Hp.CurrentValue <= 0);
        }

        private static IReadOnlyList<StatusEffectHudViewModel> CreateStatuses(IUnit unit)
        {
            var result = new List<StatusEffectHudViewModel>();
            if (unit?.StatusEffects == null) return result;
            foreach (var status in unit.StatusEffects)
            {
                string suffix = status.RemainingTurns > 0 ? $" {status.RemainingTurns}T" : string.Empty;
                if (status.Value > 0) suffix += $" / {status.Value}";
                result.Add(new StatusEffectHudViewModel(status.DisplayName + suffix));
            }
            return result;
        }

        private string BuildExpeditionLabel(RunState state)
        {
            if (state == null) return "遠征情報なし";
            string name = "遠征";
            var expeditions = runController?.RunDefinition?.expeditions;
            if (expeditions != null && state.currentExpeditionIndex >= 0 && state.currentExpeditionIndex < expeditions.Count)
            {
                name = expeditions[state.currentExpeditionIndex]?.expeditionName;
                if (string.IsNullOrWhiteSpace(name)) name = $"遠征 {state.currentExpeditionIndex + 1}";
            }
            return $"{name}  /  戦闘 {state.currentStageIndex + 1}";
        }
    }
}
