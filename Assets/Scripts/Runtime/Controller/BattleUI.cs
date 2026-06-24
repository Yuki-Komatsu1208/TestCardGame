using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Stage;
using TestCardGame.Run;
using TestCardGame.Cards.Core;
using TestCardGame.Rewards;

namespace TestCardGame.Controller
{
    public class BattleUI : MonoBehaviour
    {
        [Header("Pre-configured UI References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button endTurnButton;

        // 報酬やRun終了を表示する動的UI。
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private GameObject endRunPanel;
        [SerializeField] private TextMeshProUGUI rewardsTitleText;
        [SerializeField] private List<Button> rewardButtons = new();

        [Header("Deck overlay & view references")]
        [SerializeField] private Button checkDeckButton;
        [SerializeField] private UIs.CardListSelectionOverlay cardListOverlay;

        private GameController gameController;
        private RunController runController;

        /// <summary>
        /// バトルUIを構築し、RunControllerのイベント購読を開始する。
        /// </summary>
        public void Initialize(GameController controller)
        {
            this.gameController = controller;
            this.runController = FindAnyObjectByType<RunController>();

            // 事前配置UIがアサインされているか確認
            bool hasPreconfiguredUI = statusText != null && endTurnButton != null && rewardsPanel != null && endRunPanel != null;

            if (!hasPreconfiguredUI)
            {
                Debug.Log("BattleUI: 事前配置されたUIが不完全なため、動的にUIを生成します。");

                // 再生成時に古い動的UIを削除する。
                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("BattleUI: シーン内に Canvas が見つかりません。");
                    return;
                }

                foreach (Transform child in canvas.transform)
                {
                    if (child.name == "BattleUIPanel" || child.name == "EndTurnButton" || child.name == "RewardsPanel" || child.name == "EndRunPanel")
                    {
                        Destroy(child.gameObject);
                    }
                }

                // ステータス表示パネルを作成する。
                GameObject panelObj = new GameObject("BattleUIPanel", typeof(RectTransform));
                panelObj.transform.SetParent(canvas.transform, false);
                var panelRect = panelObj.GetComponent<RectTransform>();
                
                // 画面上部中央に配置する。
                panelRect.anchorMin = new Vector2(0f, 0.75f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot = new Vector2(0.5f, 1f);
                panelRect.anchoredPosition = new Vector2(0f, -20f);
                panelRect.sizeDelta = new Vector2(0f, 150f);

                // ステータステキストを追加する。
                GameObject textObj = new GameObject("StatusText", typeof(RectTransform));
                textObj.transform.SetParent(panelRect, false);
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                
                statusText = textObj.AddComponent<TextMeshProUGUI>();
                statusText.fontSize = 20;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.color = Color.white;
                statusText.text = "バトルを初期化しています...";

                // ターン終了ボタンを追加する。
                GameObject buttonObj = new GameObject("EndTurnButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObj.transform.SetParent(canvas.transform, false);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                
                buttonRect.anchorMin = new Vector2(1f, 0f);
                buttonRect.anchorMax = new Vector2(1f, 0f);
                buttonRect.pivot = new Vector2(1f, 0f);
                buttonRect.anchoredPosition = new Vector2(-20f, 20f);
                buttonRect.sizeDelta = new Vector2(160f, 60f);

                var btnImage = buttonObj.GetComponent<Image>();
                btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1.0f);

                endTurnButton = buttonObj.GetComponent<Button>();
                endTurnButton.onClick.AddListener(OnEndTurnClicked);

                GameObject btnTextObj = new GameObject("Text", typeof(RectTransform));
                btnTextObj.transform.SetParent(buttonRect, false);
                var btnTextRect = btnTextObj.GetComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.sizeDelta = Vector2.zero;

                var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
                btnText.fontSize = 20;
                btnText.alignment = TextAlignmentOptions.Center;
                btnText.color = Color.white;
                btnText.text = "ターン終了";

                // 報酬パネルを作成する。
                CreateRewardsPanel(canvas);

                // Run終了パネルを作成する。
                CreateEndRunPanel(canvas);
            }
            else
            {
                Debug.Log("BattleUI: 事前配置されたUIを使用します。");

                // ボタンのイベント登録を設定
                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
                endTurnButton.onClick.AddListener(OnEndTurnClicked);

                if (rewardsPanel != null) rewardsPanel.SetActive(false);
                if (endRunPanel != null) endRunPanel.SetActive(false);
            }

            // Find overlay and deck button if not assigned via Inspector
            if (cardListOverlay == null)
            {
                cardListOverlay = FindAnyObjectByType<UIs.CardListSelectionOverlay>();
            }

            if (checkDeckButton == null)
            {
                var existingBtn = GameObject.Find("CheckDeckButton");
                if (existingBtn != null)
                {
                    checkDeckButton = existingBtn.GetComponent<Button>();
                }
            }

            // Wire up the Check Deck button
            if (checkDeckButton != null)
            {
                checkDeckButton.onClick.RemoveAllListeners();
                checkDeckButton.onClick.AddListener(() =>
                {
                    if (runController != null && runController.RunState != null && cardListOverlay != null)
                    {
                        cardListOverlay.Show("現在のデッキ一覧", runController.RunState.playerDeck);
                    }
                });
            }

            // RunControllerのイベントで報酬/終了UIを切り替える。
            if (runController != null)
            {
                runController.RewardScreenOpened -= OnRewardScreenOpened;
                runController.RewardScreenOpened += OnRewardScreenOpened;

                runController.StageStarted -= OnStageStarted;
                runController.StageStarted += OnStageStarted;

                runController.RunWon -= OnRunWon;
                runController.RunWon += OnRunWon;

                runController.RunLost -= OnRunLost;
                runController.RunLost += OnRunLost;
            }

            Refresh();
        }

        /// <summary>
        /// ステージ勝利後に表示するカード報酬パネルを作成する。
        /// </summary>
        private void CreateRewardsPanel(Canvas canvas)
        {
            rewardsPanel = new GameObject("RewardsPanel", typeof(RectTransform), typeof(Image));
            rewardsPanel.transform.SetParent(canvas.transform, false);
            var rect = rewardsPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;

            rewardsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // タイトルを作成する。
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(rewardsPanel.transform, false);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.8f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.sizeDelta = Vector2.zero;

            rewardsTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            rewardsTitleText.fontSize = 28;
            rewardsTitleText.alignment = TextAlignmentOptions.Center;
            rewardsTitleText.color = Color.yellow;
            rewardsTitleText.text = "ステージクリア！報酬カードを選んでください";

            // 報酬カードを並べる領域を作成する。
            GameObject cardsGrid = new GameObject("CardsGrid", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardsGrid.transform.SetParent(rewardsPanel.transform, false);
            var gridRect = cardsGrid.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.05f, 0.2f);
            gridRect.anchorMax = new Vector2(0.95f, 0.75f);
            gridRect.sizeDelta = Vector2.zero;

            var layout = cardsGrid.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 20f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // 最大3つの報酬ボタンを作成する。
            rewardButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                GameObject cardBtnObj = new GameObject($"RewardBtn_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
                cardBtnObj.transform.SetParent(cardsGrid.transform, false);
                var btnImg = cardBtnObj.GetComponent<Image>();
                btnImg.color = new Color(0.25f, 0.25f, 0.35f, 1f);

                // Text
                GameObject cardTxtObj = new GameObject("Text", typeof(RectTransform));
                cardTxtObj.transform.SetParent(cardBtnObj.transform, false);
                var txtRect = cardTxtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.sizeDelta = Vector2.zero;

                var txtComp = cardTxtObj.AddComponent<TextMeshProUGUI>();
                txtComp.fontSize = 18;
                txtComp.alignment = TextAlignmentOptions.Center;
                txtComp.color = Color.white;
                txtComp.text = "報酬選択";

                var btn = cardBtnObj.GetComponent<Button>();
                rewardButtons.Add(btn);
            }

            rewardsPanel.SetActive(false);
        }

        /// <summary>
        /// Run勝利または敗北時に表示する終了パネルを作成する。
        /// </summary>
        private void CreateEndRunPanel(Canvas canvas)
        {
            endRunPanel = new GameObject("EndRunPanel", typeof(RectTransform), typeof(Image));
            endRunPanel.transform.SetParent(canvas.transform, false);
            var rect = endRunPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.2f);
            rect.anchorMax = new Vector2(0.85f, 0.8f);
            rect.sizeDelta = Vector2.zero;

            endRunPanel.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 0.98f);

            GameObject textObj = new GameObject("EndText", typeof(RectTransform));
            textObj.transform.SetParent(endRunPanel.transform, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 32;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.red;
            txt.text = "ラン失敗\nプレイヤーのHPが0になりました。";

            endRunPanel.SetActive(false);
        }

        /// <summary>
        /// 現在のバトル状態をテキストとボタン状態に反映する。
        /// </summary>
        public void Refresh()
        {
            if (gameController == null || statusText == null) return;

            var player = gameController.PlayerUnitInstance;
            if (player == null) return;

            string stageName = gameController.CurrentStageName;

            string playerStatusStr = GetStatusEffectsString(player);

            string enemiesInfo = "";
            int livingEnemiesCount = 0;
            foreach (var enemy in gameController.Enemies)
            {
                if (enemy == null) continue;
                if (enemy.Hp.CurrentValue > 0)
                {
                    livingEnemiesCount++;
                    string enemyStatuses = GetStatusEffectsString(enemy as IUnit);
                    enemiesInfo += $"\n{enemy.Name} HP: {enemy.Hp.CurrentValue} {enemyStatuses}";
                }
                else
                {
                    enemiesInfo += $"\n<s>{enemy.Name}（撃破済み）</s>";
                }
            }

            string turnName = gameController.IsPlayerTurn ? "<color=green>プレイヤーのターン</color>" : "<color=red>敵のターン</color>";
            statusText.text = $"<b>{stageName}</b> (残り敵: {livingEnemiesCount}体)\n" +
                             $"{turnName}\n" +
                             $"プレイヤーマナ: {player.Mana}/{player.MaxMana}\n" +
                             $"プレイヤーHP: {player.Hp.CurrentValue} {playerStatusStr}" +
                             $"{enemiesInfo}";

            if (endTurnButton != null)
            {
                endTurnButton.interactable = gameController.IsPlayerTurn && gameController.CurrentBattleResult == BattleResult.None;
            }
        }

        /// <summary>
        /// ユニットに付与されている状態異常を表示用文字列に変換する。
        /// </summary>
        private string GetStatusEffectsString(IUnit unit)
        {
            if (unit == null || unit.StatusEffects == null || unit.StatusEffects.Count == 0) return "";
            List<string> list = new();
            foreach (var s in unit.StatusEffects)
            {
                list.Add($"{s.Definition.DisplayName}({s.RemainingTurns}T, 値:{s.Value})");
            }
            return "[" + string.Join(", ", list) + "]";
        }

        /// <summary>
        /// ターン終了ボタンが押されたときにプレイヤーターン終了を要求する。
        /// </summary>
        private void OnEndTurnClicked()
        {
            if (gameController != null && gameController.IsPlayerTurn)
            {
                gameController.EndPlayerTurn();
            }
        }

        /// <summary>
        /// ステージ開始時に報酬/終了パネルを閉じて表示を更新する。
        /// </summary>
        private void OnStageStarted(StageDefinitionSO stageDef, RunState runState)
        {
            if (rewardsPanel != null) rewardsPanel.SetActive(false);
            if (endRunPanel != null) endRunPanel.SetActive(false);
            Refresh();
        }

        /// <summary>
        /// 報酬候補をボタンへ割り当て、報酬選択パネルを表示する。
        /// </summary>
        private void OnRewardScreenOpened(List<RewardChoice> choices)
        {
            if (rewardsPanel == null) return;

            rewardsPanel.SetActive(true);

            rewardsTitleText.text = "ステージクリア！報酬を選択してください";

            for (int i = 0; i < rewardButtons.Count; i++)
            {
                var btn = rewardButtons[i];
                var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i < choices.Count)
                {
                    var choice = choices[i];
                    btn.gameObject.SetActive(true);

                    if (choice.Type == RewardType.Mod)
                    {
                        textComp.text = $"<b><color=cyan>【MOD】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }
                    else if (choice.Type == RewardType.Heal)
                    {
                        textComp.text = $"<b><color=green>【HP回復】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }
                    else if (choice.Type == RewardType.LevelUp)
                    {
                        textComp.text = $"<b><color=yellow>【レベルアップ】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (choice.Type == RewardType.Heal)
                        {
                            rewardsPanel.SetActive(false);
                            runController?.ChooseHealReward();
                        }
                        else
                        {
                            // Transition to deck card selection screen
                            OpenDeckCardSelection(choice);
                        }
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// プレイヤーのデッキ一覧を表示し、MOD付与やレベルアップの対象カードを選択させる。
        /// </summary>
        private void OpenDeckCardSelection(RewardChoice choice)
        {
            if (runController == null || runController.RunState == null) return;

            string title = "";
            if (choice.Type == RewardType.Mod)
            {
                title = $"MOD付与: 対象カードを選択してください\n<color=cyan>【付与するMOD: {choice.Title}】</color>";
            }
            else if (choice.Type == RewardType.LevelUp)
            {
                title = "レベルアップ: 対象カードを選択してください\n<color=yellow>【カードレベル +1】</color>";
            }

            // Close the base rewards panel first
            if (rewardsPanel != null) rewardsPanel.SetActive(false);

            var overlay = cardListOverlay != null ? cardListOverlay : UIs.CardListSelectionOverlay.Instance;
            if (overlay != null)
            {
                overlay.Show(
                    title,
                    runController.RunState.playerDeck,
                    choice,
                    onSelect: (deckIndex) =>
                    {
                        runController?.ChooseDeckCardForReward(deckIndex, choice);
                    },
                    onCancel: () =>
                    {
                        // If selection cancelled, re-open the main rewards choice panel
                        if (rewardsPanel != null) rewardsPanel.SetActive(true);
                    }
                );
            }
        }

        /// <summary>
        /// Run勝利時の終了パネルを表示する。
        /// </summary>
        private void OnRunWon()
        {
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.05f, 0.2f, 0.05f, 0.98f);

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.yellow;
            txt.text = "ランクリア！\n\nすべてのステージを突破しました。";
        }

        /// <summary>
        /// Run敗北時の終了パネルを表示する。
        /// </summary>
        private void OnRunLost()
        {
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.2f, 0.05f, 0.05f, 0.98f);

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.red;
            txt.text = "ゲームオーバー\n\nプレイヤーのHPが0になりました。";
        }

        /// <summary>
        /// 破棄時にRunControllerのイベント購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (runController != null)
            {
                runController.RewardScreenOpened -= OnRewardScreenOpened;
                runController.StageStarted -= OnStageStarted;
                runController.RunWon -= OnRunWon;
                runController.RunLost -= OnRunLost;
            }
        }
    }
}
