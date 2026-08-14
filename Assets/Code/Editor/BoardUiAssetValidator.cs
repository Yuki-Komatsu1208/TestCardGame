using System;
using TestCardGame.BoardManage;
using TestCardGame.Controller;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCardGame.Editor
{
    /// <summary>PLAN05で必要なPrefab構造とScene参照を検証する。</summary>
    public static class BoardUiAssetValidator
    {
        [MenuItem("Tools/Test Card Game/UI/Validate Board Fit And Targeting")]
        public static void Validate()
        {
            ValidateLayouts();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/Cell.prefab");
            CellView cell = prefab != null ? prefab.GetComponent<CellView>() : null;
            if (cell == null || !cell.ValidateReferences()) throw new InvalidOperationException("Cell.prefabのCellViewまたはOverlay参照が不足しています。");
            Scene scene = EditorSceneManager.OpenScene("Assets/GameData/Scenes/BattleScene.unity", OpenSceneMode.Single);
            BoardView boardView = Find<BoardView>(scene);
            CellBuilder builder = Find<CellBuilder>(scene);
            GameController controller = Find<GameController>(scene);
            if (boardView == null || !boardView.ValidateReferences() || builder == null || controller == null)
                throw new InvalidOperationException("BattleSceneのBoardViewまたは必須Controller参照が不足しています。");
            Debug.Log("PLAN05 Validator: Cell Prefab、盤面Viewport、Controller参照を確認しました。");
        }

        /// <summary>既存5x5/6x6と縦横盤面が代表解像度のViewportへ収まることを確認する。</summary>
        private static void ValidateLayouts()
        {
            Vector2[] viewports = { new(1228f, 194f), new(1843f, 292f), new(2458f, 389f) };
            Vector2Int[] boards = { new(5, 5), new(6, 6), new(8, 4), new(4, 8) };
            var padding = new Vector4(8f, 8f, 8f, 8f);
            foreach (Vector2 viewport in viewports)
                foreach (Vector2Int board in boards)
                {
                    BoardLayoutResult result = BoardLayoutCalculator.Calculate(viewport, board, 5f, padding, 96f);
                    if (result.CellSize <= 0f || result.ContentSize.x > viewport.x + 0.01f || result.ContentSize.y > viewport.y + 0.01f)
                        throw new InvalidOperationException($"盤面{board}がViewport {viewport} に収まりません。");
                }
        }

        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }
    }
}
