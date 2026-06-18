using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Stage;
using TestCardGame.Run;
using TestCardGame.Cards.Core;

namespace TestCardGame.Controller
{
    public class BattleUI : MonoBehaviour
    {
        private GameController gameController;
        private RunController runController;

        private TextMeshProUGUI statusText;
        private Button endTurnButton;

        // Custom UI GameObjects for dynamic overlays
        private GameObject rewardsPanel;
        private GameObject endRunPanel;
        private TextMeshProUGUI rewardsTitleText;
        private List<Button> rewardButtons = new();

        public void Initialize(GameController controller)
        {
            this.gameController = controller;
            this.runController = FindAnyObjectByType<RunController>();

            // Clean up existing UI panels under canvas if recreating
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("BattleUI: No Canvas found in the scene.");
                return;
            }

            foreach (Transform child in canvas.transform)
            {
                if (child.name == "BattleUIPanel" || child.name == "EndTurnButton" || child.name == "RewardsPanel" || child.name == "EndRunPanel")
                {
                    Destroy(child.gameObject);
                }
            }

            // Create status UI panel
            GameObject panelObj = new GameObject("BattleUIPanel", typeof(RectTransform));
            panelObj.transform.SetParent(canvas.transform, false);
            var panelRect = panelObj.GetComponent<RectTransform>();
            
            // Position at top center
            panelRect.anchorMin = new Vector2(0f, 0.75f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -20f);
            panelRect.sizeDelta = new Vector2(0f, 150f);

            // Add Status Text
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
            statusText.text = "Initializing Battle...";

            // Add End Turn Button
            GameObject buttonObj = new GameObject("EndTurnButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(canvas.transform, false);
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-20f, 20f);
            buttonRect.sizeDelta = new Vector2(160f, 60f);

            var btnImage = buttonObj.GetComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1.0f); // Green

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

            // Build dynamic rewards panel
            CreateRewardsPanel(canvas);

            // Build dynamic game over / run win panel
            CreateEndRunPanel(canvas);

            // Subscribe to RunController events to toggle rewards panel
            if (runController != null)
            {
                runController.RewardScreenOpened += OnRewardScreenOpened;
                runController.StageStarted += OnStageStarted;
                runController.RunWon += OnRunWon;
                runController.RunLost += OnRunLost;
            }

            Refresh();
        }

        private void CreateRewardsPanel(Canvas canvas)
        {
            rewardsPanel = new GameObject("RewardsPanel", typeof(RectTransform), typeof(Image));
            rewardsPanel.transform.SetParent(canvas.transform, false);
            var rect = rewardsPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;

            rewardsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Title
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
            rewardsTitleText.text = "STAGE WON! Choose a Card Reward:";

            // Grid for cards
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

            // Spawn 3 Reward Buttons
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
                txtComp.text = "Card Reward";

                var btn = cardBtnObj.GetComponent<Button>();
                rewardButtons.Add(btn);
            }

            rewardsPanel.SetActive(false);
        }

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
            txt.text = "RUN LOST\nYour HP reached 0.";

            endRunPanel.SetActive(false);
        }

        public void Refresh()
        {
            if (gameController == null || statusText == null) return;

            var player = gameController.PlayerUnitInstance;
            if (player == null) return;

            // Compute active stage name
            string stageName = gameController.CurrentStageName;

            // Compute player statuses
            string playerStatusStr = GetStatusEffectsString(player);

            // Compute enemies list HP & statuses
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
                    enemiesInfo += $"\n<s>{enemy.Name} (DEAD)</s>";
                }
            }

            string turnName = gameController.IsPlayerTurn ? "<color=green>プレイヤーのターン</color>" : "<color=red>敵のターン</color>";
            statusText.text = $"<b>{stageName}</b> (残り敵: {livingEnemiesCount}体)\n" +
                             $"{turnName}\n" +
                             $"プレイヤーマナ: {player.Mana}/{player.MaxMana}\n" +
                             $"Player HP: {player.Hp.CurrentValue} {playerStatusStr}" +
                             $"{enemiesInfo}";

            if (endTurnButton != null)
            {
                endTurnButton.interactable = gameController.IsPlayerTurn && gameController.CurrentBattleResult == BattleResult.None;
            }
        }

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

        private void OnEndTurnClicked()
        {
            if (gameController != null && gameController.IsPlayerTurn)
            {
                gameController.EndPlayerTurn();
            }
        }

        // RunController Event Handlers
        private void OnStageStarted(StageDefinitionSO stageDef, RunState runState)
        {
            if (rewardsPanel != null) rewardsPanel.SetActive(false);
            if (endRunPanel != null) endRunPanel.SetActive(false);
            Refresh();
        }

        private void OnRewardScreenOpened(List<CardDefinitionSO> choices)
        {
            if (rewardsPanel == null) return;

            rewardsPanel.SetActive(true);

            for (int i = 0; i < rewardButtons.Count; i++)
            {
                var btn = rewardButtons[i];
                var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i < choices.Count)
                {
                    var cardDef = choices[i];
                    var levelData = cardDef.GetDataForLevel(1);
                    btn.gameObject.SetActive(true);
                    textComp.text = $"<b>{cardDef.cardName}</b>\nコスト: {levelData.cost}\n{levelData.description}";

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        rewardsPanel.SetActive(false);
                        runController?.ChooseRewardCard(cardDef);
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        private void OnRunWon()
        {
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.05f, 0.2f, 0.05f, 0.98f); // Green success background

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.yellow;
            txt.text = "RUN COMPLETED!\n\nAll stages cleared successfully!\nThank you for playing!";
        }

        private void OnRunLost()
        {
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.2f, 0.05f, 0.05f, 0.98f); // Red failure background

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.red;
            txt.text = "GAME OVER\n\nYour HP has reached 0.\nTry again!";
        }

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