using System;
using TestCardGame.UIs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestCardGame.Editor
{
    /// <summary>Outpost Sceneの固定枠数、必須参照、初期表示を検証する。</summary>
    public static class OutpostUiAssetValidator
    {
        private const string ScenePath = "Assets/GameData/Scenes/OutpostScene.unity";
        [MenuItem("TestCardGame/UI/Outpost UIを検証")]
        public static void ValidateAll()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindAnyObjectByType<OutpostSceneController>();
            var root = UnityEngine.Object.FindAnyObjectByType<OutpostRootView>();
            var eventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
            if (controller == null || root == null || eventSystem == null) throw new InvalidOperationException("Outpost UIのScene参照が不足しています。");
            if (!controller.ValidateReferences() || !root.ValidateReferences()) throw new InvalidOperationException("Outpost UIの必須参照検証に失敗しました。");
            if (root.Shop.CardSlotCount != 10 || root.Shop.KeystoneSlotCount != 3) throw new InvalidOperationException("Outpostの商品枠数が不正です。");
            if (!root.transform.Find("FacilitiesPanel").gameObject.activeSelf || root.transform.Find("InnPanel").gameObject.activeSelf || root.transform.Find("ShopPanel").gameObject.activeSelf || root.transform.Find("PreparingPanel").gameObject.activeSelf)
                throw new InvalidOperationException("Outpostの初期パネル状態が不正です。");
            Debug.Log("Outpost UIの検証に成功しました。");
        }
    }
}
