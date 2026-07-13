using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Stage;
using TestCardGame.Rewards;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using TestCardGame.Cards.Core.Modifiers;

namespace TestCardGame.Controller
{
    public class RunController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private GameController gameController;
        [SerializeField] private HandView handView;
        [SerializeField] private RewardController rewardController;
        [SerializeField] private List<CardModifierSO> modifierPool = new();

        private RunState runState;

        public RunState RunState => runState;
        public RunDefinitionSO RunDefinition => runDefinition;
        public IReadOnlyList<RewardChoice> RewardSelectionChoices => rewardController != null
            ? rewardController.RewardSelectionChoices
            : System.Array.Empty<RewardChoice>();

        public event System.Action<RunState> RunStarted;
        public event System.Action<StageDefinitionSO, RunState> StageStarted;
        public event System.Action RunWon;
        public event System.Action RunLost;

        private void Awake()
        {
            if (gameController == null)
            {
                gameController = FindAnyObjectByType<GameController>();
            }

            if (rewardController == null)
            {
                rewardController = GetComponent<RewardController>();
            }

            if (rewardController == null)
            {
                rewardController = gameObject.AddComponent<RewardController>();
            }

            if (gameController != null)
            {
                gameController.BattleEnded += OnBattleEnded;
            }

            rewardController.SetGameController(gameController);
            rewardController.ConfigureModifierPool(modifierPool);
            rewardController.RewardResolved -= AdvanceToNextStage;
            rewardController.RewardResolved += AdvanceToNextStage;
        }

        private void Start()
        {
            if (runDefinition != null)
            {
                StartRun();
            }
        }

        public void StartRun()
        {
            if (runDefinition == null)
            {
                Debug.LogError("RunController: RunDefinitionSO が設定されていません。");
                return;
            }

            runState = new RunState();
            runState.currentStageIndex = 0;
            runState.currentHp = runDefinition.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;

            runState.playerDeck.Clear();
            if (runDefinition.playerDefinition != null && runDefinition.playerDefinition.initialCards != null)
            {
                foreach (var entry in runDefinition.playerDefinition.initialCards)
                {
                    if (entry != null && entry.card != null)
                    {
                        runState.playerDeck.Add(new CardBase(entry.card, new CardLevel(entry.level)));
                    }
                }
            }

            Debug.Log($"ラン開始: {runDefinition.runName} を {runState.playerDeck.Count} 枚のカードで開始します。");
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
            Debug.Log($"ステージ開始: インデックス {runState.currentStageIndex} の {currentStage.stageName} を読み込みます。");

            if (gameController != null)
            {
                gameController.InitializeStage(currentStage, runState);
            }

            if (rewardController != null)
            {
                rewardController.SetGameController(gameController);
                rewardController.ConfigureModifierPool(modifierPool);
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
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                runState.currentHp = gameController.PlayerUnitInstance.Hp.CurrentValue;
            }

            Debug.Log("ステージクリア: 報酬を選択します。");
            ResetPlayerDeckBattleState();
            rewardController?.OpenRewardScreen(runState, MaxHp);
        }

        public void FailRun()
        {
            Debug.Log("ラン失敗: プレイヤーのHPが0になりました。");
            RunLost?.Invoke();
        }

        private int MaxHp
        {
            get
            {
                if (gameController != null && gameController.PlayerUnitInstance != null)
                {
                    return gameController.PlayerUnitInstance.Hp.InitialValue;
                }
                return runDefinition != null && runDefinition.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;
            }
        }

        private void AdvanceToNextStage()
        {
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

        private void ResetPlayerDeckBattleState()
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

        private void WinRun()
        {
            Debug.Log("ラン勝利: すべてのステージをクリアしました。");
            RunWon?.Invoke();
        }

        private void OnDestroy()
        {
            if (gameController != null)
            {
                gameController.BattleEnded -= OnBattleEnded;
            }

            if (rewardController != null)
            {
                rewardController.RewardResolved -= AdvanceToNextStage;
            }
        }
    }
}
