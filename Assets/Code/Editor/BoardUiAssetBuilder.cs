using System;
using TestCardGame.BoardManage;
using TestCardGame.Controller;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN05の完成済みCell Prefabと盤面Viewportを事前構築する。</summary>
    public static class BoardUiAssetBuilder
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string CellPrefabPath = "Assets/GameData/Prefabs/Cell.prefab";

        [MenuItem("Tools/Test Card Game/UI/Build Board Fit And Targeting")]
        public static void BuildAll()
        {
            BuildCellPrefab();
            InstallBoardViewport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN05: 盤面自動フィットと対象範囲UIの事前構築が完了しました。");
        }

        /// <summary>バッチモードから構築と検証を連続実行する。</summary>
        public static void BuildAndValidateBatch()
        {
            BuildAll();
            BoardUiAssetValidator.Validate();
        }

        /// <summary>Cell Prefabへ永続表示と3種の独立枠を保存する。</summary>
        private static void BuildCellPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CellPrefabPath);
            try
            {
                DestroyChild(root.transform, "MoveCandidateBorder");
                DestroyChild(root.transform, "PlayerActionBorder");
                DestroyChild(root.transform, "EnemyIntentBorder");
                Image baseImage = root.GetComponent<Image>();
                if (baseImage == null) throw new InvalidOperationException("Cell.prefabに基礎Imageがありません。");
                CellView cellView = root.GetComponent<CellView>() ?? root.AddComponent<CellView>();
                Image move = CreateBorder("MoveCandidateBorder", root.transform, new Color(0.30f, 0.85f, 0.45f, 1f), 3f);
                Image player = CreateBorder("PlayerActionBorder", root.transform, new Color(0.90f, 0.12f, 0.10f, 1f), 5f);
                Image enemy = CreateBorder("EnemyIntentBorder", root.transform, new Color(0.65f, 0.04f, 0.04f, 1f), 3f);
                SetReference(cellView, "baseImage", baseImage);
                SetReference(cellView, "moveCandidateBorder", move);
                SetReference(cellView, "playerActionBorder", player);
                SetReference(cellView, "enemyIntentBorder", enemy);
                PrefabUtility.SaveAsPrefabAsset(root, CellPrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        /// <summary>BattleScene中央の予約領域へViewportと既存Board Contentを接続する。</summary>
        private static void InstallBoardViewport()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject board = FindSceneObject(scene, "Board") ?? throw new InvalidOperationException("BattleSceneにBoardがありません。");
            Canvas canvas = board.GetComponentInParent<Canvas>();
            if (canvas == null) throw new InvalidOperationException("BattleSceneのBoardに親Canvasがありません。");
            GameObject player = FindSceneObject(scene, "Player") ?? throw new InvalidOperationException("BattleSceneにPlayer Viewがありません。");
            Transform oldViewport = canvas.transform.Find("BoardViewport");
            if (oldViewport != null)
            {
                board.transform.SetParent(canvas.transform, false);
                player.transform.SetParent(canvas.transform, false);
                UnityEngine.Object.DestroyImmediate(oldViewport.gameObject);
            }

            var viewportObject = new GameObject("BoardViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(BoardView));
            viewportObject.layer = 5;
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(canvas.transform, false);
            viewport.anchorMin = new Vector2(0.02f, 0.29f);
            viewport.anchorMax = new Vector2(0.98f, 0.56f);
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.pivot = new Vector2(0.5f, 0.5f);
            Image background = viewportObject.GetComponent<Image>();
            background.color = new Color(0.055f, 0.05f, 0.065f, 0.97f);
            background.raycastTarget = false;

            RectTransform content = board.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.anchoredPosition = Vector2.zero;
            player.transform.SetParent(viewport, false);
            RectTransform playerRect = player.GetComponent<RectTransform>();
            playerRect.anchorMin = playerRect.anchorMax = new Vector2(0.5f, 0.5f);
            playerRect.anchoredPosition = Vector2.zero;
            player.transform.SetAsLastSibling();
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();
            if (grid == null) throw new InvalidOperationException("BattleSceneのBoardにGridLayoutGroupがありません。");
            BoardView boardView = viewportObject.GetComponent<BoardView>();
            SetReference(boardView, "viewport", viewport);
            SetReference(boardView, "content", content);
            SetReference(boardView, "grid", grid);
            SetReference(board.GetComponent<CellBuilder>(), "boardView", boardView);
            SetReference(FindSceneComponent<GameController>(scene), "boardView", boardView);
            viewportObject.transform.SetSiblingIndex(Mathf.Max(0, canvas.transform.childCount - 3));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Image CreateBorder(string name, Transform parent, Color color, float inset)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.layer = 5;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset; rect.offsetMax = -Vector2.one * inset;
            Image image = gameObject.GetComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            image.color = color;
            image.raycastTarget = false;
            gameObject.SetActive(false);
            return image;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == name) return child.gameObject;
            return null;
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            if (target == null) throw new InvalidOperationException($"{propertyName}の設定対象がありません。");
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName) ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName}がありません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
