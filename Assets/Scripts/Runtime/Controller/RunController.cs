using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Stage;
using TestCardGame.Rewards;

namespace TestCardGame.Controller
{
    public class RunController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private GameController gameController;
        [SerializeField] private HandView handView;
        [SerializeField] private RewardController rewardController;

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

        /// <summary>
        /// シーン初期化時に依存コンポーネントとイベント購読を解決する。
        /// </summary>
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

            // 報酬の確定後だけ、Runの進行を再開する。
            rewardController.SetGameController(gameController);
            rewardController.RewardResolved -= AdvanceToNextStage;
            rewardController.RewardResolved += AdvanceToNextStage;

        }

        /// <summary>
        /// 依存解決後、Run定義があれば自動開始する。
        /// </summary>
        private void Start()
        {
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

            if (rewardController != null)
            {
                rewardController.SetGameController(gameController);
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
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                runState.currentHp = gameController.PlayerUnitInstance.Hp.CurrentValue;
            }

            Debug.Log("ステージクリア: 報酬を選択します。");
            // 報酬内容の生成と選択UI通知はRewardControllerへ委譲する。
            rewardController?.OpenRewardScreen(runState, MaxHp);
        }

        /// <summary>
        /// Run敗北を通知する。
        /// </summary>
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

        /// <summary>
        /// 報酬確定後にステージ番号を進め、次ステージまたは Run 終了へ遷移する。
        /// </summary>
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

            if (rewardController != null)
            {
                rewardController.RewardResolved -= AdvanceToNextStage;
            }
        }
    }
}
