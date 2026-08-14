using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using TestCardGame.UIs;
using TestCardGame.UIs.Cards;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN03 Prefabの50枠、6列、必須参照、禁止コンポーネントを検証する。</summary>
    public static class CardListUiAssetValidator
    {
        [MenuItem("TestCardGame/UI/PLAN03 カード一覧Prefabを検証")]
        public static void ValidateAll()
        {
            var errors = CollectErrors();
            if (errors.Count == 0)
            {
                Debug.Log("PLAN03 カード一覧Prefabの検証に成功しました。");
                return;
            }
            foreach (string error in errors) Debug.LogError(error);
        }

        public static void ValidateAllBatch()
        {
            var errors = CollectErrors();
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            Debug.Log("PLAN03 カード一覧Prefabのバッチ検証に成功しました。");
        }

        private static List<string> CollectErrors()
        {
            var errors = new List<string>();
            ValidateSelectionRules(errors);
            GameObject root = PrefabUtility.LoadPrefabContents(CardListUiAssetBuilder.OverlayPrefabPath);
            if (root == null)
            {
                errors.Add("CardListSelectionOverlay Prefabがありません。");
                return errors;
            }

            try
            {
                var overlay = root.GetComponent<CardListSelectionOverlay>();
                if (overlay == null)
                {
                    errors.Add("CardListSelectionOverlayコンポーネントがありません。");
                    return errors;
                }
                var serialized = new SerializedObject(overlay);
                SerializedProperty slots = serialized.FindProperty("slots");
                if (slots == null || slots.arraySize != CardSelectionSession.MaximumCardCount)
                    errors.Add("一覧スロット参照は50件必要です。");
                else
                {
                    var unique = new HashSet<UnityEngine.Object>();
                    for (int i = 0; i < slots.arraySize; i++)
                    {
                        UnityEngine.Object value = slots.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (value == null) errors.Add($"一覧スロット{i}の参照がありません。");
                        else if (!unique.Add(value)) errors.Add($"一覧スロット{i}の参照が重複しています。");
                    }
                }

                RequireReference(serialized, "detailPanel", errors);
                RequireReference(serialized, "selectionCountText", errors);
                RequireReference(serialized, "okButton", errors);
                RequireReference(serialized, "cancelButton", errors);
                RequireReference(serialized, "enhancementModal", errors);

                Transform content = root.transform.Find("DisplayRoot/Container/Body/CardScrollView/Viewport/GridContent");
                var grid = content != null ? content.GetComponent<GridLayoutGroup>() : null;
                if (grid == null || grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount || grid.constraintCount != 6)
                    errors.Add("GridContentは6列固定のGridLayoutGroupである必要があります。");
                if (content == null || content.childCount != CardSelectionSession.MaximumCardCount)
                    errors.Add("GridContent直下には50個のスロットが必要です。");
                if (root.GetComponentsInChildren<MoveCardDragHandler>(true).Length > 0)
                    errors.Add("一覧用Prefabへ戦闘用ドラッグ処理を付けることはできません。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return errors;
        }

        /// <summary>min/max、重複実体、トグル、確定前変更の主要境界をEditor上で検証する。</summary>
        private static void ValidateSelectionRules(List<string> errors)
        {
            var definition = ScriptableObject.CreateInstance<CardDefinitionSO>();
            definition.cardName = "検証カード";
            definition.level1 = new CardLevelData();
            definition.level2 = new CardLevelData();
            definition.level3 = new CardLevelData();
            try
            {
                var first = new CardBase(definition, new CardLevel(1));
                var second = new CardBase(definition, new CardLevel(1));
                var cards = new List<CardBase> { first, second };
                var request = new CardSelectionRequest("検証", cards, _ => true, 1, 2, true);
                if (!CardSelectionSession.TryCreate(request, out CardSelectionSession session, out _))
                {
                    errors.Add("正常なカード選択要求を開始できません。");
                    return;
                }
                if (!session.TryToggle(new CardSelectionToken(0), out _) || session.SelectedCount != 1)
                    errors.Add("カードのトグル選択に失敗しました。");
                if (!session.TryToggle(new CardSelectionToken(0), out _) || session.SelectedCount != 0)
                    errors.Add("選択済みカードの解除に失敗しました。");

                var duplicateRequest = new CardSelectionRequest("重複", new List<CardBase> { first, first },
                    _ => true, 2, 2, true);
                if (CardSelectionSession.Validate(duplicateRequest).IsValid)
                    errors.Add("同一カード実体を候補2枚として数えています。");

                session.TryToggle(new CardSelectionToken(0), out _);
                cards[0] = second;
                if (session.TryBuildResult(out _, out _))
                    errors.Add("確定前に差し替えられたカードを受理しています。");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        private static void RequireReference(SerializedObject serialized, string propertyName, List<string> errors)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                errors.Add($"{propertyName} の参照がありません。");
        }
    }
}
