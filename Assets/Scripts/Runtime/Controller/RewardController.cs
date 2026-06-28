using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Run;
using TestCardGame.Rewards;

namespace TestCardGame.Controller
{
    /// <summary>
    /// 戦闘後報酬の提示と適用を担当する。
    /// </summary>
    public class RewardController : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private List<Cards.Core.Modifiers.CardModifierSO> modifierPool = new();

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

        /// <summary>
        /// 現在の Run 状態に応じて報酬候補を生成し、報酬選択UIへ通知する。
        /// </summary>
        public void OpenRewardScreen(RunState runState, int maxHp)
        {
            currentRunState = runState;
            currentMaxHp = Mathf.Max(1, maxHp);
            rewardSelectionChoices.Clear();

            // まずMODを2枠確定で並べ、残り1枠を状況に応じて埋める。
            var selectedMods = new List<Cards.Core.Modifiers.CardModifierSO>();

            Cards.Core.Modifiers.CardModifierSO mod1 = GetRandomUniqueModifier(selectedMods);
            if (mod1 != null)
            {
                selectedMods.Add(mod1);
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod1));
            }

            Cards.Core.Modifiers.CardModifierSO mod2 = GetRandomUniqueModifier(selectedMods);
            if (mod2 != null)
            {
                selectedMods.Add(mod2);
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Mod, mod2));
            }

            float hpRatio = GetCurrentHpRatio();
            if (hpRatio <= 0.40f)
            {
                rewardSelectionChoices.Add(new RewardChoice(RewardType.Heal));
            }
            else
            {
                float roll = Random.value;
                if (roll < 0.40f)
                {
                    rewardSelectionChoices.Add(new RewardChoice(RewardType.Heal));
                }
                else if (roll < 0.70f)
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
        /// 既に選ばれた候補を除外しつつ、MOD プールから1件抽選する。
        /// </summary>
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
                if (modifierPool.Count > 0)
                {
                    return modifierPool[Random.Range(0, modifierPool.Count)];
                }
                return null;
            }

            return available[Random.Range(0, available.Count)];
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
}
