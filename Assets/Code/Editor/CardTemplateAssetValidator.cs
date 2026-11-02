using System;
using TestCardGame.Cards.Views;
using UnityEditor;
using UnityEngine;

namespace TestCardGame.Editor
{
    public static class CardTemplateAssetValidator
    {
        [MenuItem("Tools/Test Card Game/UI/Validate Card Template")]
        public static void Validate()
        {
            foreach (string path in new[] { CardTemplateAssetBuilder.TemplatePath, CardTemplateAssetBuilder.HandPath })
            {
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null) throw new InvalidOperationException("Prefab missing: " + path);
                var views = root.GetComponentsInChildren<CardView>(true);
                int expected = path == CardTemplateAssetBuilder.HandPath ? HandView.RequiredSlotCount : 1;
                if (views.Length != expected) throw new InvalidOperationException("Unexpected card slot count: " + path);
                foreach (CardView view in views)
                {
                    var so = new SerializedObject(view);
                    foreach (string field in new[] { "nameText", "costText", "descriptionText", "levelText", "cooldownText", "artworkImage" })
                        if (so.FindProperty(field).objectReferenceValue == null)
                            throw new InvalidOperationException(view.name + ": Missing " + field);
                    if (!so.FindProperty("useTemplateLayout").boolValue)
                        throw new InvalidOperationException("Template mode not enabled: " + view.name);
                    var cost = (TMPro.TMP_Text)so.FindProperty("costText").objectReferenceValue;
                    var badge = cost.transform.parent.GetComponent<RectTransform>();
                    var circle = badge.GetComponent<UnityEngine.UI.Image>();
                    if (circle == null || circle.sprite == null || Mathf.Abs(badge.sizeDelta.x - badge.sizeDelta.y) > 0.01f)
                        throw new InvalidOperationException("Invalid circular cost badge: " + view.name);
                    var artwork = (UnityEngine.UI.Image)so.FindProperty("artworkImage").objectReferenceValue;
                    if (!artwork.preserveAspect) throw new InvalidOperationException("Artwork aspect not preserved");
                    foreach (var graphic in view.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                        if (graphic.gameObject != view.gameObject && graphic.raycastTarget)
                            throw new InvalidOperationException("Decoration intercepts input: " + graphic.name);
                    if (path == CardTemplateAssetBuilder.HandPath && view.GetComponent<CardTemplateBinding>() != null)
                        throw new InvalidOperationException("Standalone binder must not override hand data");
                    if (view.GetComponent<UnityEngine.UI.Image>().sprite != null)
                        throw new InvalidOperationException("Card base must use Unity panels, not generated texture");
                }
                Debug.Log("Card template validated: " + path + " (" + views.Length + " slots)");
            }
        }
    }
}
