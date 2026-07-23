using System.Collections.Generic;
using System.Linq;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Core.Modifiers;
using TestCardGame.Cards.VOs;
using TestCardGame.Rewards;
using TestCardGame.Run;
using TestCardGame.Stage;
using UnityEngine;

namespace TestCardGame.Controller
{
    public class RunController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private GameController gameController;
        [SerializeField] private HandView handView;
        [SerializeField] private RewardController rewardController;
        [SerializeField] private List<CardModifierSO> modifierPool = new();

        [Header("DEBUG")]
        [Tooltip("有効時は開始デッキをデバッグ攻撃・移動・ヘヴィストライクだけに置き換え、キーストーン選択後にその初期カードを追加します。")]
        [SerializeField] private bool DEBUG;
        [SerializeField] private CardDefinitionSO debugMoveCard;
        [SerializeField] private CardDefinitionSO debugAttackCard;
        [SerializeField] private CardDefinitionSO debugHeavyStrikeCard;

        [Header("Phase 2 Economy")]
        [SerializeField, Min(0)] private int normalBattleGoldReward = 15;
        [SerializeField, Min(0)] private int bossBattleGoldReward = 30;
        [SerializeField, Min(0)] private int overhuntBattleGoldReward = 20;

        [Header("Town Costs")]
        [SerializeField, Min(0)] private int townHealCost = 20;
        [SerializeField, Min(0)] private int townLevelUpCost = 30;
        [SerializeField, Min(0)] private int townModCost = 25;
        [SerializeField, Min(0)] private int townNewCardCost = 40;

        private readonly RunProgressService runProgressService = new();
        private TownService townService;
        private RunState runState;

        public RunState RunState => runState;
        public RunDefinitionSO RunDefinition => runDefinition;
        public IReadOnlyList<RewardChoice> RewardSelectionChoices => rewardController != null
            ? rewardController.RewardSelectionChoices
            : System.Array.Empty<RewardChoice>();

        public bool IsAwaitingRouteChoice => runState != null && runState.phase == RunProgressPhase.AwaitingReturnOrOverhuntChoice;
        public bool IsAwaitingOverhuntChoice => runState != null && runState.phase == RunProgressPhase.AwaitingOverhuntDecision;
        public bool IsTownOpen => runState != null && runState.phase == RunProgressPhase.Town;
        public bool CanGoToOverhunt => runProgressService.CanGoToOverhunt(runDefinition, runState);
        public bool IsFinalExpedition => runProgressService.IsFinalExpedition(runDefinition, runState);
        public bool HasSelectedKeystone => runProgressService.HasSelectedKeystone(runState);
        public IReadOnlyList<KeystoneDefinition> AvailableKeystones => runProgressService.GetAvailableKeystones(runDefinition);
        public int TownHealCost => townHealCost;
        public int TownLevelUpCost => townLevelUpCost;
        public int TownModCost => townModCost;
        public int TownNewCardCost => townNewCardCost;

        public event System.Action<RunState> RunStarted;
        public event System.Action<StageDefinitionSO, RunState> StageStarted;
        public event System.Action<RunState> RouteChoiceRequested;
        public event System.Action<RunState> OverhuntChoiceRequested;
        public event System.Action<RunState> TownOpened;
        public event System.Action<RunState> RunStateChanged;
        public event System.Action RunWon;
        public event System.Action RunLost;

        private void Awake()
        {
            townService = new TownService(new SystemRandomService());

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
            rewardController.RewardResolved -= OnRewardResolved;
            rewardController.RewardResolved += OnRewardResolved;
        }

        private void Start()
        {
            if (runDefinition != null)
            {
                var session = RunSession.GetOrCreate();
                if (session.HasActiveRun && session.Definition == runDefinition)
                {
                    runState = session.State;
                    if (runState.phase == RunProgressPhase.Expedition) StartCurrentStage();
                    return;
                }
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

            if (!runProgressService.HasExpeditions(runDefinition))
            {
                Debug.LogError("RunController: 遠征またはステージが設定されていません。");
                return;
            }

            var session = RunSession.GetOrCreate();
            session.Begin(runDefinition);
            runState = session.State;
            ApplyDebugStartDeck();

            Debug.Log($"ラン開始: {runDefinition.runName} を {runState.playerDeck.Count} 枚のカードで開始します。");
            RunStarted?.Invoke(runState);
            // RUN開始地点は戦闘ではなく街。盤面を作らずにUIだけ先に使えるようにする。
            gameController?.InitializeBattleUI();
            OpenTown();
        }

        public void StartCurrentStage()
        {
            if (runDefinition == null || runState == null)
            {
                return;
            }

            var currentStage = runProgressService.GetCurrentEncounterStage(runDefinition, runState);
            if (currentStage == null || runProgressService.ShouldCompleteBeforeStageStart(runDefinition, runState))
            {
                WinRun();
                return;
            }

            Debug.Log($"ステージ開始: {currentStage.stageName} (Phase: {runState.phase})");

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
            RunStateChanged?.Invoke(runState);
        }

        private void OnBattleEnded(BattleResult result)
        {
            if (runState == null)
            {
                return;
            }

            if (result == BattleResult.Win)
            {
                HandleBattleWin();
            }
            else if (result == BattleResult.Lose)
            {
                FailRun();
            }
        }

        private void HandleBattleWin()
        {
            int currentHp = runState.currentHp;
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                currentHp = gameController.PlayerUnitInstance.Hp.CurrentValue;
            }

            var result = runProgressService.HandleBattleWin(runState, runDefinition, CreateEconomyConfig(), currentHp);
            Debug.Log(result.Message);

            if (result.Flow == BattleWinFlow.RequestOverhuntChoice)
            {
                OverhuntChoiceRequested?.Invoke(runState);
                RunStateChanged?.Invoke(runState);
                return;
            }

            if (result.Flow == BattleWinFlow.RequestNormalReward)
            {
                RunStateChanged?.Invoke(runState);
                rewardController?.OpenRewardScreen(runState, MaxHp);
                return;
            }

            if (result.Flow == BattleWinFlow.RequestRouteChoice)
            {
                RouteChoiceRequested?.Invoke(runState);
                RunStateChanged?.Invoke(runState);
                return;
            }

            if (result.Flow == BattleWinFlow.StartNextStage)
            {
                StartCurrentStage();
            }
        }

        public void ChooseReturnToTown()
        {
            if (runState == null)
            {
                return;
            }

            string message = runProgressService.ChooseReturnToTown(runState);
            if (message == null)
            {
                return;
            }

            Debug.Log(message);
            OpenTown();
        }

        public void ChooseOverhunt()
        {
            if (!runProgressService.ChooseOverhunt(runState, runDefinition))
            {
                return;
            }

            Debug.Log("OverHuntへ進行します。死亡時はRUN失敗です。");
            RunStateChanged?.Invoke(runState);
            StartCurrentStage();
        }

        private void OnRewardResolved()
        {
            if (runState == null)
            {
                return;
            }

            if (runProgressService.CompleteNormalBattleReward(runState))
            {
                RunStateChanged?.Invoke(runState);
                StartCurrentStage();
            }
        }

        private void OpenTown()
        {
            if (runState == null)
            {
                return;
            }

            RunSession.GetOrCreate().OpenTown();
            Debug.Log($"Outpostに到着しました。所持ゴールド: {runState.ownedGold}");
            TownOpened?.Invoke(runState);
            RunStateChanged?.Invoke(runState);
            UnityEngine.SceneManagement.SceneManager.LoadScene("OutpostScene");
        }

        public bool TryBuyTownHeal()
        {
            var result = townService.TryBuyHeal(runState, CreateEconomyConfig(), MaxHp);
            if (!result.Succeeded)
            {
                return false;
            }

            SyncPlayerHpAfterTownHeal(result.HealAmount);
            Debug.Log(result.Message);
            RunStateChanged?.Invoke(runState);
            return true;
        }

        public bool TryBuyTownLevelUp()
        {
            var result = townService.TryBuyLevelUp(runState, CreateEconomyConfig());
            return CompleteTownPurchase(result);
        }

        public bool TryBuyTownModifier()
        {
            var result = townService.TryBuyModifier(runState, CreateEconomyConfig(), modifierPool);
            return CompleteTownPurchase(result);
        }

        public bool TryBuyTownNewCard()
        {
            var result = townService.TryBuyNewCard(runState, runDefinition, CreateEconomyConfig());
            return CompleteTownPurchase(result);
        }

        public void LeaveTown()
        {
            if (runState == null || runState.phase != RunProgressPhase.Town)
            {
                return;
            }

            if (!HasSelectedKeystone)
            {
                Debug.LogWarning("キーストーンを選択するまで遠征には出発できません。");
                return;
            }

            if (RunSession.GetOrCreate().TryStartNextExpedition())
            {
                Debug.Log($"次の遠征へ進行します。遠征 {runState.currentExpeditionIndex + 1} を開始します。");
                RunStateChanged?.Invoke(runState);
                StartCurrentStage();
                return;
            }

            WinRun();
        }

        public bool TrySelectKeystone(KeystoneId keystoneId)
        {
            if (!RunSession.GetOrCreate().TrySelectKeystone(keystoneId)) return false;

            KeystoneDefinition selected = AvailableKeystones.FirstOrDefault(keystone => keystone.id == keystoneId);
            Debug.Log($"キーストーンを取得: {selected?.displayName}。所持カード数: {runState.playerDeck.Count}");
            RunStateChanged?.Invoke(runState);
            return true;
        }

        private void ApplyDebugStartDeck()
        {
            if (!DEBUG || runState == null)
            {
                return;
            }

            if (debugMoveCard == null || debugAttackCard == null || debugHeavyStrikeCard == null)
            {
                Debug.LogError("RunController: DEBUGが有効ですが、デバッグ初期手札が設定されていません。");
                return;
            }

            runState.playerDeck.Clear();
            runState.playerDeck.Add(new CardBase(debugMoveCard, CardLevel.one));
            runState.playerDeck.Add(new CardBase(debugAttackCard, CardLevel.one));
            runState.playerDeck.Add(new CardBase(debugHeavyStrikeCard, CardLevel.one));
        }

        public bool CanBuyTownHeal()
        {
            return townService.CanBuyHeal(runState, CreateEconomyConfig(), MaxHp);
        }

        public bool CanBuyTownLevelUp()
        {
            return townService.CanBuyLevelUp(runState, CreateEconomyConfig());
        }

        public bool CanBuyTownModifier()
        {
            return townService.CanBuyModifier(runState, CreateEconomyConfig(), modifierPool);
        }

        public bool CanBuyTownNewCard()
        {
            return townService.CanBuyNewCard(runState, runDefinition, CreateEconomyConfig());
        }

        public void FailRun()
        {
            RunSession.GetOrCreate().MarkFailed();
            if (runState != null)
            {
                RunStateChanged?.Invoke(runState);
            }

            Debug.Log("ラン失敗: プレイヤーのHPが0になりました。OverHunt中の死亡も含めてRUN失敗です。");
            RunLost?.Invoke();
        }

        private bool CompleteTownPurchase(TownPurchaseResult result)
        {
            if (!result.Succeeded)
            {
                return false;
            }

            Debug.Log(result.Message);
            RunStateChanged?.Invoke(runState);
            return true;
        }

        private void SyncPlayerHpAfterTownHeal(int healAmount)
        {
            if (gameController == null || gameController.PlayerUnitInstance == null)
            {
                return;
            }

            var playerHp = gameController.PlayerUnitInstance.Hp;
            playerHp.Heal(healAmount);
            if (playerHp.CurrentValue > playerHp.InitialValue)
            {
                playerHp.TakeDamage(playerHp.CurrentValue - playerHp.InitialValue);
            }
            runState.currentHp = playerHp.CurrentValue;
        }

        private RunEconomyConfig CreateEconomyConfig()
        {
            return new RunEconomyConfig(
                normalBattleGoldReward,
                bossBattleGoldReward,
                overhuntBattleGoldReward,
                townHealCost,
                townLevelUpCost,
                townModCost,
                townNewCardCost);
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

        private void WinRun()
        {
            RunSession.GetOrCreate().MarkCompleted();
            if (runState != null)
            {
                RunStateChanged?.Invoke(runState);
            }

            Debug.Log("ラン勝利: 最小RUNループを完了しました。");
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
                rewardController.RewardResolved -= OnRewardResolved;
            }
        }
    }
}
