using System;
using TMPro;
using TestCardGame.UIs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>Outpost専用PrefabとScene上の事前配置UIを構築する。</summary>
    public static class OutpostUiAssetBuilder
    {
        private const string Directory = "Assets/GameData/Prefabs/OutpostUI";
        private const string CardSlotPath = Directory + "/OutpostCardProductSlot.prefab";
        private const string KeystoneSlotPath = Directory + "/OutpostKeystoneProductSlot.prefab";
        private const string RootPath = Directory + "/OutpostUiRoot.prefab";
        private const string ScenePath = "Assets/GameData/Scenes/OutpostScene.unity";
        private static readonly Color Ink = new(0.08f, 0.07f, 0.06f, 1f);
        private static readonly Color Parchment = new(0.82f, 0.75f, 0.60f, 0.98f);
        private static readonly Color Dark = new(0.10f, 0.12f, 0.13f, 0.98f);
        private static readonly Color Rust = new(0.48f, 0.16f, 0.10f, 1f);

        [MenuItem("TestCardGame/UI/Outpost UIアセットを構築")]
        public static void BuildAll()
        {
            BuildPrefabsOnly();
            InstallIntoScene();
            OutpostUiAssetValidator.ValidateAll();
        }

        /// <summary>並列作業中でもSceneを変更せずOutpost専用Prefabだけを生成する。</summary>
        public static void BuildPrefabsOnly()
        {
            EnsureDirectory();
            BuildCardSlot();
            BuildKeystoneSlot();
            BuildRoot();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Outpost専用Prefabを構築しました。");
        }

        private static void BuildCardSlot()
        {
            GameObject root = Image("OutpostCardProductSlot", null, Vector2.zero, Vector2.one, Parchment, true);
            var view = root.AddComponent<OutpostCardProductSlotView>();
            Button button = root.AddComponent<Button>();
            TMP_Text title = Text("Name", root.transform, "カード名", 20, new Vector2(.05f, .54f), new Vector2(.95f, .94f));
            TMP_Text price = Text("Price", root.transform, "20 G", 18, new Vector2(.05f, .08f), new Vector2(.95f, .34f));
            GameObject sale = Tag("SaleTag", root.transform, "SALE", new Color(.88f, .55f, .08f, 1f), new Vector2(.02f, .72f), new Vector2(.38f, .98f));
            GameObject sold = Tag("SoldOutTag", root.transform, "売り切れ", Rust, new Vector2(.08f, .35f), new Vector2(.92f, .67f));
            sold.SetActive(false);
            Set(view, "nameText", title); Set(view, "priceText", price); Set(view, "saleTag", sale); Set(view, "soldOutTag", sold); Set(view, "button", button);
            PrefabUtility.SaveAsPrefabAsset(root, CardSlotPath); UnityEngine.Object.DestroyImmediate(root);
        }

        private static void BuildKeystoneSlot()
        {
            GameObject root = Image("OutpostKeystoneProductSlot", null, Vector2.zero, Vector2.one, Parchment, true);
            var view = root.AddComponent<OutpostKeystoneProductSlotView>();
            Button button = root.AddComponent<Button>();
            TMP_Text title = Text("Name", root.transform, "キーストーン", 21, new Vector2(.05f, .68f), new Vector2(.95f, .94f));
            TMP_Text description = Text("Description", root.transform, "説明", 15, new Vector2(.06f, .25f), new Vector2(.94f, .68f));
            TMP_Text price = Text("Price", root.transform, "60 G", 18, new Vector2(.05f, .04f), new Vector2(.95f, .24f));
            GameObject sold = Tag("SoldOutTag", root.transform, "売り切れ", Rust, new Vector2(.06f, .34f), new Vector2(.94f, .66f));
            GameObject unavailable = Tag("UnavailableTag", root.transform, "購入不能", new Color(.28f, .28f, .28f, 1f), new Vector2(.06f, .34f), new Vector2(.94f, .66f));
            sold.SetActive(false); unavailable.SetActive(false);
            Set(view, "nameText", title); Set(view, "descriptionText", description); Set(view, "priceText", price); Set(view, "soldOutTag", sold); Set(view, "unavailableTag", unavailable); Set(view, "button", button);
            PrefabUtility.SaveAsPrefabAsset(root, KeystoneSlotPath); UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>施設、宿屋、ショップ、準備中を同時に確認できるPrefabを構築する。</summary>
        private static void BuildRoot()
        {
            GameObject root = Rect("OutpostUiRoot", null, Vector2.zero, Vector2.one);
            var rootView = root.AddComponent<OutpostRootView>();
            Image("Background", root.transform, Vector2.zero, Vector2.one, new Color(.08f, .12f, .10f, 1f), false);
            TMP_Text heading = Text("Heading", root.transform, "OUTPOST", 46, new Vector2(.04f, .70f), new Vector2(.96f, .76f)); heading.color = new Color(.92f, .78f, .48f, 1f);

            GameObject facilities = Panel("FacilitiesPanel", root.transform);
            Text("Title", facilities.transform, "どこへ向かう？", 31, new Vector2(.08f, .78f), new Vector2(.92f, .92f));
            Button innButton = Button("InnButton", facilities.transform, "宿屋", new Vector2(.08f, .51f), new Vector2(.45f, .72f));
            Button shopButton = Button("ShopButton", facilities.transform, "ショップ", new Vector2(.55f, .51f), new Vector2(.92f, .72f));
            Button workshopButton = Button("WorkshopButton", facilities.transform, "工房", new Vector2(.08f, .26f), new Vector2(.45f, .47f));
            Button questButton = Button("QuestButton", facilities.transform, "クエストカウンター", new Vector2(.55f, .26f), new Vector2(.92f, .47f));
            Button departButton = Button("DepartButton", facilities.transform, "次の遠征へ出発", new Vector2(.30f, .04f), new Vector2(.70f, .20f));

            GameObject inn = Panel("InnPanel", root.transform); var innView = inn.AddComponent<OutpostInnView>();
            Text("Title", inn.transform, "宿屋 ― 傷には薬、眠れぬ夜には請求書", 29, new Vector2(.06f, .80f), new Vector2(.94f, .94f));
            TMP_Text hp = Text("Hp", inn.transform, "HP  50 / 100  →  100 / 100", 28, new Vector2(.18f, .58f), new Vector2(.82f, .72f));
            TMP_Text cost = Text("Cost", inn.transform, "必要ゴールド  100 G", 24, new Vector2(.18f, .43f), new Vector2(.82f, .56f));
            TMP_Text reason = Text("Reason", inn.transform, "", 20, new Vector2(.18f, .31f), new Vector2(.82f, .42f));
            Button use = Button("UseButton", inn.transform, "休む", new Vector2(.35f, .16f), new Vector2(.65f, .29f));
            Button innBack = Button("BackButton", inn.transform, "戻る", new Vector2(.03f, .03f), new Vector2(.17f, .12f));
            Set(innView, "hpText", hp); Set(innView, "costText", cost); Set(innView, "reasonText", reason); Set(innView, "useButton", use);

            GameObject shop = Panel("ShopPanel", root.transform); var shopView = shop.AddComponent<OutpostShopView>();
            Text("Title", shop.transform, "ショップ ― 値札は正直、店主はどうかな", 27, new Vector2(.03f, .91f), new Vector2(.97f, .99f));
            Text("KeystoneLabel", shop.transform, "キーストーン", 21, new Vector2(.03f, .74f), new Vector2(.19f, .89f));
            var keySlots = new OutpostKeystoneProductSlotView[3];
            GameObject keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KeystoneSlotPath);
            for (int i = 0; i < keySlots.Length; i++)
            {
                GameObject slot = (GameObject)PrefabUtility.InstantiatePrefab(keyPrefab); slot.name = $"KeystoneSlot_{i + 1}"; slot.transform.SetParent(shop.transform, false);
                SetRect(slot, new Vector2(.20f + i * .255f, .72f), new Vector2(.44f + i * .255f, .89f)); keySlots[i] = slot.GetComponent<OutpostKeystoneProductSlotView>();
            }
            Text("CardLabel", shop.transform, "カード商品", 21, new Vector2(.03f, .56f), new Vector2(.14f, .68f));
            var cardSlots = new OutpostCardProductSlotView[10];
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardSlotPath);
            for (int i = 0; i < cardSlots.Length; i++)
            {
                int column = i % 5; int row = i / 5;
                GameObject slot = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab); slot.name = $"CardSlot_{i + 1:00}"; slot.transform.SetParent(shop.transform, false);
                float x = .03f + column * .193f; float y = row == 0 ? .32f : .07f;
                SetRect(slot, new Vector2(x, y), new Vector2(x + .176f, y + .22f)); cardSlots[i] = slot.GetComponent<OutpostCardProductSlotView>();
            }
            Button shopBack = Button("BackButton", shop.transform, "戻る", new Vector2(.82f, .01f), new Vector2(.97f, .065f));
            SetArray(shopView, "cardSlots", cardSlots); SetArray(shopView, "keystoneSlots", keySlots);

            GameObject preparing = Panel("PreparingPanel", root.transform); var preparingView = preparing.AddComponent<OutpostPreparingView>();
            TMP_Text facility = Text("Facility", preparing.transform, "工房", 34, new Vector2(.15f, .60f), new Vector2(.85f, .76f));
            TMP_Text message = Text("Message", preparing.transform, "準備中", 46, new Vector2(.15f, .38f), new Vector2(.85f, .58f));
            Button preparingBack = Button("BackButton", preparing.transform, "戻る", new Vector2(.03f, .03f), new Vector2(.17f, .12f));
            Set(preparingView, "facilityText", facility); Set(preparingView, "messageText", message);

            Set(rootView, "facilitiesPanel", facilities); Set(rootView, "innPanel", inn); Set(rootView, "shopPanel", shop); Set(rootView, "preparingPanel", preparing);
            Set(rootView, "innButton", innButton); Set(rootView, "shopButton", shopButton); Set(rootView, "workshopButton", workshopButton); Set(rootView, "questButton", questButton); Set(rootView, "departButton", departButton);
            SetArray(rootView, "backButtons", new[] { innBack, shopBack, preparingBack }); Set(rootView, "inn", innView); Set(rootView, "shop", shopView); Set(rootView, "preparing", preparingView);
            facilities.SetActive(true); inn.SetActive(false); shop.SetActive(false); preparing.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, RootPath); UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>Outpost SceneのCanvasへPrefabを一つだけ配置してController参照を設定する。</summary>
        public static void InstallIntoScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            OutpostSceneController controller = UnityEngine.Object.FindAnyObjectByType<OutpostSceneController>();
            if (canvas == null || controller == null) throw new InvalidOperationException("OutpostSceneにCanvasまたはControllerがありません。");
            Transform existing = canvas.transform.Find("OutpostUiRoot"); if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RootPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene); instance.name = "OutpostUiRoot"; instance.transform.SetParent(canvas.transform, false); instance.transform.SetAsFirstSibling();
            Set(controller, "rootView", instance.GetComponent<OutpostRootView>());
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("Outpost UIをSceneへ配置しました。");
        }

        private static GameObject Panel(string name, Transform parent) { GameObject panel = Image(name, parent, new Vector2(.04f, .02f), new Vector2(.96f, .69f), new Color(.76f, .69f, .56f, .97f), true); return panel; }
        private static GameObject Tag(string name, Transform parent, string label, Color color, Vector2 min, Vector2 max) { GameObject tag = Image(name, parent, min, max, color, false); TMP_Text text = Text("Text", tag.transform, label, 18, Vector2.zero, Vector2.one); text.color = Color.white; return tag; }
        private static Button Button(string name, Transform parent, string label, Vector2 min, Vector2 max) { GameObject go = Image(name, parent, min, max, new Color(.57f, .48f, .34f, 1f), true); Button button = go.AddComponent<Button>(); Text("Text", go.transform, label, 23, Vector2.zero, Vector2.one); return button; }
        private static TMP_Text Text(string name, Transform parent, string value, float size, Vector2 min, Vector2 max) { GameObject go = Rect(name, parent, min, max); var text = go.AddComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.alignment = TextAlignmentOptions.Center; text.color = Ink; text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false; return text; }
        private static GameObject Image(string name, Transform parent, Vector2 min, Vector2 max, Color color, bool raycast) { GameObject go = Rect(name, parent, min, max); var image = go.AddComponent<Image>(); image.color = color; image.raycastTarget = raycast; return go; }
        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max) { var go = new GameObject(name, typeof(RectTransform)); if (parent != null) go.transform.SetParent(parent, false); SetRect(go, min, max); return go; }
        private static void SetRect(GameObject go, Vector2 min, Vector2 max) { var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one; }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value) { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetArray<T>(UnityEngine.Object target, string field, T[] values) where T : UnityEngine.Object { var so = new SerializedObject(target); var property = so.FindProperty(field); property.arraySize = values.Length; for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void EnsureDirectory() { if (!AssetDatabase.IsValidFolder("Assets/GameData/Prefabs")) AssetDatabase.CreateFolder("Assets/GameData", "Prefabs"); if (!AssetDatabase.IsValidFolder(Directory)) AssetDatabase.CreateFolder("Assets/GameData/Prefabs", "OutpostUI"); }
    }
}
