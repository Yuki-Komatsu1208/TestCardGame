using System;
using System.IO;
using TMPro;
using TestCardGame.UIs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>共通UI PrefabとScene上の事前配置データを再構築するEditor専用ツール。</summary>
    public static class CommonUiAssetBuilder
    {
        private const string PrefabDirectory = "Assets/GameData/Prefabs/CommonUI";
        private const string CommonUiPrefabPath = PrefabDirectory + "/CommonUiRoot.prefab";
        private const string StyleGuidePrefabPath = PrefabDirectory + "/CommonUiStyleGuide.prefab";
        private const string OutpostScenePath = "Assets/GameData/Scenes/OutpostScene.unity";
        private const string BattleScenePath = "Assets/GameData/Scenes/BattleScene.unity";

        private static readonly Color Parchment = new(0.82f, 0.75f, 0.60f, 0.98f);
        private static readonly Color ParchmentDark = new(0.60f, 0.50f, 0.36f, 1f);
        private static readonly Color Ink = new(0.08f, 0.07f, 0.06f, 1f);
        private static readonly Color Iron = new(0.16f, 0.18f, 0.20f, 1f);
        private static readonly Color Amber = new(0.92f, 0.58f, 0.08f, 1f);

        [MenuItem("TestCardGame/UI/共通UIアセットを構築")]
        public static void BuildAll()
        {
            EnsureDirectory(PrefabDirectory);
            BuildCommonUiPrefab();
            BuildStyleGuidePrefab();
            InstallIntoScene(OutpostScenePath, true);
            InstallIntoScene(BattleScenePath, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("共通UIアセットとScene配置を構築しました。");
        }

        /// <summary>バッチ実行時に構築と検証を連続して行う。</summary>
        public static void BuildAndValidateBatch()
        {
            BuildAll();
            CommonUiAssetValidator.ValidateAllBatch();
        }

        /// <summary>上部ステータス、ログ、設定モーダルを持つ共通Prefabを生成する。</summary>
        private static void BuildCommonUiPrefab()
        {
            var root = CreateRectObject("CommonUiRoot", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var controller = root.AddComponent<CommonUiController>();

            var topFrame = CreateImage("TopFrame", root.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -246f), new Vector2(-12f, -8f), Iron, false);
            var statusPanel = CreateImage("StatusPanel", topFrame.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -94f), new Vector2(-112f, -12f), Parchment, false);
            var statusBar = statusPanel.AddComponent<CommonStatusBarView>();
            var labels = new TMP_Text[8];
            var values = new TMP_Text[8];
            for (int i = 0; i < labels.Length; i++)
            {
                float minX = i / (float)labels.Length;
                float maxX = (i + 1) / (float)labels.Length;
                var slot = CreateImage($"StatusSlot_{i + 1}", statusPanel.transform,
                    new Vector2(minX, 0f), new Vector2(maxX, 1f), new Vector2(3f, 5f), new Vector2(-3f, -5f),
                    i % 2 == 0 ? Parchment : new Color(0.76f, 0.68f, 0.53f, 1f), false);
                labels[i] = CreateText("Label", slot.transform, "項目", 18f, FontStyles.Bold,
                    TextAlignmentOptions.TopLeft, new Vector2(0f, 0.52f), Vector2.one, new Vector2(10f, 0f), new Vector2(-8f, -5f), Ink);
                values[i] = CreateText("Value", slot.transform, "--", 25f, FontStyles.Bold,
                    TextAlignmentOptions.BottomRight, Vector2.zero, new Vector2(1f, 0.58f), new Vector2(8f, 5f), new Vector2(-10f, 0f), Ink);
            }
            SetObjectArray(statusBar, "labels", labels);
            SetObjectArray(statusBar, "values", values);

            var messagePanel = CreateImage("MessagePanel", topFrame.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(12f, -226f), new Vector2(-12f, -106f), new Color(0.88f, 0.83f, 0.70f, 0.98f), false);
            var messageText = CreateText("MessageText", messagePanel.transform, "ここに施設の結果や戦闘ログが表示されます。", 21f,
                FontStyles.Normal, TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
                new Vector2(18f, 12f), new Vector2(-18f, -12f), Ink);
            var messageView = messagePanel.AddComponent<CommonMessageLogView>();
            SetObjectReference(messageView, "messageText", messageText);
            SetInteger(messageView, "maxVisibleMessages", 4);

            var settingsButton = CreateButton("SettingsButton", topFrame.transform, "⚙", 38f,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-100f, -92f), new Vector2(-18f, -12f));

            var modalRoot = CreateImage("SettingsModal", root.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.025f, 0.78f), true);
            var modalView = root.AddComponent<SettingsModalView>();
            var settingsController = root.AddComponent<SettingsController>();
            var window = CreateImage("Window", modalRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-430f, -430f), new Vector2(430f, 430f), Parchment, true);
            CreateText("Title", window.transform, "設定", 42f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 0.88f), Vector2.one, new Vector2(24f, 0f), new Vector2(-24f, -16f), Ink);
            Slider masterSlider = CreateVolumeRow(window.transform, "マスター音量", 0.75f, out TMP_Text masterValue);
            Slider bgmSlider = CreateVolumeRow(window.transform, "BGM音量", 0.63f, out TMP_Text bgmValue);
            Slider seSlider = CreateVolumeRow(window.transform, "SE音量", 0.51f, out TMP_Text seValue);
            CreateResolutionRow(window.transform, 0.37f, out Button previousResolution, out TMP_Text resolutionValue, out Button nextResolution);
            Toggle fullScreenToggle = CreateToggleRow(window.transform, "フルスクリーン", 0.25f);
            Toggle vSyncToggle = CreateToggleRow(window.transform, "VSync", 0.15f);
            var closeButton = CreateButton("CloseButton", window.transform, "閉じる", 24f,
                new Vector2(0.32f, 0.025f), new Vector2(0.68f, 0.105f), Vector2.zero, Vector2.zero);
            SetObjectReference(modalView, "modalRoot", modalRoot);
            SetObjectReference(modalView, "closeButton", closeButton);
            SetObjectReference(modalView, "masterVolumeSlider", masterSlider);
            SetObjectReference(modalView, "masterVolumeValue", masterValue);
            SetObjectReference(modalView, "bgmVolumeSlider", bgmSlider);
            SetObjectReference(modalView, "bgmVolumeValue", bgmValue);
            SetObjectReference(modalView, "seVolumeSlider", seSlider);
            SetObjectReference(modalView, "seVolumeValue", seValue);
            SetObjectReference(modalView, "previousResolutionButton", previousResolution);
            SetObjectReference(modalView, "resolutionValue", resolutionValue);
            SetObjectReference(modalView, "nextResolutionButton", nextResolution);
            SetObjectReference(modalView, "fullScreenToggle", fullScreenToggle);
            SetObjectReference(modalView, "vSyncToggle", vSyncToggle);
            SetObjectReference(settingsController, "view", modalView);

            SetObjectReference(controller, "statusBar", statusBar);
            SetObjectReference(controller, "messageLog", messageView);
            SetObjectReference(controller, "settingsButton", settingsButton);
            SetObjectReference(controller, "settingsModal", modalView);

            PrefabUtility.SaveAsPrefabAsset(root, CommonUiPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>共通配色と操作状態をPrefab Modeで比較できる見本を生成する。</summary>
        private static void BuildStyleGuidePrefab()
        {
            var root = CreateRectObject("CommonUiStyleGuide", null, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-550f, -325f), new Vector2(550f, 325f));
            CreateImage("Background", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Iron, false);
            CreateText("Title", root.transform, "COMMON UI STYLE GUIDE", 38f, FontStyles.Bold, TextAlignmentOptions.Left,
                new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.82f, 0.56f, 1f));
            CreateSwatch(root.transform, "通常パネル / 黒文字", Parchment, Ink, 0.64f);
            CreateSwatch(root.transform, "警告・致命的エラー / 琥珀", new Color(0.28f, 0.22f, 0.14f, 1f), Amber, 0.46f);
            CreateButton("NormalButton", root.transform, "通常ボタン", 24f,
                new Vector2(0.08f, 0.16f), new Vector2(0.31f, 0.31f), Vector2.zero, Vector2.zero);
            var disabled = CreateButton("DisabledButton", root.transform, "操作不能", 24f,
                new Vector2(0.38f, 0.16f), new Vector2(0.61f, 0.31f), Vector2.zero, Vector2.zero);
            disabled.interactable = false;
            var tag = CreateImage("Tag", root.transform, new Vector2(0.68f, 0.16f), new Vector2(0.91f, 0.31f),
                Vector2.zero, Vector2.zero, new Color(0.54f, 0.18f, 0.12f, 1f), false);
            CreateText("Text", tag.transform, "売り切れ札の見本", 23f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f), Color.white);
            PrefabUtility.SaveAsPrefabAsset(root, StyleGuidePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>既存Sceneへ共通Prefabを一つだけ配置しCanvas基準を統一する。</summary>
        private static void InstallIntoScene(string scenePath, bool ensureEventSystem)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Canvas canvas = FindSceneCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"{scenePath}: CanvasScaler を持つScene内Canvasがありません。");

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                throw new InvalidOperationException($"{scenePath}: CanvasScaler がありません。");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Transform existing = canvas.transform.Find("CommonUiRoot");
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CommonUiPrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(canvas.transform, false);
            instance.name = "CommonUiRoot";
            instance.transform.SetAsLastSibling();
            Transform modal = instance.transform.Find("SettingsModal");
            if (modal != null)
                modal.gameObject.SetActive(false);

            if (ensureEventSystem && UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            if (scenePath == OutpostScenePath)
            {
                var outpost = UnityEngine.Object.FindAnyObjectByType<OutpostSceneController>();
                if (outpost == null)
                    throw new InvalidOperationException("OutpostSceneController がありません。");
                SetObjectReference(outpost, "commonUi", instance.GetComponent<CommonUiController>());
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>Editor内部オブジェクトを除外し、対象Scene内の基準Canvasだけを取得する。</summary>
        private static Canvas FindSceneCanvas(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas candidate in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (candidate.GetComponent<CanvasScaler>() != null)
                        return candidate;
                }
            }

            return null;
        }

        /// <summary>値表示を伴う音量Slider行を事前構築する。</summary>
        private static Slider CreateVolumeRow(Transform parent, string title, float minY, out TMP_Text valueText)
        {
            var row = CreateImage(title, parent, new Vector2(0.08f, minY), new Vector2(0.92f, minY + 0.14f),
                Vector2.zero, Vector2.zero, new Color(0.72f, 0.64f, 0.49f, 1f), false);
            CreateText("Title", row.transform, title, 25f, FontStyles.Bold, TextAlignmentOptions.Left,
                new Vector2(0.03f, 0f), new Vector2(0.31f, 1f), Vector2.zero, Vector2.zero, Ink);
            Slider slider = CreateSlider("Slider", row.transform, new Vector2(0.32f, 0.25f), new Vector2(0.82f, 0.75f));
            valueText = CreateText("Value", row.transform, "100%", 23f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.83f, 0f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero, Ink);
            return slider;
        }

        /// <summary>前後ボタンと現在値を持つ解像度選択行を事前構築する。</summary>
        private static void CreateResolutionRow(Transform parent, float minY, out Button previous, out TMP_Text value, out Button next)
        {
            var row = CreateImage("Resolution", parent, new Vector2(0.08f, minY), new Vector2(0.92f, minY + 0.12f),
                Vector2.zero, Vector2.zero, new Color(0.72f, 0.64f, 0.49f, 1f), false);
            CreateText("Title", row.transform, "解像度", 25f, FontStyles.Bold, TextAlignmentOptions.Left,
                new Vector2(0.03f, 0f), new Vector2(0.28f, 1f), Vector2.zero, Vector2.zero, Ink);
            previous = CreateButton("Previous", row.transform, "◀", 24f,
                new Vector2(0.30f, 0.16f), new Vector2(0.40f, 0.84f), Vector2.zero, Vector2.zero);
            value = CreateText("Value", row.transform, "1920 × 1080", 23f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.41f, 0f), new Vector2(0.83f, 1f), Vector2.zero, Vector2.zero, Ink);
            next = CreateButton("Next", row.transform, "▶", 24f,
                new Vector2(0.84f, 0.16f), new Vector2(0.94f, 0.84f), Vector2.zero, Vector2.zero);
        }

        /// <summary>ラベル付きToggle行を事前構築する。</summary>
        private static Toggle CreateToggleRow(Transform parent, string title, float minY)
        {
            var row = CreateImage(title, parent, new Vector2(0.08f, minY), new Vector2(0.92f, minY + 0.09f),
                Vector2.zero, Vector2.zero, new Color(0.72f, 0.64f, 0.49f, 1f), false);
            CreateText("Title", row.transform, title, 24f, FontStyles.Bold, TextAlignmentOptions.Left,
                new Vector2(0.03f, 0f), new Vector2(0.80f, 1f), Vector2.zero, Vector2.zero, Ink);
            GameObject background = CreateImage("Toggle", row.transform, new Vector2(0.86f, 0.18f), new Vector2(0.94f, 0.82f),
                Vector2.zero, Vector2.zero, ParchmentDark, true);
            var toggle = background.AddComponent<Toggle>();
            GameObject checkmark = CreateImage("Checkmark", background.transform, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f),
                Vector2.zero, Vector2.zero, Amber, false);
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            return toggle;
        }

        /// <summary>背景、Fill、Handleをすべて事前配置したSliderを生成する。</summary>
        private static Slider CreateSlider(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var slider = root.AddComponent<Slider>();
            GameObject background = CreateImage("Background", root.transform, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f),
                Vector2.zero, Vector2.zero, new Color(0.28f, 0.24f, 0.19f, 1f), false);
            GameObject fillArea = CreateRectObject("Fill Area", root.transform, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f),
                new Vector2(8f, 0f), new Vector2(-8f, 0f));
            GameObject fill = CreateImage("Fill", fillArea.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, Amber, false);
            GameObject handleArea = CreateRectObject("Handle Slide Area", root.transform, Vector2.zero, Vector2.one,
                new Vector2(8f, 0f), new Vector2(-8f, 0f));
            GameObject handle = CreateImage("Handle", handleArea.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-10f, -18f), new Vector2(10f, 18f), ParchmentDark, true);
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private static void CreateSwatch(Transform parent, string text, Color background, Color foreground, float minY)
        {
            var panel = CreateImage(text, parent, new Vector2(0.08f, minY), new Vector2(0.92f, minY + 0.13f),
                Vector2.zero, Vector2.zero, background, false);
            CreateText("Text", panel.transform, text, 25f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(10f, 5f), new Vector2(-10f, -5f), foreground);
        }

        private static GameObject CreateRectObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            if (parent != null)
                gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return gameObject;
        }

        private static GameObject CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color, bool raycastTarget)
        {
            GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return gameObject;
        }

        private static Button CreateButton(string name, Transform parent, string text, float fontSize, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject gameObject = CreateImage(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, ParchmentDark, true);
            var button = gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.88f, 0.58f, 1f);
            colors.pressedColor = new Color(0.72f, 0.60f, 0.42f, 1f);
            colors.disabledColor = new Color(0.40f, 0.40f, 0.40f, 0.65f);
            button.colors = colors;
            CreateText("Text", gameObject.transform, text, fontSize, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(6f, 3f), new Vector2(-6f, -3f), Ink);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, FontStyles style,
            TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var label = gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.name}: {propertyName} が見つかりません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.name}: {propertyName} が見つかりません。");
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInteger(UnityEngine.Object target, string propertyName, int value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.name}: {propertyName} が見つかりません。");
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string assetDirectory)
        {
            string[] parts = assetDirectory.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
