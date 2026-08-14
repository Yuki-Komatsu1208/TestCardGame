using System;
using System.IO;
using TMPro;
using TestCardGame.UIs;
using TestCardGame.UIs.Cards;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN03専用の一覧スロットと共通カード一覧Prefabを事前構築する。</summary>
    public static class CardListUiAssetBuilder
    {
        public const string PrefabDirectory = "Assets/GameData/Prefabs/CardList";
        public const string SlotPrefabPath = PrefabDirectory + "/CardListSlot.prefab";
        public const string OverlayPrefabPath = PrefabDirectory + "/CardListSelectionOverlay.prefab";

        private static readonly Color Ink = new(0.08f, 0.07f, 0.06f, 1f);
        private static readonly Color Iron = new(0.13f, 0.14f, 0.16f, 0.98f);
        private static readonly Color Parchment = new(0.82f, 0.75f, 0.60f, 1f);
        private static readonly Color ParchmentDark = new(0.55f, 0.45f, 0.31f, 1f);
        private static readonly Color Amber = new(0.92f, 0.58f, 0.08f, 1f);

        [MenuItem("TestCardGame/UI/PLAN03 カード一覧Prefabを構築")]
        public static void BuildAll()
        {
            EnsureDirectory(PrefabDirectory);
            BuildSlotPrefab();
            BuildOverlayPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN03 カード一覧Prefabを構築しました。Sceneへの配置は行っていません。");
        }

        public static void BuildAndValidateBatch()
        {
            BuildAll();
            CardListUiAssetValidator.ValidateAllBatch();
        }

        private static void BuildSlotPrefab()
        {
            GameObject root = CreateImage("CardListSlot", null, Parchment, true);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            var view = root.AddComponent<CardListSlotView>();

            TMP_Text name = CreateText("CardName", root.transform, "カード名", 20f, FontStyles.Bold,
                TextAlignmentOptions.TopLeft, new Vector2(0.06f, 0.66f), new Vector2(0.94f, 0.96f), Ink);
            TMP_Text level = CreateText("Level", root.transform, "Lv.1", 17f, FontStyles.Bold,
                TextAlignmentOptions.BottomLeft, new Vector2(0.06f, 0.05f), new Vector2(0.48f, 0.23f), Ink);
            TMP_Text cost = CreateText("Cost", root.transform, "COST 1", 17f, FontStyles.Bold,
                TextAlignmentOptions.BottomRight, new Vector2(0.50f, 0.05f), new Vector2(0.94f, 0.23f), Ink);
            TMP_Text reason = CreateText("DisabledReason", root.transform, "対象外", 15f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.04f, 0.29f), new Vector2(0.96f, 0.62f), Color.white);
            GameObject selection = CreateBorder("SelectionMark", root.transform, new Color(0.85f, 0.16f, 0.12f, 0.92f), 7f);
            GameObject focus = CreateBorder("FocusMark", root.transform, Amber, 3f);

            SetReference(view, "button", button);
            SetReference(view, "background", root.GetComponent<Image>());
            SetReference(view, "nameText", name);
            SetReference(view, "levelText", level);
            SetReference(view, "costText", cost);
            SetReference(view, "disabledReasonText", reason);
            SetReference(view, "selectionMark", selection);
            SetReference(view, "focusMark", focus);
            reason.gameObject.SetActive(false);
            selection.SetActive(false);
            focus.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, SlotPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void BuildOverlayPrefab()
        {
            GameObject root = CreateRect("CardListSelectionOverlay", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var overlay = root.AddComponent<CardListSelectionOverlay>();
            GameObject displayRoot = CreateRect("DisplayRoot", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateImage("Blocker", displayRoot.transform, new Color(0.01f, 0.01f, 0.015f, 0.82f), true);
            GameObject container = CreateImage("Container", displayRoot.transform, Iron, true,
                new Vector2(0.025f, 0.04f), new Vector2(0.975f, 0.96f));

            GameObject header = CreateImage("Header", container.transform, ParchmentDark, false,
                new Vector2(0.015f, 0.90f), new Vector2(0.985f, 0.985f));
            TMP_Text title = CreateText("TitleText", header.transform, "カード一覧", 36f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.025f, 0.08f), new Vector2(0.82f, 0.92f), Color.white);
            Button close = CreateButton("CloseButton", header.transform, "閉じる",
                new Vector2(0.84f, 0.13f), new Vector2(0.98f, 0.87f));

            GameObject body = CreateRect("Body", container.transform, new Vector2(0.015f, 0.14f), new Vector2(0.985f, 0.89f), Vector2.zero, Vector2.zero);
            GameObject scrollRoot = CreateImage("CardScrollView", body.transform, new Color(0.08f, 0.09f, 0.10f, 1f), true,
                Vector2.zero, new Vector2(0.69f, 1f));
            var scroll = scrollRoot.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            GameObject viewport = CreateImage("Viewport", scrollRoot.transform, new Color(0f, 0f, 0f, 0.01f), true,
                new Vector2(0.015f, 0.015f), new Vector2(0.985f, 0.985f));
            var mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            GameObject content = CreateRect("GridContent", viewport.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(0f, 2130f));
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.pivot = new Vector2(0.5f, 1f);
            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(158f, 220f);
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
            if (slotPrefab == null)
                throw new InvalidOperationException("CardListSlot Prefabを読み込めません。");
            var slots = new CardListSlotView[CardSelectionSession.MaximumCardCount];
            for (int i = 0; i < slots.Length; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
                instance.name = $"CardListSlot_{i:00}";
                instance.transform.SetParent(content.transform, false);
                slots[i] = instance.GetComponent<CardListSlotView>();
            }

            GameObject detail = CreateImage("CardDetailPanel", body.transform, Parchment, true,
                new Vector2(0.705f, 0f), Vector2.one);
            var detailView = detail.AddComponent<CardDetailPanelView>();
            TMP_Text detailName = CreateText("CardName", detail.transform, "カードを選択してください", 30f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.87f), new Vector2(0.94f, 0.97f), Ink);
            TMP_Text detailLevel = CreateText("Level", detail.transform, "レベル 1", 21f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.79f), new Vector2(0.48f, 0.86f), Ink);
            TMP_Text detailCost = CreateText("Cost", detail.transform, "コスト 1", 21f, FontStyles.Bold,
                TextAlignmentOptions.Right, new Vector2(0.50f, 0.79f), new Vector2(0.92f, 0.86f), Ink);
            TMP_Text detailCooldown = CreateText("Cooldown", detail.transform, "クールダウン 0", 19f, FontStyles.Normal,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.79f), Ink);
            CreateText("EffectLabel", detail.transform, "効果", 20f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.71f), Ink);
            TMP_Text effect = CreateText("EffectText", detail.transform, "効果説明", 18f, FontStyles.Normal,
                TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.64f), Ink);
            CreateText("ModifierLabel", detail.transform, "MOD", 20f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.36f), Ink);
            TMP_Text modifiers = CreateText("ModifierListText", detail.transform, "なし", 18f, FontStyles.Normal,
                TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.09f), new Vector2(0.92f, 0.29f), Ink);
            TMP_Text disabled = CreateText("DisabledReason", detail.transform, "このカードは対象に選べません。", 17f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.01f), new Vector2(0.94f, 0.08f), new Color(0.58f, 0.14f, 0.08f, 1f));
            disabled.gameObject.SetActive(false);
            SetReference(detailView, "nameText", detailName);
            SetReference(detailView, "levelText", detailLevel);
            SetReference(detailView, "costText", detailCost);
            SetReference(detailView, "cooldownText", detailCooldown);
            SetReference(detailView, "effectText", effect);
            SetReference(detailView, "modifierListText", modifiers);
            SetReference(detailView, "disabledReasonText", disabled);

            GameObject footer = CreateImage("Footer", container.transform, ParchmentDark, false,
                new Vector2(0.015f, 0.015f), new Vector2(0.985f, 0.125f));
            TMP_Text count = CreateText("SelectionCountText", footer.transform, "0 / 3", 28f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.03f, 0.10f), new Vector2(0.45f, 0.90f), Color.white);
            Button cancel = CreateButton("CancelButton", footer.transform, "キャンセル",
                new Vector2(0.62f, 0.15f), new Vector2(0.79f, 0.85f));
            Button ok = CreateButton("OkButton", footer.transform, "OK",
                new Vector2(0.81f, 0.15f), new Vector2(0.98f, 0.85f));

            CardEnhancementConfirmationView modal = BuildConfirmationModal(displayRoot.transform);

            SetReference(overlay, "containerPanel", displayRoot);
            SetReference(overlay, "titleText", title);
            SetReference(overlay, "closeButton", close);
            SetReference(overlay, "scrollRect", scroll);
            SetArray(overlay, "slots", slots);
            SetReference(overlay, "detailPanel", detailView);
            SetReference(overlay, "footerRoot", footer);
            SetReference(overlay, "selectionCountText", count);
            SetReference(overlay, "cancelButton", cancel);
            SetReference(overlay, "okButton", ok);
            SetReference(overlay, "enhancementModal", modal);

            PrefabUtility.SaveAsPrefabAsset(root, OverlayPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static CardEnhancementConfirmationView BuildConfirmationModal(Transform parent)
        {
            GameObject blocker = CreateImage("EnhancementConfirmation", parent, new Color(0.01f, 0.01f, 0.015f, 0.88f), true);
            var view = blocker.AddComponent<CardEnhancementConfirmationView>();
            GameObject window = CreateImage("Window", blocker.transform, Parchment, true,
                new Vector2(0.28f, 0.25f), new Vector2(0.72f, 0.75f));
            TMP_Text title = CreateText("Title", window.transform, "強化します", 38f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f), Ink);
            TMP_Text target = CreateText("Target", window.transform, "対象: カード名", 24f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.75f), Ink);
            TMP_Text enhancement = CreateText("Enhancement", window.transform, "強化内容", 22f, FontStyles.Normal,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.57f), Ink);
            TMP_Text error = CreateText("Error", window.transform, "強化を適用できませんでした。", 18f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.34f), new Color(0.72f, 0.15f, 0.08f, 1f));
            Button no = CreateButton("NoButton", window.transform, "NO", new Vector2(0.12f, 0.07f), new Vector2(0.43f, 0.22f));
            Button ok = CreateButton("OkButton", window.transform, "OK", new Vector2(0.57f, 0.07f), new Vector2(0.88f, 0.22f));
            error.gameObject.SetActive(false);
            SetReference(view, "modalRoot", blocker);
            SetReference(view, "titleText", title);
            SetReference(view, "targetText", target);
            SetReference(view, "enhancementText", enhancement);
            SetReference(view, "errorText", error);
            SetReference(view, "okButton", ok);
            SetReference(view, "noButton", no);
            return view;
        }

        private static GameObject CreateBorder(string name, Transform parent, Color color, float thickness)
        {
            GameObject root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateImage("Top", root.transform, color, false, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -thickness), Vector2.zero);
            CreateImage("Bottom", root.transform, color, false, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, thickness));
            CreateImage("Left", root.transform, color, false, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, 0f));
            CreateImage("Right", root.transform, color, false, new Vector2(1f, 0f), Vector2.one, new Vector2(-thickness, 0f), Vector2.zero);
            return root;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateImage(name, parent, new Color(0.30f, 0.24f, 0.17f, 1f), true, anchorMin, anchorMax);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            CreateText("Label", root.transform, label, 22f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Color.white);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float size, FontStyles style,
            TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject root = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var label = root.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        private static GameObject CreateImage(string name, Transform parent, Color color, bool raycast,
            Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            GameObject root = CreateRect(name, parent, anchorMin ?? Vector2.zero, anchorMax ?? Vector2.one,
                offsetMin ?? Vector2.zero, offsetMax ?? Vector2.zero);
            var image = root.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return root;
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var root = new GameObject(name, typeof(RectTransform));
            if (parent != null) root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return root;
        }

        private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray<T>(UnityEngine.Object target, string propertyName, T[] values) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string directory)
        {
            string current = "Assets";
            foreach (string segment in directory.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + segment;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segment);
                current = next;
            }
        }
    }
}
