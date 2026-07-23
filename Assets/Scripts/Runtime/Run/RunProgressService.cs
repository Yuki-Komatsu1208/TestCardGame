using System;
using System.Collections.Generic;
using System.Linq;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using TestCardGame.Stage;

namespace TestCardGame.Run
{
    public enum BattleWinFlow
    {
        None,
        StartNextStage,
        RequestNormalReward,
        RequestRouteChoice,
        RequestOverhuntChoice
    }

    public sealed class BattleWinResult
    {
        public BattleWinFlow Flow { get; }
        public string Message { get; }

        public BattleWinResult(BattleWinFlow flow, string message)
        {
            Flow = flow;
            Message = message;
        }
    }

    public sealed class RunProgressService
    {
        public RunState StartRun(RunDefinitionSO runDefinition)
        {
            if (runDefinition == null)
            {
                throw new ArgumentNullException(nameof(runDefinition));
            }

            var runState = new RunState
            {
                currentExpeditionIndex = 0,
                currentStageIndex = 0,
                hasStartedCurrentExpedition = false,
                currentHp = runDefinition.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100,
                ownedGold = 0,
                pendingGold = 0,
                overhuntDepth = 0,
                isInOverhunt = false,
                phase = RunProgressPhase.Expedition
            };

            if (runDefinition.playerDefinition?.initialCards != null)
            {
                foreach (var entry in runDefinition.playerDefinition.initialCards)
                {
                    if (entry?.card != null)
                    {
                        runState.playerDeck.Add(new CardBase(entry.card, new CardLevel(entry.level)));
                    }
                }
            }

            return runState;
        }

        public BattleWinResult HandleBattleWin(RunState runState, RunDefinitionSO runDefinition, RunEconomyConfig economyConfig, int currentHp)
        {
            if (runState == null)
            {
                throw new ArgumentNullException(nameof(runState));
            }

            if (economyConfig == null)
            {
                throw new ArgumentNullException(nameof(economyConfig));
            }

            runState.currentHp = currentHp;
            RemoveInstantCards(runState);
            ResetPlayerDeckBattleState(runState);

            if (runState.isInOverhunt)
            {
                runState.pendingGold += economyConfig.OverhuntBattleGoldReward;
                runState.overhuntDepth++;
                runState.phase = RunProgressPhase.AwaitingOverhuntDecision;
                return new BattleWinResult(
                    BattleWinFlow.RequestOverhuntChoice,
                    $"OverHunt戦クリア: 保留ゴールド {runState.pendingGold}, 深度 {runState.overhuntDepth}");
            }

            if (IsBossStage(runDefinition, runState))
            {
                runState.ownedGold += economyConfig.BossBattleGoldReward;
                runState.phase = RunProgressPhase.AwaitingReturnOrOverhuntChoice;
                return new BattleWinResult(
                    BattleWinFlow.RequestRouteChoice,
                    $"遠征ボス撃破: ゴールド {economyConfig.BossBattleGoldReward} を獲得。帰還またはOverHuntを選択してください。");
            }

            runState.ownedGold += economyConfig.NormalBattleGoldReward;
            runState.currentStageIndex++;
            runState.phase = RunProgressPhase.AwaitingNormalRewardResolution;
            return new BattleWinResult(
                BattleWinFlow.RequestNormalReward,
                $"通常戦闘クリア: ゴールド {economyConfig.NormalBattleGoldReward} を獲得。報酬を選択してください。");
        }

        public string ChooseReturnToTown(RunState runState)
        {
            if (runState == null)
            {
                throw new ArgumentNullException(nameof(runState));
            }

            if (runState.phase != RunProgressPhase.AwaitingReturnOrOverhuntChoice
                && runState.phase != RunProgressPhase.AwaitingOverhuntDecision)
            {
                return null;
            }

            runState.isInOverhunt = false;
            runState.ownedGold += runState.pendingGold;
            runState.pendingGold = 0;
            runState.phase = RunProgressPhase.Town;

            return $"街へ帰還: OverHuntの保留ゴールドを精算しました。所持ゴールド {runState.ownedGold}";
        }

        public bool CompleteNormalBattleReward(RunState runState)
        {
            if (runState == null || runState.phase != RunProgressPhase.AwaitingNormalRewardResolution)
            {
                return false;
            }

            runState.phase = RunProgressPhase.Expedition;
            return true;
        }

        public bool ChooseOverhunt(RunState runState, RunDefinitionSO runDefinition)
        {
            if (runState == null || !CanGoToOverhunt(runDefinition, runState))
            {
                return false;
            }

            if (runState.phase != RunProgressPhase.AwaitingReturnOrOverhuntChoice
                && runState.phase != RunProgressPhase.AwaitingOverhuntDecision)
            {
                return false;
            }

            runState.isInOverhunt = true;
            runState.phase = RunProgressPhase.OverHunt;
            return true;
        }

        public void OpenTown(RunState runState)
        {
            if (runState == null)
            {
                throw new ArgumentNullException(nameof(runState));
            }

            runState.phase = RunProgressPhase.Town;
        }

        /// <summary>
        /// 街から次の遠征を開始する。HP、デッキ（MODを含む）、所持ゴールドは RunState に残す。
        /// OverHunt の保留状態だけは帰還時の精算後なので、新しい遠征用にリセットする。
        /// </summary>
        public bool StartNextExpedition(RunState runState, RunDefinitionSO runDefinition)
        {
            if (runState == null || runState.phase != RunProgressPhase.Town)
            {
                return false;
            }

            if (!runState.hasStartedCurrentExpedition)
            {
                runState.hasStartedCurrentExpedition = true;
                runState.phase = RunProgressPhase.Expedition;
                return true;
            }

            if (runState.currentExpeditionIndex + 1 >= GetExpeditionCount(runDefinition))
            {
                return false;
            }

            runState.currentExpeditionIndex++;
            runState.currentStageIndex = 0;
            runState.hasStartedCurrentExpedition = true;
            runState.pendingGold = 0;
            runState.overhuntDepth = 0;
            runState.isInOverhunt = false;
            runState.phase = RunProgressPhase.Expedition;
            return true;
        }

        public bool IsFinalExpedition(RunDefinitionSO runDefinition, RunState runState)
        {
            return runState != null && runState.currentExpeditionIndex >= GetExpeditionCount(runDefinition) - 1;
        }

        public IReadOnlyList<KeystoneDefinition> GetAvailableKeystones(RunDefinitionSO runDefinition)
        {
            return runDefinition?.keystones?.Where(keystone => keystone != null && keystone.id != KeystoneId.None).ToList()
                ?? new List<KeystoneDefinition>();
        }

        public bool HasSelectedKeystone(RunState runState)
        {
            return runState != null && runState.selectedKeystone != KeystoneId.None;
        }

        public bool TrySelectKeystone(RunState runState, RunDefinitionSO runDefinition, KeystoneId keystoneId)
        {
            if (runState == null || runState.phase != RunProgressPhase.Town || HasSelectedKeystone(runState)) return false;

            KeystoneDefinition keystone = GetAvailableKeystones(runDefinition).FirstOrDefault(candidate => candidate.id == keystoneId);
            if (keystone == null) return false;

            runState.selectedKeystone = keystone.id;
            runState.ownedItemIds ??= new List<string>();
            runState.ownedItemIds.Add(keystone.id.ToString());
            runState.favoredBuildTags = keystone.favoredTags?.Distinct().ToList() ?? new List<BuildTag>();

            if (keystone.initialCards != null)
            {
                foreach (CardDefinitionSO card in keystone.initialCards.Where(card => card != null))
                    runState.playerDeck.Add(new CardBase(card, CardLevel.one));
            }

            return true;
        }

        public bool HasExpeditions(RunDefinitionSO runDefinition)
        {
            return GetExpeditionCount(runDefinition) > 0;
        }

        public void MarkRunFailed(RunState runState)
        {
            if (runState != null)
            {
                runState.phase = RunProgressPhase.Failed;
            }
        }

        public void MarkRunCompleted(RunState runState)
        {
            if (runState != null)
            {
                runState.phase = RunProgressPhase.Completed;
            }
        }

        public StageDefinitionSO GetCurrentEncounterStage(RunDefinitionSO runDefinition, RunState runState)
        {
            if (runState != null && runState.isInOverhunt)
            {
                return GetOverhuntStage(runDefinition, runState);
            }

            var expedition = GetCurrentExpedition(runDefinition, runState);
            if (expedition?.stages == null || runState == null || runState.currentStageIndex < 0 || runState.currentStageIndex >= expedition.stages.Count)
            {
                return null;
            }

            return expedition.stages[runState.currentStageIndex];
        }

        public bool ShouldCompleteBeforeStageStart(RunDefinitionSO runDefinition, RunState runState)
        {
            return runState != null
                && !runState.isInOverhunt
                && runState.currentStageIndex > GetBossStageIndex(runDefinition, runState);
        }

        public bool CanGoToOverhunt(RunDefinitionSO runDefinition, RunState runState)
        {
            return GetOverhuntStage(runDefinition, runState) != null;
        }

        private StageDefinitionSO GetOverhuntStage(RunDefinitionSO runDefinition, RunState runState)
        {
            var expedition = GetCurrentExpedition(runDefinition, runState);
            if (expedition?.overhuntStages == null || expedition.overhuntStages.Count == 0)
            {
                return null;
            }

            var availableStages = expedition.overhuntStages.Where(stage => stage != null).ToList();
            if (availableStages.Count == 0)
            {
                return null;
            }

            int index = runState != null ? runState.overhuntDepth % availableStages.Count : 0;
            return availableStages[index];
        }

        private bool IsBossStage(RunDefinitionSO runDefinition, RunState runState)
        {
            return runState != null
                && runState.currentStageIndex >= 0
                && runState.currentStageIndex == GetBossStageIndex(runDefinition, runState);
        }

        private int GetBossStageIndex(RunDefinitionSO runDefinition, RunState runState)
        {
            var expedition = GetCurrentExpedition(runDefinition, runState);
            return expedition?.stages != null ? expedition.stages.Count - 1 : -1;
        }

        private int GetExpeditionCount(RunDefinitionSO runDefinition)
        {
            if (runDefinition?.expeditions != null && runDefinition.expeditions.Count > 0)
            {
                return runDefinition.expeditions.Count(expedition => expedition?.stages != null && expedition.stages.Count > 0);
            }

            return runDefinition?.stages != null && runDefinition.stages.Count > 0 ? 1 : 0;
        }

        private ExpeditionDefinition GetCurrentExpedition(RunDefinitionSO runDefinition, RunState runState)
        {
            int expeditionIndex = runState?.currentExpeditionIndex ?? 0;
            if (runDefinition?.expeditions != null && runDefinition.expeditions.Count > 0)
            {
                return expeditionIndex >= 0 && expeditionIndex < runDefinition.expeditions.Count
                    ? runDefinition.expeditions[expeditionIndex]
                    : null;
            }

            if (runDefinition?.stages == null || runDefinition.stages.Count == 0 || expeditionIndex != 0)
            {
                return null;
            }

            return new ExpeditionDefinition
            {
                expeditionName = runDefinition.runName,
                stages = runDefinition.stages,
                overhuntStages = runDefinition.stages.Take(System.Math.Max(0, runDefinition.stages.Count - 1)).ToList()
            };
        }

        private void ResetPlayerDeckBattleState(RunState runState)
        {
            if (runState?.playerDeck == null)
            {
                return;
            }

            foreach (var card in runState.playerDeck)
            {
                card?.ResetBattleState();
            }
        }

        /// <summary>
        /// Instantカードは未使用でも戦闘をまたいで保持しない。
        /// </summary>
        private static void RemoveInstantCards(RunState runState)
        {
            runState?.playerDeck?.RemoveAll(card => card != null && card.RemovesAfterUse);
        }
    }
}
