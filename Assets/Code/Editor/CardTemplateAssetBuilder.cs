using System;
using TMPro;
using TestCardGame.Cards.Views;
using UnityEditor;
using UnityEngine;

namespace TestCardGame.Editor
{
    /// <summary>共有カードレイアウトを既存CardViewへ適用。入力Componentやルートは置換しない。</summary>
    public static class CardTemplateAssetBuilder
    {
        public const string TemplatePath = "Assets/GameData/Prefabs/UI/Cards/CardTemplate.prefab";
        public const string BaseImagePath = "Assets/GameData/UI/Cards/CardTemplateBase.png";
        public const string HandPath = "Assets/GameData/Prefabs/UI/Battle/BattleHand.prefab";
        private const string CirclePath = "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Circle.png";
        private const string FontPath = "Assets/UI/UnityResource/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private static readonly Color Ink = new Color(0.12f, 0.10f, 0.08f, 1f);
        private static readonly Color Parchment = new Color(0.82f, 0.75f, 0.58f, 1f);
        private static readonly Color Iron = new Color(0.13f, 0.12f, 0.13f, 1f);

        [MenuItem("Tools/Test Card Game/UI/Build Card Template")]
        public static void BuildTemplate()
        {
            EnsureDirectory("Assets/GameData/Prefabs/UI/Cards");
            bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath) != null;
            GameObject root = exists ? PrefabUtility.LoadPrefabContents(TemplatePath)
                : new GameObject("CardTemplate", typeof(RectTransform));
            try
            {
                root.SetActive(false);
                if (!exists) root.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 210f);
                CardView view = root.GetComponent<CardView>();
                if (view == null) view = root.AddComponent<CardView>();
                ApplyLayout(view);
                var binding = root.GetComponent<CardTemplateBinding>();
                if (binding == null) binding = root.AddComponent<CardTemplateBinding>();
                SetReference(binding, "cardView", view);
                root.SetActive(true);
                PrefabUtility.SaveAsPrefabAsset(root, TemplatePath);
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(root);
                else UnityEngine.Object.DestroyImmediate(root);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Test Card Game/UI/Apply Card Template To Battle Hand")]
        public static void ApplyToHand()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HandPath);
            try
            {
                var views = root.GetComponentsInChildren<CardView>(true);
                if (views.Length != HandView.RequiredSlotCount)
                    throw new InvalidOperationException("BattleHandのカード数が50枠ではありません。");
                foreach (var view in views) ApplyLayout(view);
                PrefabUtility.SaveAsPrefabAsset(root, HandPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        /// <summary>事前配置・再生成の両方から使う。既存の文字参照を維持する。</summary>
        public static void ApplyLayout(CardView view)
        {
            var root = view.gameObject;
            var rootImage = root.GetComponent<UnityEngine.UI.Image>();
            if (rootImage == null) rootImage = root.AddComponent<UnityEngine.UI.Image>();
            rootImage.sprite = null;
            rootImage.type = UnityEngine.UI.Image.Type.Simple;
            rootImage.color = Iron;
            // Standalone previews are decorative; hand roots retain their input surface.
            rootImage.raycastTarget = root.GetComponent<TestCardGame.UIs.Battle.BattleHandCardSlotView>() != null;

            var bronze = new Color(0.54f, 0.43f, 0.26f, 1f);
            var trim = Image(root.transform, "FrameTrim", Vector2.zero, Vector2.one, bronze);
            trim.rectTransform.offsetMin = new Vector2(2f, 2f);
            trim.rectTransform.offsetMax = new Vector2(-2f, -2f);
            trim.transform.SetAsFirstSibling();
            var paper = Image(trim.transform, "Parchment", Vector2.zero, Vector2.one, Parchment);
            paper.rectTransform.offsetMin = new Vector2(2f, 2f);
            paper.rectTransform.offsetMax = new Vector2(-2f, -2f);
            var header = Image(root.transform, "TitlePanel", new Vector2(0.05f, 0.815f), new Vector2(0.95f, 0.97f), new Color(0.73f, 0.64f, 0.46f, 1f));
            var artworkFrame = Image(root.transform, "ArtworkFrame", new Vector2(0.06f, 0.43f), new Vector2(0.94f, 0.79f), bronze);
            var well = Image(artworkFrame.transform, "ImageWell", Vector2.zero, Vector2.one, new Color(0.19f, 0.18f, 0.17f, 1f));
            well.rectTransform.offsetMin = new Vector2(2f, 2f);
            well.rectTransform.offsetMax = new Vector2(-2f, -2f);
            well.transform.SetAsFirstSibling();
            var explanation = Image(root.transform, "DescriptionPanel", new Vector2(0.06f, 0.10f), new Vector2(0.94f, 0.41f), bronze);
            var explanationPaper = Image(explanation.transform, "Paper", Vector2.zero, Vector2.one, new Color(0.88f, 0.82f, 0.67f, 1f));
            explanationPaper.rectTransform.offsetMin = new Vector2(1f, 1f);
            explanationPaper.rectTransform.offsetMax = new Vector2(-1f, -1f);
            header.transform.SetSiblingIndex(1);
            explanation.transform.SetSiblingIndex(2);
            artworkFrame.transform.SetSiblingIndex(3);
            var illustration = Image(artworkFrame.transform, "Artwork", Vector2.zero, Vector2.one, Color.white);
            illustration.rectTransform.offsetMin = new Vector2(1.5f, 1.5f);
            illustration.rectTransform.offsetMax = new Vector2(-1.5f, -1.5f);
            illustration.preserveAspect = true;
            illustration.enabled = illustration.sprite != null;

            var title = ExistingText(view, "nameText", "Name");
            var cost = ExistingText(view, "costText", "Cost");
            var description = ExistingText(view, "descriptionText", "Description");
            FormatText(title, new Vector2(0.29f, 0.87f), new Vector2(0.93f, 0.965f), 13f,
                TextAlignmentOptions.Center, FontStyles.Bold, Ink);
            FormatText(description, new Vector2(0.09f, 0.14f), new Vector2(0.91f, 0.395f), 11.5f,
                TextAlignmentOptions.TopLeft, FontStyles.Normal, Ink);
            var level = ExistingText(view, "levelText", "Level");
            FormatText(level, new Vector2(0.49f, 0.805f), new Vector2(0.91f, 0.86f), 8.5f,
                TextAlignmentOptions.Right, FontStyles.Normal, Ink);
            var cooldown = ExistingText(view, "cooldownText", "Cooldown");
            FormatText(cooldown, new Vector2(0.49f, 0.055f), new Vector2(0.90f, 0.105f), 9f,
                TextAlignmentOptions.Right, FontStyles.Bold, Ink);

            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath)
                ?? throw new InvalidOperationException("コスト用のCircle Spriteがありません。");
            var rim = Image(root.transform, "CostBadge", Vector2.zero, Vector2.zero, Iron);
            rim.sprite = circle;
            rim.preserveAspect = true;
            rim.rectTransform.anchorMin = rim.rectTransform.anchorMax = new Vector2(0.04f, 0.96f);
            rim.rectTransform.pivot = new Vector2(0f, 1f);
            rim.rectTransform.anchoredPosition = Vector2.zero;
            rim.rectTransform.sizeDelta = new Vector2(32f, 32f);
            var center = Image(rim.transform, "Center", Vector2.zero, Vector2.one, new Color(0.63f, 0.18f, 0.12f, 1f));
            center.sprite = circle;
            center.rectTransform.offsetMin = new Vector2(2f, 2f);
            center.rectTransform.offsetMax = new Vector2(-2f, -2f);
            cost.transform.SetParent(rim.transform, false);
            FormatText(cost, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), 17f,
                TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            title.transform.SetAsLastSibling();
            description.transform.SetAsLastSibling();
            level.transform.SetAsLastSibling();
            cooldown.transform.SetAsLastSibling();
            rim.transform.SetAsLastSibling();
            cost.transform.SetAsLastSibling();

            SetReference(view, "nameText", title);
            SetReference(view, "costText", cost);
            SetReference(view, "descriptionText", description);
            SetReference(view, "artworkImage", illustration);
            SetReference(view, "levelText", level);
            SetReference(view, "cooldownText", cooldown);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("useTemplateLayout").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMPro.TextMeshProUGUI ExistingText(CardView view, string field, string fallbackName)
        {
            var so = new SerializedObject(view);
            var existing = so.FindProperty(field).objectReferenceValue as TMPro.TextMeshProUGUI;
            if (existing != null) return existing;
            return Child(view.transform, fallbackName).GetComponent<TMPro.TextMeshProUGUI>()
                ?? Child(view.transform, fallbackName).gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        }

        private static void FormatText(TMPro.TextMeshProUGUI text, Vector2 min, Vector2 max, float size,
            TextAlignmentOptions alignment, FontStyles style, Color color)
        {
            Stretch(text.rectTransform, min, max);
            // Keep the project's Latin font + Japanese fallback stack.
            if (text.font == null) text.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FontPath);
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.margin = Vector4.zero;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
        }

        private static UnityEngine.UI.Image Image(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var rect = Child(parent, name);
            Stretch(rect, min, max);
            var image = rect.GetComponent<UnityEngine.UI.Image>();
            if (image == null) image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Child(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<RectTransform>()
                ?? throw new InvalidOperationException(name + " has no RectTransform");
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string path)
        {
            var parts = path.Split('/'); string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(current + "/" + parts[i])) AssetDatabase.CreateFolder(current, parts[i]);
                current += "/" + parts[i];
            }
        }
    }
}
