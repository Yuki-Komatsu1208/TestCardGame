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

        /// <summary>
        /// シーン開始時に依存コンポーネントを解決し、Run定義があれば自動開始する。
        /// </summary>
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

            // シーンにRun定義が設定されている場合は自動的に開始する。
            if (runDefinition != null)
            {
                StartRun();
            }
        }

        /// <summary>
        /// Run状態を初期化し、初期デッキを作成して最初のステージを開始する。
        /// </summary>
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

            // プレイヤー定義の初期カードをRun中のデッキへコピーする。
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

            Debug.Log($"ラン開始: {runDefinition.runName} を {runState.playerDeck.Count} 枚のカードで開始します。");
            RunStarted?.Invoke(runState);

            StartCurrentStage();
        }

        /// <summary>
        /// 現在のステージ番号に対応するステージを読み込み、バトルを開始する。
        /// </summary>
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

        /// <summary>
        /// バトル終了通知を受け取り、勝利なら報酬へ、敗北ならRun失敗へ遷移する。
        /// </summary>
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

        /// <summary>
        /// ステージ勝利時にHPをRun状態へ同期し、報酬選択を開始する。
        /// </summary>
        public void CompleteStage()
        {
            // プレイヤーの現在HPをRun状態へ戻す。
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                runState.currentHp = gameController.PlayerUnitInstance.Hp.CurrentValue;
            }

            Debug.Log("ステージクリア: 報酬を選択します。");
            OpenRewardScreen();
        }

        /// <summary>
        /// Run敗北を通知する。
        /// </summary>
        public void FailRun()
        {
            Debug.Log("ラン失敗: プレイヤーのHPが0になりました。");
            RunLost?.Invoke();
        }

        /// <summary>
        /// 現在ステージの報酬プールから候補カードを抽選し、報酬画面を開く。
        /// </summary>
        private void OpenRewardScreen()
        {
            rewardSelectionChoices.Clear();
            var currentStage = runDefinition.stages[runState.currentStageIndex];

            if (currentStage.rewardPool != null && currentStage.rewardPool.candidates != null && currentStage.rewardPool.candidates.Count > 0)
            {
                // 報酬候補を最大3枚抽選する。
                var candidatesList = new List<CardDefinitionSO>();
                foreach (var entry in currentStage.rewardPool.candidates)
                {
                    if (entry != null && entry.card != null)
                    {
                        candidatesList.Add(entry.card);
                    }
                }

                int count = Mathf.Min(3, candidatesList.Count);
                for (int i = 0; i < count; i++)
                {
                    int index = Random.Range(0, candidatesList.Count);
                    rewardSelectionChoices.Add(candidatesList[index]);
                    candidatesList.RemoveAt(index);
                }
            }

            // 報酬プールが空の場合の予備処理。
            if (rewardSelectionChoices.Count == 0 && gameController != null && gameController.PlayerUnitInstance != null)
            {
                // 現時点では予備報酬は設定しない。
            }

            RewardScreenOpened?.Invoke(rewardSelectionChoices);
        }

        /// <summary>
        /// 選択した報酬カードをデッキへ追加し、次ステージまたはRun勝利へ進める。
        /// </summary>
        public void ChooseRewardCard(CardDefinitionSO selectedCard)
        {
            if (selectedCard != null)
            {
                runState.playerDeck.Add(new DeckCardEntry(selectedCard, 1));
                Debug.Log($"報酬カード選択: {selectedCard.cardName} をデッキへ追加します。");
            }

            // 次ステージへ進める。
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

        /// <summary>
        /// 全ステージクリアを通知する。
        /// </summary>
        private void WinRun()
        {
            Debug.Log("ラン勝利: すべてのステージをクリアしました。");
            RunWon?.Invoke();
        }

        /// <summary>
        /// 破棄時にイベント購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (gameController != null)
            {
                gameController.BattleEnded -= OnBattleEnded;
            }
        }
    }
}
