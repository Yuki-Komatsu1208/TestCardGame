using System;
using TMPro;
using TestCardGame.Run;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>Outpostの施設選択とショップ表示を構築するUnity View。</summary>
    public sealed class OutpostSceneController : MonoBehaviour
    {
        [SerializeField] private ShopCatalogSO catalog;
        [SerializeField] private Sprite fieldBackground;
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private OutpostEconomyConfigSO innEconomy;

        private readonly Color panelColor = new(0.08f, 0.12f, 0.16f, 0.94f);
        private Transform canvas;

        private void Awake()
        {
            var foundCanvas = FindAnyObjectByType<Canvas>();
            if (foundCanvas == null)
            {
                Debug.LogError("OutpostSceneController: Canvas がありません。");
                return;
            }
            canvas = foundCanvas.transform;
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            if (!RunSession.GetOrCreate().HasActiveRun)
            {
                Debug.LogError("OutpostSceneController: 有効なRUNがありません。");
                return;
            }
            if (catalog != null)
                RunSession.GetOrCreate().EnsureShopInventory(catalog);
            BuildFacilityScreen();
        }

        private void BuildFacilityScreen()
        {
            ClearScreen();
            CreateBackground(canvas);
            var root = CreatePanel(canvas, "OutpostRoot", new Vector2(.04f, .04f), new Vector2(.96f, .96f), panelColor);
            CreateText(root.transform, "Title", "OUTPOST", 32, TextAlignmentOptions.Center, new Vector2(0, .9f), new Vector2(1, 1), Color.white);
            CreateStatus(root.transform, .84f, .9f);
            CreateFacilityButton(root.transform, "宿屋（全回復）", new Vector2(.05f, .38f), new Vector2(.45f, .5f), TryUseInn);
            CreateFacilityButton(root.transform, "ショップ", new Vector2(.55f, .38f), new Vector2(.95f, .5f), BuildShopScreen);
            CreateFacilityButton(root.transform, "工房", new Vector2(.05f, .22f), new Vector2(.45f, .34f), () => Debug.Log("工房は次の実装対象です。"));
            CreateFacilityButton(root.transform, "クエストカウンター", new Vector2(.55f, .22f), new Vector2(.95f, .34f), () => Debug.Log("クエストは次の実装対象です。"));
            CreateFacilityButton(root.transform, "次の遠征へ出発", new Vector2(.2f, .05f), new Vector2(.8f, .17f), Depart);
        }

        private void BuildShopScreen()
        {
            if (catalog == null)
            {
                Debug.LogError("OutpostSceneController: ShopCatalogSO が設定されていません。");
                return;
            }
            RunSession.GetOrCreate().EnsureShopInventory(catalog);
            ClearScreen();
            CreateBackground(canvas);
            var root = CreatePanel(canvas, "ShopRoot", new Vector2(.02f, .02f), new Vector2(.98f, .98f), panelColor);
            CreateText(root.transform, "Title", "ショップ", 30, TextAlignmentOptions.Center, new Vector2(0, .91f), new Vector2(1, 1), Color.white);
            CreateStatus(root.transform, .855f, .91f);
            CreateFacilityButton(root.transform, "戻る", new Vector2(.02f, .02f), new Vector2(.18f, .1f), BuildFacilityScreen);
            CreateKeystoneRow(root.transform);
            CreateCardOffers(root.transform);
        }

        private void CreateKeystoneRow(Transform parent)
        {
            var state = RunSession.GetOrCreate().State;
            var row = CreatePanel(parent, "Keystones", new Vector2(.02f, .66f), new Vector2(.98f, .83f), new Color(.24f, .16f, .38f));
            CreateText(row.transform, "Label", "キーストーン（60G・RUN中は1個まで）", 17, TextAlignmentOptions.Left, new Vector2(.02f, .73f), new Vector2(.98f, .98f), Color.white);
            if (state.selectedKeystone != KeystoneId.None)
            {
                var owned = catalog.keystones.Find(key => key != null && key.id == state.selectedKeystone);
                CreateText(row.transform, "Owned", $"購入済み: {owned?.displayName ?? state.selectedKeystone.ToString()}", 19, TextAlignmentOptions.Center, new Vector2(.05f, .14f), new Vector2(.95f, .67f), new Color(1f, .82f, .25f));
                return;
            }
            int count = catalog.keystones?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                var keystone = catalog.keystones[i];
                if (keystone == null || keystone.id == KeystoneId.None) continue;
                float width = 1f / count;
                var product = CreatePanel(row.transform, keystone.displayName, new Vector2(i * width + .015f, .08f), new Vector2((i + 1) * width - .015f, .65f), new Color(.06f, .07f, .09f, .96f));
                var button = product.AddComponent<Button>();
                button.interactable = state.ownedGold >= catalog.keystonePrice;
                var captured = keystone.id;
                button.onClick.AddListener(() => { if (RunSession.GetOrCreate().TryBuyKeystone(catalog, captured)) BuildShopScreen(); });
                CreateText(product.transform, "Text", $"{keystone.displayName}\n{keystone.description}\n{catalog.keystonePrice}G", 13, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Color.white);
            }
        }

        private void CreateCardOffers(Transform parent)
        {
            var state = RunSession.GetOrCreate().State;
            var row = CreatePanel(parent, "Cards", new Vector2(.02f, .12f), new Vector2(.98f, .63f), new Color(.12f, .2f, .28f));
            CreateText(row.transform, "Label", "カード（このOutpost滞在中は固定・購入済み枠は補充なし）", 17, TextAlignmentOptions.Left, new Vector2(.02f, .9f), new Vector2(.98f, .99f), Color.white);
            for (int i = 0; i < state.shopCardOffers.Count; i++)
            {
                var offer = state.shopCardOffers[i];
                int column = i % 5;
                int line = i / 5;
                float minX = .015f + column * .197f;
                float minY = line == 0 ? .47f : .04f;
                var product = CreatePanel(row.transform, $"Card_{i}", new Vector2(minX, minY), new Vector2(minX + .18f, minY + .37f), new Color(.06f, .07f, .09f, .96f));
                var button = product.AddComponent<Button>();
                button.interactable = !offer.isPurchased && state.ownedGold >= offer.price;
                int capturedIndex = i;
                button.onClick.AddListener(() => { if (RunSession.GetOrCreate().TryBuyShopCard(catalog, capturedIndex)) BuildShopScreen(); });
                string price = offer.isOnSale ? $"SALE\n{offer.price}G（半額）" : $"{offer.price}G";
                string label = offer.isPurchased ? "購入済み" : price;
                CreateText(product.transform, "Text", $"{offer.card.cardName}\n{label}", 14, TextAlignmentOptions.Center, new Vector2(.03f, .08f), new Vector2(.97f, .94f), offer.isOnSale ? new Color(1f, .7f, .25f) : Color.white);
            }
        }

        private void CreateStatus(Transform parent, float minY, float maxY)
        {
            var state = RunSession.GetOrCreate().State;
            int maxStunResist = runDefinition?.playerDefinition != null ? runDefinition.playerDefinition.maxStunResist : 100;
            CreateText(parent, "Gold", $"所持ゴールド: {state.ownedGold}G / HP: {state.currentHp}/{MaxHp} / スタン耐性: {state.currentStunResist}/{maxStunResist}", 20, TextAlignmentOptions.Right, new Vector2(.45f, minY), new Vector2(.98f, maxY), new Color(1f, .82f, .25f));
        }

        private void ClearScreen()
        {
            for (int i = canvas.childCount - 1; i >= 0; i--) Destroy(canvas.GetChild(i).gameObject);
        }
        private void CreateBackground(Transform parent)
        {
            var image = CreatePanel(parent, "OutpostBackground", Vector2.zero, Vector2.one, Color.white).GetComponent<Image>();
            image.sprite = fieldBackground;
            image.type = fieldBackground != null ? Image.Type.Tiled : Image.Type.Simple;
            image.color = fieldBackground != null ? new Color(.48f, .48f, .48f, 1) : new Color(.08f, .16f, .12f, 1);
            image.raycastTarget = false;
            image.transform.SetAsFirstSibling();
        }
        private int MaxHp => runDefinition?.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;
        private void TryUseInn()
        {
            float baseMultiplier = innEconomy != null ? innEconomy.innBaseMultiplier : 2f;
            float expeditionMultiplier = innEconomy != null ? innEconomy.innExpeditionMultiplier : 0f;
            if (RunSession.GetOrCreate().TryUseInn(MaxHp, baseMultiplier, expeditionMultiplier, out int cost)) BuildFacilityScreen();
        }
        private void Depart()
        {
            if (RunSession.GetOrCreate().TryStartNextExpedition()) UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
        private static void CreateFacilityButton(Transform parent, string label, Vector2 min, Vector2 max, Action action)
        {
            var buttonObject = CreatePanel(parent, label, min, max, new Color(.16f, .2f, .25f, 1));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => action());
            CreateText(buttonObject.transform, "Text", label, 20, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Color.white);
        }
        private static GameObject CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image)); panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color; return panel;
        }
        private static void CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Vector2 min, Vector2 max, Color color)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); label.transform.SetParent(parent, false);
            var rect = label.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var tmp = label.GetComponent<TextMeshProUGUI>(); tmp.text = text; tmp.fontSize = fontSize; tmp.alignment = alignment; tmp.color = color; tmp.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
