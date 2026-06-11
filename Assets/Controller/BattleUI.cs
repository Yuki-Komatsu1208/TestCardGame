using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestCardGame.Controller;
using TestCardGame.Charactor.Player;

namespace TestCardGame.Controller
{
    public class BattleUI : MonoBehaviour
    {
        private GameController gameController;
        private TextMeshProUGUI statusText;
        private Button endTurnButton;

        public void Initialize(GameController controller)
        {
            this.gameController = controller;

            // Find the canvas in the scene
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("BattleUI: No Canvas found in the scene.");
                return;
            }

            // Create status UI panel
            GameObject panelObj = new GameObject("BattleUIPanel", typeof(RectTransform));
            panelObj.transform.SetParent(canvas.transform, false);
            var panelRect = panelObj.GetComponent<RectTransform>();
            
            // Position at top center
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -20f);
            panelRect.sizeDelta = new Vector2(500f, 100f);

            // Add Status Text (TextMeshProUGUI)
            GameObject textObj = new GameObject("StatusText", typeof(RectTransform));
            textObj.transform.SetParent(panelRect, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            statusText = textObj.AddComponent<TextMeshProUGUI>();
            statusText.fontSize = 24;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.color = Color.white;
            statusText.text = "Initializing Battle...";

            // Add End Turn Button
            GameObject buttonObj = new GameObject("EndTurnButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(canvas.transform, false);
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            
            // Position at bottom right
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-20f, 20f);
            buttonRect.sizeDelta = new Vector2(160f, 60f);

            var btnImage = buttonObj.GetComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1.0f); // Green button

            endTurnButton = buttonObj.GetComponent<Button>();
            endTurnButton.onClick.AddListener(OnEndTurnClicked);

            // Add text inside button
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

            Refresh();
        }

        public void Refresh()
        {
            if (gameController == null || statusText == null) return;

            var player = gameController.PlayerUnitInstance;
            var enemy = gameController.EnemyUnitInstance;

            if (player != null && enemy != null)
            {
                string turnName = gameController.IsPlayerTurn ? "<color=green>プレイヤーのターン</color>" : "<color=red>敵のターン</color>";
                statusText.text = $"{turnName}\nマナ: {player.Mana}/{player.MaxMana}\n" +
                                 $"Player HP: {player.Hp.CurrentValue} | Enemy HP: {enemy.Hp.CurrentValue}";
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable = gameController.IsPlayerTurn;
            }
        }

        private void OnEndTurnClicked()
        {
            if (gameController != null && gameController.IsPlayerTurn)
            {
                gameController.EndPlayerTurn();
            }
        }
    }
}