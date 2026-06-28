using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Stage;
using TestCardGame.Rewards;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Controller
{
    public class RunController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private GameController gameController;
        [SerializeField] private HandView handView;

        private RunState runState;
        [SerializeField] private List<Cards.Core.Modifiers.CardModifierSO> modifierPool = new();
        private List<RewardChoice> rewardSelectionChoices = new();

        public RunState RunState => runState;
        public RunDefinitionSO RunDefinition => runDefinition;
        public IReadOnlyList<RewardChoice> RewardSelectionChoices => rewardSelectionChoices;

        public event System.Action<RunState> RunStarted;
        public event System.Action<StageDefinitionSO, RunState> StageStarted;
        public event System.Action<List<RewardChoice>> RewardScreenOpened;
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
                        runState.playerDeck.Add(new CardBase(entry.card, new CardLevel(entry.level)));
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
            ResetPlayerDeckBattleState();
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

        private bool IsHpReduced => runState != null && runState.currentHp < MaxHp;

        /// <summary>
        /// 現在ステージの報酬プールから候補カードを抽選し、報酬画面を開く。
        /// </summary>
        private void OpenRewardScreen()
        {
            rewardSelectionChoices.Clear();

            // 1. Guaranteed MOD 1
            var selectedMods = new List<Cards.Core.Modifiers.CardModifierSO>();
            Cards.Core.Modifiers.CardModifierSO mod1 = GetRandomUniqueModifier(selectedMods);
            if (mod1 != null)
            {
                selectedMods.Add(mod1);
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod1));
            }

            // 2. Guaranteed MOD 2
            Cards.Core.Modifiers.CardModifierSO mod2 = GetRandomUniqueModifier(selectedMods);
            if (mod2 != null)
            {
                selectedMods.Add(mod2);
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod2));
            }

            // 3. Option 3: HP recovery or Level Up (or MOD if Level Up rolls false)
            if (IsHpReduced)
            {
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Heal));
            }
            else
            {
                // Level Up has low probability (e.g. 20%). Otherwise show MOD 3.
                if (Random.value < 0.20f)
                {
                    rewardSelectionChoices.Add(new RewardChoice(RewardType.LevelUp));
                }
                else
                {
                    Cards.Core.Modifiers.CardModifierSO mod3 = GetRandomUniqueModifier(selectedMods);
                    if (mod3 != null)
                    {
                        rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod3));
                    }
                }
            }

            RewardScreenOpened?.Invoke(rewardSelectionChoices);
        }

        private Cards.Core.Modifiers.CardModifierSO GetRandomUniqueModifier(List<Cards.Core.Modifiers.CardModifierSO> excludeList)
        {
            var available = new List<Cards.Core.Modifiers.CardModifierSO>();
            foreach (var mod in modifierPool)
            {
                if (mod != null && !excludeList.Contains(mod))
                {
                    available.Add(mod);
                }
            }

            if (available.Count == 0)
            {
                // Fallback: If no unique modifiers are left, pick any from pool
                if (modifierPool.Count > 0)
                {
                    return modifierPool[Random.Range(0, modifierPool.Count)];
                }
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }

        /// <summary>
        /// HP回復報酬を適用する。
        /// </summary>
        public void ChooseHealReward()
        {
            int maxHp = MaxHp;
            int healAmount = Mathf.RoundToInt(maxHp * 0.25f);
            
            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                var hp = gameController.PlayerUnitInstance.Hp;
                hp.Heal(healAmount);
                if (hp.CurrentValue > hp.InitialValue)
                {
                    int excess = hp.CurrentValue - hp.InitialValue;
                    hp.TakeDamage(excess);
                }
                runState.currentHp = hp.CurrentValue;
            }
            else
            {
                runState.currentHp = Mathf.Min(maxHp, runState.currentHp + healAmount);
            }
            
            Debug.Log($"HP回復報酬選択: HPが {healAmount} 回復しました。現在HP: {runState.currentHp}/{maxHp}");
            
            AdvanceToNextStage();
        }

        /// <summary>
        /// MODまたはレベルアップ報酬を特定のカードに適用する。
        /// </summary>
        public void ChooseDeckCardForReward(int deckCardIndex, RewardChoice choice)
        {
            if (runState == null || choice == null || deckCardIndex < 0 || deckCardIndex >= runState.playerDeck.Count) return;
            var card = runState.playerDeck[deckCardIndex];
            if (!choice.CanApplyTo(card))
            {
                Debug.LogWarning("選択された報酬はこのカードに適用できません。");
                return;
            }

            if (choice.Type == RewardType.Mod)
            {
                choice.ApplyTo(card);
                Debug.Log($"MOD付与報酬選択: カード「{card.CardName}」にMOD「{choice.Modifier.DisplayName}」を付与しました。");
            }
            else if (choice.Type == RewardType.LevelUp)
            {
                choice.ApplyTo(card);
                Debug.Log($"レベルアップ報酬選択: カード「{card.CardName}」のレベルが {card.Level.Level} に上がりました。");
            }

            AdvanceToNextStage();
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (modifierPool == null || modifierPool.Count == 0)
            {
                PopulateModifierPool();
            }
        }

        [ContextMenu("Populate Modifier Pool")]
        public void PopulateModifierPool()
        {
            modifierPool.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardModifierSO");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var modifier = UnityEditor.AssetDatabase.LoadAssetAtPath<Cards.Core.Modifiers.CardModifierSO>(path);
                if (modifier != null)
                {
                    modifierPool.Add(modifier);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

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
