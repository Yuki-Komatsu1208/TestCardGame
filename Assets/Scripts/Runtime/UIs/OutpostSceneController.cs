using System.Collections.Generic;
using TMPro;
using TestCardGame.Run;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>Outpostの施設選択を構築する。</summary>
    public sealed class OutpostSceneController : MonoBehaviour
    {
        [SerializeField] private ShopCatalogSO catalog;
        [SerializeField] private Sprite fieldBackground;
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private OutpostEconomyConfigSO innEconomy;

        private readonly Color panelColor = new(0.08f, 0.12f, 0.16f, 0.94f);

        private void Awake()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("OutpostSceneController: Canvas がありません。");
                return;
            }

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            if (!RunSession.GetOrCreate().HasActiveRun)
            {
                Debug.LogError("OutpostSceneController: 有効なRUNがありません。RunControllerから開始してください。");
                return;
            }

            Build(canvas.transform);
        }

        private void Build(Transform canvas)
        {
            CreateBackground(canvas);
            var root = CreatePanel(canvas, "OutpostRoot", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), panelColor);

            CreateText(root.transform, "Title", "OUTPOST", 32, TextAlignmentOptions.Center,
                new Vector2(0f, 0.9f), new Vector2(1f, 1f), Color.white);
            var state = RunSession.GetOrCreate().State;
            CreateText(root.transform, "Gold", $"所持ゴールド: {state.ownedGold}G / HP: {state.currentHp}/{MaxHp}", 20, TextAlignmentOptions.Right,
                new Vector2(0.65f, 0.84f), new Vector2(0.96f, 0.9f), new Color(1f, 0.82f, 0.25f));

            if (state.selectedKeystone == KeystoneId.None)
            {
                CreateKeystoneButtons(root.transform);
            }
            CreateFacilityButton(root.transform, "宿屋（全回復）", new Vector2(0.05f, 0.38f), new Vector2(0.45f, 0.5f), TryUseInn);
            CreateFacilityButton(root.transform, "ショップ", new Vector2(0.55f, 0.38f), new Vector2(0.95f, 0.5f), () => Debug.Log("ショップは次の実装対象です。"));
            CreateFacilityButton(root.transform, "工房", new Vector2(0.05f, 0.22f), new Vector2(0.45f, 0.34f), () => Debug.Log("工房は次の実装対象です。"));
            CreateFacilityButton(root.transform, "クエストカウンター", new Vector2(0.55f, 0.22f), new Vector2(0.95f, 0.34f), () => Debug.Log("クエストは次の実装対象です。"));
            CreateFacilityButton(root.transform, "次の遠征へ出発", new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.17f), Depart);
        }

        private void CreateBackground(Transform canvas)
        {
            var image = CreatePanel(canvas, "ShopBackground", Vector2.zero, Vector2.one, Color.white).GetComponent<Image>();
            image.sprite = fieldBackground;
            image.type = fieldBackground != null ? Image.Type.Tiled : Image.Type.Simple;
            image.color = fieldBackground != null ? new Color(0.48f, 0.48f, 0.48f, 1f) : new Color(0.08f, 0.16f, 0.12f, 1f);
            image.raycastTarget = false;
            image.transform.SetAsFirstSibling();
        }

        private int MaxHp => runDefinition?.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;

        private void TryUseInn()
        {
            float baseMultiplier = innEconomy != null ? innEconomy.innBaseMultiplier : 2f;
            float expeditionMultiplier = innEconomy != null ? innEconomy.innExpeditionMultiplier : 0f;
            if (!RunSession.GetOrCreate().TryUseInn(MaxHp, baseMultiplier, expeditionMultiplier, out int cost)) return;

            Debug.Log($"宿屋で全回復: {cost}G");
            UnityEngine.SceneManagement.SceneManager.LoadScene("OutpostScene");
        }

        private void Depart()
        {
            if (RunSession.GetOrCreate().TryStartNextExpedition())
                UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }

        private void CreateKeystoneButtons(Transform parent)
        {
            var panel = CreatePanel(parent, "Keystones", new Vector2(0.04f, 0.54f), new Vector2(0.96f, 0.82f), new Color(0.24f, 0.16f, 0.38f));
            CreateText(panel.transform, "Label", "開始キーストーンを選択してください（0G）", 18, TextAlignmentOptions.Center, new Vector2(0.02f, 0.7f), new Vector2(0.98f, 0.98f), Color.white);
            float width = 1f / Mathf.Max(1, runDefinition.keystones.Count);
            for (int i = 0; i < runDefinition.keystones.Count; i++)
            {
                var key = runDefinition.keystones[i];
                if (key == null) continue;
                var button = CreatePanel(panel.transform, key.displayName, new Vector2(i * width + 0.02f, 0.08f), new Vector2((i + 1) * width - 0.02f, 0.62f), new Color(0.08f, 0.07f, 0.12f, 1f)).AddComponent<Button>();
                var captured = key.id;
                button.onClick.AddListener(() =>
                {
                    if (RunSession.GetOrCreate().TrySelectKeystone(captured))
                        UnityEngine.SceneManagement.SceneManager.LoadScene("OutpostScene");
                });
                CreateText(button.transform, "Text", $"{key.displayName}\n{key.description}", 15, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Color.white);
            }
        }

        private static void CreateFacilityButton(Transform parent, string label, Vector2 min, Vector2 max, System.Action action)
        {
            var buttonObject = CreatePanel(parent, label, min, max, new Color(0.16f, 0.2f, 0.25f, 1f));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => action());
            CreateText(buttonObject.transform, "Text", label, 20, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Color.white);
        }

        private void CreateOfferRow(Transform parent, string title, List<ShopOffer> offers, Vector2 min, Vector2 max, Color color)
        {
            var row = CreatePanel(parent, title, min, max, color);
            CreateText(row.transform, "Label", title, 18, TextAlignmentOptions.Left,
                new Vector2(0.02f, 0.7f), new Vector2(0.98f, 0.98f), Color.white);

            var content = new GameObject("Offers", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            content.transform.SetParent(row.transform, false);
            var rect = content.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.06f);
            rect.anchorMax = new Vector2(0.98f, 0.67f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var layout = content.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            if (offers == null || offers.Count == 0)
            {
                CreateText(content.transform, "Empty", "商品未設定", 16, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Color.white);
                return;
            }

            foreach (var offer in offers)
            {
                if (offer == null) continue;
                var product = CreatePanel(content.transform, offer.DisplayName, Vector2.zero, Vector2.one, new Color(0.06f, 0.07f, 0.09f, 0.96f));
                var button = product.AddComponent<Button>();
                button.interactable = false;
                CreateText(product.transform, "Name", offer.DisplayName, 16, TextAlignmentOptions.Center,
                    new Vector2(0.04f, 0.42f), new Vector2(0.96f, 0.94f), Color.white);
                CreateText(product.transform, "Description", offer.description, 12, TextAlignmentOptions.Center,
                    new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.47f), new Color(0.82f, 0.82f, 0.82f));
                CreateText(product.transform, "Price", $"{offer.price}G", 15, TextAlignmentOptions.Right,
                    new Vector2(0.04f, 0.02f), new Vector2(0.94f, 0.2f), new Color(1f, 0.82f, 0.25f));
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static void CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Vector2 min, Vector2 max, Color color)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(parent, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var tmp = label.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
