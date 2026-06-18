using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Stage;
using TestCardGame.Rewards;
using TestCardGame.Cards.Core;

namespace TestCardGame.Controller
{
    public class RunController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private GameController gameController;
        [SerializeField] private HandView handView;

        private RunState runState;
        private List<CardDefinitionSO> rewardSelectionChoices = new();

        public RunState RunState => runState;
        public RunDefinitionSO RunDefinition => runDefinition;
        public IReadOnlyList<CardDefinitionSO> RewardSelectionChoices => rewardSelectionChoices;

        public event System.Action<RunState> RunStarted;
        public event System.Action<StageDefinitionSO, RunState> StageStarted;
        public event System.Action<List<CardDefinitionSO>> RewardScreenOpened;
        public event System.Action RunWon;
        public event System.Action RunLost;

        private void Start()
        {
            if (gameController == null)
            {
                gameController = FindAnyObjectByType<GameController>();
            }

            if (gameController != null)
            {
                gameController.BattleEnded += OnBattleEnded;
            }

            // Start default run automatically if runDefinition is assigned in the scene
            if (runDefinition != null)
            {
                StartRun();
            }
        }

        public void StartRun()
        {
            if (runDefinition == null)
            {
                Debug.LogError("RunController: RunDefinitionSO is not assigned.");
                return;
            }

            runState = new RunState();
            runState.currentStageIndex = 0;
            runState.currentHp = runDefinition.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;

            // Copy initial cards to run deck
            runState.playerDeck.Clear();
            if (runDefinition.playerDefinition != null && runDefinition.playerDefinition.initialCards != null)
            {
                foreach (var entry in runDefinition.playerDefinition.initialCards)
                {
                    if (entry != null && entry.card != null)
                    {
                        runState.playerDeck.Add(new DeckCardEntry(entry.card, entry.level));
                    }
                }
            }

            Debug.Log($"RunStarted: {runDefinition.runName} started with {runState.playerDeck.Count} cards.");
            RunStarted?.Invoke(runState);

            StartCurrentStage();
        }

        public void StartCurrentStage()
        {
            if (runDefinition == null || runState == null) return;

            if (runState.currentStageIndex >= runDefinition.stages.Count)
            {
                WinRun();
                return;
            }

            var currentStage = runDefinition.stages[runState.currentStageIndex];
            Debug.Log($"StageStarted: Loading stage index {runState.currentStageIndex}: {currentStage.stageName}");

            if (gameController != null)
            {
                gameController.InitializeStage(currentStage, runState);
            }

            if (handView == null)
            {
                handView = FindAnyObjectByType<HandView>();
            }

            if (handView != null && gameController != null)
            {
                handView.ShowCards(gameController.GetPlayerCards());
            }

            StageStarted?.Invoke(currentStage, runState);
        }

        private void OnBattleEnded(BattleResult result)
        {
            if (result == BattleResult.Win)
            {
                CompleteStage();
            }
            else if (result == BattleResult.Lose)
            {
                FailRun();
            }
        }

        public void CompleteStage()
        {
            // Sync player's HP back to RunState
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                runState.currentHp = gameController.PlayerUnitInstance.Hp.CurrentValue;
            }

            Debug.Log("StageCompleted: Choosing rewards...");
            OpenRewardScreen();
        }

        public void FailRun()
        {
            Debug.Log("RunFailed: Player HP reached 0.");
            RunLost?.Invoke();
        }

        private void OpenRewardScreen()
        {
            rewardSelectionChoices.Clear();
            var currentStage = runDefinition.stages[runState.currentStageIndex];

            if (currentStage.rewardPool != null && currentStage.rewardPool.candidates != null && currentStage.rewardPool.candidates.Count > 0)
            {
                // Draw 3 random candidates
                var candidatesList = new List<CardDefinitionSO>();
                foreach (var entry in currentStage.rewardPool.candidates)
                {
                    if (entry != null && entry.card != null)
                    {
                        candidatesList.Add(entry.card);
                    }
                }

                // Random draw (up to 3)
                int count = Mathf.Min(3, candidatesList.Count);
                for (int i = 0; i < count; i++)
                {
                    int index = Random.Range(0, candidatesList.Count);
                    rewardSelectionChoices.Add(candidatesList[index]);
                    candidatesList.RemoveAt(index);
                }
            }

            // Fallback default rewards if pool is empty
            if (rewardSelectionChoices.Count == 0 && gameController != null && gameController.PlayerUnitInstance != null)
            {
                // Grab up to 3 cards from definitions
                // Just a fallback
            }

            RewardScreenOpened?.Invoke(rewardSelectionChoices);
        }

        public void ChooseRewardCard(CardDefinitionSO selectedCard)
        {
            if (selectedCard != null)
            {
                runState.playerDeck.Add(new DeckCardEntry(selectedCard, 1));
                Debug.Log($"Chosen reward card: {selectedCard.cardName}. Adding to deck.");
            }

            // Advance stage
            runState.currentStageIndex++;

            if (runState.currentStageIndex >= runDefinition.stages.Count)
            {
                WinRun();
            }
            else
            {
                StartCurrentStage();
            }
        }

        private void WinRun()
        {
            Debug.Log("RunWon! All stages cleared!");
            RunWon?.Invoke();
        }

        private void OnDestroy()
        {
            if (gameController != null)
            {
                gameController.BattleEnded -= OnBattleEnded;
            }
        }
    }
}