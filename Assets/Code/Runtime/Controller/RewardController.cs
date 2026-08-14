using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Rewards;
using TestCardGame.Cards.Core.Modifiers;

namespace TestCardGame.Controller
{
    /// <summary>
    /// 戦闘後報酬の提示と適用を担当する。
    /// </summary>
    public class RewardController : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private List<CardModifierSO> modifierPool = new();
        [Header("Reward Rules")]
        [SerializeField, Min(1)] private int guaranteedModChoices = 2;
        [SerializeField, Min(0)] private int minimumRecommendedModifierPoolSize = 5;
        [SerializeField, Range(0f, 1f)] private float lowHpThreshold = 0.40f;
        [SerializeField, Range(0f, 1f)] private float highHpThreshold = 0.70f;
        [SerializeField] private RewardRollProfile highHpProfile = new(0.85f, 0f, 0.15f);
        [SerializeField] private RewardRollProfile midHpProfile = new(0.65f, 0.20f, 0.15f);
        [SerializeField] private RewardRollProfile lowHpProfile = new(0.45f, 0.45f, 0.10f);

        private readonly List<RewardChoice> rewardSelectionChoices = new();
        private RunState currentRunState;
        private int currentMaxHp = 100;

        public IReadOnlyList<RewardChoice> RewardSelectionChoices => rewardSelectionChoices;

        public event System.Action<List<RewardChoice>> RewardScreenOpened;
        public event System.Action RewardResolved;

        /// <summary>
        /// 戦闘中オブジェクトへ報酬適用するための GameController を差し替える。
        /// </summary>
        public void SetGameController(GameController controller)
        {
            gameController = controller;
        }

        public void ConfigureModifierPool(IEnumerable<CardModifierSO> modifiers)
        {
            modifierPool.Clear();
            if (modifiers == null)
            {
                return;
            }

            modifierPool.AddRange(modifiers.Where(mod => mod != null).Distinct());
        }

        /// <summary>
        /// 現在の Run 状態に応じて報酬候補を生成し、報酬選択UIへ通知する。
        /// </summary>
        public void OpenRewardScreen(RunState runState, int maxHp)
        {
            currentRunState = runState;
            currentMaxHp = Mathf.Max(1, maxHp);
            rewardSelectionChoices.Clear();
            WarnIfModifierPoolIsShallow();

            // まずMODを2枠確定で並べ、残り1枠を状況に応じて埋める。
            var selectedMods = new List<CardModifierSO>();

            for (int i = 0; i < guaranteedModChoices; i++)
            {
                CardModifierSO mod = GetRandomUniqueModifier(selectedMods);
                if (mod != null)
                {
                    selectedMods.Add(mod);
                    rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod));
                }
            }

            RewardType specialRewardType = RollSpecialRewardType();
            if (specialRewardType == RewardType.Mod)
            {
                CardModifierSO mod = GetRandomUniqueModifier(selectedMods);
                if (mod != null)
                {
                    rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod));
                }
                else
                {
                    Debug.LogWarning("報酬用のMOD候補が不足しているため、特殊枠を追加できませんでした。");
                }
            }
            else
            {
                rewardSelectionChoices.Add(new RewardChoice(specialRewardType));
            }

            RewardScreenOpened?.Invoke(rewardSelectionChoices);
        }

        /// <summary>
        /// 回復報酬を適用し、報酬選択を完了する。
        /// </summary>
        public void ChooseHealReward()
        {
            if (currentRunState == null)
            {
                return;
            }

            int healAmount = Mathf.RoundToInt(currentMaxHp * 0.25f);

            if (gameController != null && gameController.PlayerUnitInstance != null)
            {
                // 戦闘中の実HPにもRunStateにも同じ結果を反映する。
                var hp = gameController.PlayerUnitInstance.Hp;
                hp.Heal(healAmount);
                if (hp.CurrentValue > hp.InitialValue)
                {
                    int excess = hp.CurrentValue - hp.InitialValue;
                    hp.TakeDamage(excess);
                }
                currentRunState.currentHp = hp.CurrentValue;
            }
            else
            {
                currentRunState.currentHp = Mathf.Min(currentMaxHp, currentRunState.currentHp + healAmount);
            }

            Debug.Log($"HP回復報酬選択: HPが {healAmount} 回復しました。現在HP: {currentRunState.currentHp}/{currentMaxHp}");
            CompleteRewardSelection();
        }

        /// <summary>
        /// 選択されたカードへ MOD 付与またはレベルアップを適用する。
        /// </summary>
        public void ChooseDeckCardForReward(int deckCardIndex, RewardChoice choice)
        {
            if (currentRunState == null || choice == null)
            {
                return;
            }

            if (deckCardIndex < 0 || deckCardIndex >= currentRunState.playerDeck.Count)
            {
                return;
            }

            var card = currentRunState.playerDeck[deckCardIndex];
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

            CompleteRewardSelection();
        }

        /// <summary>
        /// 現在HPを最大HP比率で返す。
        /// </summary>
        private float GetCurrentHpRatio()
        {
            if (currentRunState == null || currentMaxHp <= 0)
            {
                return 1f;
            }

            return (float)currentRunState.currentHp / currentMaxHp;
        }

        /// <summary>
        /// 現在のHP帯と候補可否に応じて、特殊枠の報酬タイプを抽選する。
        /// </summary>
        private RewardType RollSpecialRewardType()
        {
            RewardRollProfile profile = GetRewardRollProfile(GetCurrentHpRatio());
            bool canOfferHeal = CanOfferHealReward();
            bool canOfferLevelUp = CanOfferLevelUpReward();
            float modWeight = Mathf.Max(0f, profile.modWeight);
            float healWeight = canOfferHeal ? Mathf.Max(0f, profile.healWeight) : 0f;
            float levelUpWeight = canOfferLevelUp ? Mathf.Max(0f, profile.levelUpWeight) : 0f;
            float totalWeight = modWeight + healWeight + levelUpWeight;

            if (totalWeight <= 0f)
            {
                return RewardType.Mod;
            }

            float roll = Random.value * totalWeight;
            if (roll < modWeight)
            {
                return RewardType.Mod;
            }

            roll -= modWeight;
            if (roll < healWeight)
            {
                return RewardType.Heal;
            }

            return RewardType.LevelUp;
        }

        private RewardRollProfile GetRewardRollProfile(float hpRatio)
        {
            if (hpRatio >= highHpThreshold)
            {
                return highHpProfile;
            }

            if (hpRatio >= lowHpThreshold)
            {
                return midHpProfile;
            }

            return lowHpProfile;
        }

        private bool CanOfferHealReward()
        {
            return currentRunState != null && currentRunState.currentHp < currentMaxHp;
        }

        private bool CanOfferLevelUpReward()
        {
            return currentRunState != null
                && currentRunState.playerDeck != null
                && currentRunState.playerDeck.Any(card => card != null && card.Level.CanUpgrade);
        }

        /// <summary>
        /// 既に選ばれた候補を除外しつつ、MOD プールから1件抽選する。
        /// </summary>
        private CardModifierSO GetRandomUniqueModifier(List<CardModifierSO> excludeList)
        {
            var available = new List<CardModifierSO>();
            foreach (var mod in modifierPool)
            {
                if (mod != null && !excludeList.Contains(mod))
                {
                    available.Add(mod);
                }
            }

            if (available.Count == 0)
            {
                if (modifierPool.Count > 0)
                {
                    return modifierPool[Random.Range(0, modifierPool.Count)];
                }
                return null;
            }

            int totalWeight = available.Sum(mod => BuildWeightService.GetWeight(mod.BuildTags, currentRunState));
            int roll = Random.Range(0, totalWeight);
            foreach (CardModifierSO mod in available)
            {
                roll -= BuildWeightService.GetWeight(mod.BuildTags, currentRunState);
                if (roll < 0) return mod;
            }

            return available[available.Count - 1];
        }

        private void WarnIfModifierPoolIsShallow()
        {
            int uniqueCount = modifierPool.Where(mod => mod != null).Distinct().Count();
            if (uniqueCount < minimumRecommendedModifierPoolSize)
            {
                Debug.LogWarning($"MOD報酬プールが少なめです。現在の一意候補数: {uniqueCount}, 推奨最低数: {minimumRecommendedModifierPoolSize}");
            }
        }

        /// <summary>
        /// 報酬処理の一時状態を破棄し、Run の進行再開を通知する。
        /// </summary>
        private void CompleteRewardSelection()
        {
            // 報酬解決後はRunController側に進行再開だけ通知する。
            currentRunState = null;
            RewardResolved?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// インスペクタ更新時に MOD プールの初期化を補助する。
        /// </summary>
        private void OnValidate()
        {
            if (modifierPool == null || modifierPool.Count == 0)
            {
                PopulateModifierPool();
            }
        }

        /// <summary>
        /// プロジェクト内の CardModifierSO を収集して報酬プールへ設定する。
        /// </summary>
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
    }

    [System.Serializable]
    public class RewardRollProfile
    {
        [Range(0f, 1f)] public float modWeight = 0.85f;
        [Range(0f, 1f)] public float healWeight = 0f;
        [Range(0f, 1f)] public float levelUpWeight = 0.15f;

        public RewardRollProfile(float modWeight, float healWeight, float levelUpWeight)
        {
            this.modWeight = modWeight;
            this.healWeight = healWeight;
            this.levelUpWeight = levelUpWeight;
        }
    }
}
