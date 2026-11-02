using System.Collections.Generic;
using TestCardGame.Rewards;
using TestCardGame.Run;
using TestCardGame.Stage;
using TestCardGame.UIs.Battle;
using TestCardGame.UIs.PostBattle;
using UnityEngine;

namespace TestCardGame.Controller
{
    /// <summary>事前配置済みBattle UIとゲーム進行イベントを仲介する。</summary>
    public sealed class BattleUI : MonoBehaviour
    {
        [Header("Pre-configured UI References")]
        [SerializeField] private BattleHudView battleHudView;
        [SerializeField] private BattleUnitHudPresenter unitHudPresenter;

        /// <summary>個体説明を閉じる必要がある戦闘状態とデッキ表示を返す。</summary>
        public bool IsUnitHoverBlocked => gameController == null || gameController.CurrentBattleResult != BattleResult.None
            || (cardListOverlay != null && cardListOverlay.IsOpen)
            || (runController != null && (runController.IsTownOpen || runController.IsAwaitingRouteChoice || runController.IsAwaitingOverhuntChoice));
        [SerializeField] private PostBattleRootView postBattleView;

        [Header("Deck overlay")]
        [SerializeField] private UIs.CardListSelectionOverlay cardListOverlay;

        private GameController gameController;
        private RunController runController;
        private RewardController rewardController;
        private BattleHudPresenter battleHudPresenter;
        private RunEndMessageService runEndMessageService;

        /// <summary>事前配置済みViewを検証し、Runと報酬のイベント購読を開始する。</summary>
        public void Initialize(GameController controller)
        {
            gameController = controller;
            runController = GetComponent<RunController>();
            rewardController = GetComponent<RewardController>();
            battleHudPresenter = new BattleHudPresenter(gameController, runController);
            runEndMessageService = new RunEndMessageService(new SystemRandomService());

            if (!ValidateReferences())
            {
                FatalRunRecoveryService.TryRecover(runController?.RunDefinition,
                    "BattleUIの必須Inspector参照が不足しています。");
                return;
            }

            BindViewEvents();
            BindGameEvents();
            postBattleView.HideAll();
            Refresh();
        }

        /// <summary>現在の戦闘状態を常設HUDへ反映する。</summary>
        public void Refresh()
        {
            if (gameController == null || battleHudView == null || battleHudPresenter == null) return;
            bool interactionEnabled = gameController.CurrentBattleResult == BattleResult.None
                && (runController == null || (!runController.IsTownOpen && !runController.IsAwaitingRouteChoice
                    && !runController.IsAwaitingOverhuntChoice));
            BattleHudViewModel model = battleHudPresenter.CreateViewModel(interactionEnabled);
            battleHudView.Render(model);
            unitHudPresenter?.Render(model);
        }

        private void BindViewEvents()
        {
            battleHudView.EndTurnRequested -= OnEndTurnClicked;
            battleHudView.EndTurnRequested += OnEndTurnClicked;
            battleHudView.DeckRequested -= OnDeckRequested;
            battleHudView.DeckRequested += OnDeckRequested;
            postBattleView.RewardPanel.RewardSelected -= OnRewardSelected;
            postBattleView.RewardPanel.RewardSelected += OnRewardSelected;
            postBattleView.RoutePanel.ActionSelected -= OnRouteActionSelected;
            postBattleView.RoutePanel.ActionSelected += OnRouteActionSelected;
            postBattleView.RunEndPanel.RestartRequested -= OnRestartRequested;
            postBattleView.RunEndPanel.RestartRequested += OnRestartRequested;
        }

        private void BindGameEvents()
        {
            if (rewardController != null)
            {
                rewardController.RewardScreenOpened -= OnRewardScreenOpened;
                rewardController.RewardScreenOpened += OnRewardScreenOpened;
            }
            if (runController == null) return;
            runController.StageStarted -= OnStageStarted;
            runController.StageStarted += OnStageStarted;
            runController.RouteChoiceRequested -= OnRouteChoiceRequested;
            runController.RouteChoiceRequested += OnRouteChoiceRequested;
            runController.OverhuntChoiceRequested -= OnOverhuntChoiceRequested;
            runController.OverhuntChoiceRequested += OnOverhuntChoiceRequested;
            runController.RunStateChanged -= OnRunStateChanged;
            runController.RunStateChanged += OnRunStateChanged;
            runController.RunWon -= OnRunWon;
            runController.RunWon += OnRunWon;
            runController.RunLost -= OnRunLost;
            runController.RunLost += OnRunLost;
        }

        private void OnEndTurnClicked()
        {
            if (gameController != null && gameController.IsPlayerTurn)
                gameController.EndPlayerTurn();
        }

        private void OnDeckRequested()
        {
            if (runController?.RunState != null && cardListOverlay != null)
                cardListOverlay.ShowBrowse("現在のデッキ一覧", runController.RunState.playerDeck);
        }

        private void OnStageStarted(StageDefinitionSO stageDefinition, RunState state)
        {
            postBattleView?.HideAll();
            Refresh();
        }

        private void OnRunStateChanged(RunState state) => Refresh();

        /// <summary>バックエンド報酬を表示専用モデルへ変換し、固定3枠へ渡す。</summary>
        private void OnRewardScreenOpened(List<RewardChoice> choices)
        {
            RewardChoiceViewModel[] models = BuildRewardModels(choices);
            if (models == null)
            {
                return;
            }
            postBattleView.HideAll();
            postBattleView.RewardPanel.Show(models);
        }

        private void OnRewardSelected(int choiceId)
        {
            IReadOnlyList<RewardChoice> choices = runController?.RewardSelectionChoices;
            if (choices == null || choiceId < 0 || choiceId >= choices.Count || choices[choiceId] == null)
            {
                FatalRunRecoveryService.TryRecover(runController?.RunDefinition,
                    $"報酬選択IDが不正です: {choiceId}");
                return;
            }

            RewardChoice choice = choices[choiceId];
            if (choice.Type == RewardType.Heal)
            {
                postBattleView.RewardPanel.Hide();
                rewardController.ChooseHealReward();
                return;
            }
            OpenDeckCardSelection(choice);
        }

        /// <summary>PLAN03の共通カード一覧を戦闘外強化選択として開く。</summary>
        private void OpenDeckCardSelection(RewardChoice choice)
        {
            if (runController?.RunState?.playerDeck == null)
            {
                FatalRunRecoveryService.TryRecover(runController?.RunDefinition, "強化対象のデッキがありません。");
                return;
            }

            postBattleView.RewardPanel.Hide();
            string title = choice.Type == RewardType.Mod
                ? $"MOD付与: {choice.Title}"
                : "レベルアップ: カードレベル +1";
            if (cardListOverlay == null)
            {
                FatalRunRecoveryService.TryRecover(runController.RunDefinition,
                    "強化に必要なカード一覧UIがありません。");
                return;
            }

            cardListOverlay.Show(title, runController.RunState.playerDeck, choice,
                deckIndex => rewardController.ChooseDeckCardForReward(deckIndex, choice),
                RestoreRewardPanel);
        }

        /// <summary>強化選択をキャンセルした場合に、検証済みの固定3択へ戻す。</summary>
        private void RestoreRewardPanel()
        {
            RewardChoiceViewModel[] models = BuildRewardModels(runController?.RewardSelectionChoices);
            if (models != null)
                postBattleView.RewardPanel.Show(models);
        }

        private void OnRouteChoiceRequested(RunState state)
        {
            ShowRouteChoice(state, "遠征ボスを撃破した", runController != null && runController.CanGoToOverhunt,
                "OverHuntへ進む");
        }

        private void OnOverhuntChoiceRequested(RunState state)
        {
            ShowRouteChoice(state, "OverHunt戦を突破した", true, "さらに続行する");
        }

        private void ShowRouteChoice(RunState state, string title, bool canContinue, string continueLabel)
        {
            if (state == null)
            {
                FatalRunRecoveryService.TryRecover(runController?.RunDefinition, "経路選択に必要なRUN状態がありません。");
                return;
            }
            postBattleView.HideAll();
            postBattleView.RoutePanel.Show(new RouteChoiceViewModel(title, state.currentHp,
                state.overhuntDepth, state.pendingGold, canContinue, continueLabel));
        }

        private void OnRouteActionSelected(RouteChoiceAction action)
        {
            postBattleView.RoutePanel.Hide();
            if (action == RouteChoiceAction.ReturnToOutpost)
                runController?.ChooseReturnToTown();
            else
                runController?.ChooseOverhunt();
        }

        private void OnRunWon()
        {
            postBattleView.HideAll();
            postBattleView.RunEndPanel.Show(new RunEndViewModel(RunEndKind.Victory,
                "RUNを完遂した", runEndMessageService.ChooseVictoryButtonLabel()));
        }

        private void OnRunLost()
        {
            postBattleView.HideAll();
            postBattleView.RunEndPanel.Show(new RunEndViewModel(RunEndKind.Death,
                "致命的な傷を負った", runEndMessageService.ChooseDeathButtonLabel()));
        }

        private void OnRestartRequested()
        {
            postBattleView.RunEndPanel.SetInteractionEnabled(false);
            if (runController == null || !runController.RestartRunToOutpost())
            {
                postBattleView.RunEndPanel.Show(new RunEndViewModel(RunEndKind.FatalError,
                    "致命的なエラーが発生しました", "操作できません"));
                postBattleView.RunEndPanel.SetInteractionEnabled(false);
            }
        }

        private RewardChoiceViewModel[] BuildRewardModels(IReadOnlyList<RewardChoice> choices)
        {
            if (choices == null || choices.Count != RewardChoicePanelView.RequiredSlotCount)
            {
                int count = choices?.Count ?? 0;
                FatalRunRecoveryService.TryRecover(runController?.RunDefinition,
                    $"戦闘報酬は3件必要ですが、{count}件でした。");
                return null;
            }
            var models = new RewardChoiceViewModel[choices.Count];
            for (int i = 0; i < choices.Count; i++)
            {
                RewardChoice choice = choices[i];
                if (choice == null)
                {
                    FatalRunRecoveryService.TryRecover(runController?.RunDefinition,
                        $"戦闘報酬の{i + 1}枠目がnullです。");
                    return null;
                }
                models[i] = new RewardChoiceViewModel(i, choice?.Title, choice?.Description,
                    ResolveRewardKind(choice), true, string.Empty);
            }
            return models;
        }

        private static RewardDisplayKind ResolveRewardKind(RewardChoice choice)
        {
            if (choice == null) return RewardDisplayKind.Modifier;
            return choice.Type switch
            {
                RewardType.Heal => RewardDisplayKind.Heal,
                RewardType.LevelUp => RewardDisplayKind.LevelUp,
                _ => RewardDisplayKind.Modifier
            };
        }

        public bool ValidateReferences()
        {
            bool valid = GetComponent<RunController>() != null && GetComponent<RewardController>() != null
                && battleHudView != null && postBattleView != null && cardListOverlay != null
                && unitHudPresenter != null && unitHudPresenter.ValidateReferences();
            return valid && battleHudView.ValidateReferences() && postBattleView.ValidateReferences();
        }

        private void OnDestroy()
        {
            if (battleHudView != null)
            {
                battleHudView.EndTurnRequested -= OnEndTurnClicked;
                battleHudView.DeckRequested -= OnDeckRequested;
            }
            if (postBattleView != null)
            {
                postBattleView.RewardPanel.RewardSelected -= OnRewardSelected;
                postBattleView.RoutePanel.ActionSelected -= OnRouteActionSelected;
                postBattleView.RunEndPanel.RestartRequested -= OnRestartRequested;
            }
            if (rewardController != null)
                rewardController.RewardScreenOpened -= OnRewardScreenOpened;
            if (runController != null)
            {
                runController.StageStarted -= OnStageStarted;
                runController.RouteChoiceRequested -= OnRouteChoiceRequested;
                runController.OverhuntChoiceRequested -= OnOverhuntChoiceRequested;
                runController.RunStateChanged -= OnRunStateChanged;
                runController.RunWon -= OnRunWon;
                runController.RunLost -= OnRunLost;
            }
        }
    }
}
