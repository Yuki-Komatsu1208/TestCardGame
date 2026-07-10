using TestCardGame.Actions.Effects;
using UnityEditor;
using UnityEngine;

internal static class ActionEffectParameterEditorGUI
{
    /// <summary>
    /// Inspectorに表示する効果パラメータの行数を返す。
    /// </summary>
    public static int GetVisibleParameterCount(ActionEffectSO effect)
    {
        return Mathf.Max(1, effect?.ParameterFields.Length ?? 1);
    }

    /// <summary>
    /// PropertyDrawer用に効果パラメータを描画する。
    /// </summary>
    public static void DrawFields(Rect line, ActionEffectSO effect, SerializedProperty parameters)
    {
        if (effect == null)
        {
            EditorGUI.LabelField(line, "Parameters", "Select an effect.");
            return;
        }

        if (effect.ParameterFields.Length == 0)
        {
            EditorGUI.LabelField(line, "Parameters", "None");
            return;
        }

        foreach (string fieldName in effect.ParameterFields)
        {
            DrawField(ref line, parameters, fieldName);
        }
    }

    /// <summary>
    /// 通常のInspectorレイアウトで効果パラメータを描画する。
    /// </summary>
    public static void DrawFieldsLayout(ActionEffectSO effect, SerializedProperty parameters)
    {
        if (effect == null)
        {
            EditorGUILayout.LabelField("Parameters", "Select an effect.");
            return;
        }

        if (effect.ParameterFields.Length == 0)
        {
            EditorGUILayout.LabelField("Parameters", "None");
            return;
        }

        foreach (string fieldName in effect.ParameterFields)
        {
            var field = parameters.FindPropertyRelative(fieldName);
            if (field != null)
            {
                EditorGUILayout.PropertyField(field);
            }
            else
            {
                EditorGUILayout.HelpBox($"Unknown parameter field: {fieldName}", MessageType.Warning);
            }
        }
    }

    /// <summary>
    /// 効果アセットの初期値をSerializedPropertyへ反映する。
    /// </summary>
    public static void ApplyParameters(SerializedProperty parameters, ActionEffectParameters source)
    {
        parameters.FindPropertyRelative("step").intValue = source.step;
        parameters.FindPropertyRelative("damage").intValue = source.damage;
        parameters.FindPropertyRelative("duration").intValue = source.duration;
        parameters.FindPropertyRelative("range").intValue = source.range;
        parameters.FindPropertyRelative("hitType").enumValueIndex = (int)source.hitType;
        parameters.FindPropertyRelative("radius").intValue = source.radius;
        parameters.FindPropertyRelative("distance").intValue = source.distance;
        parameters.FindPropertyRelative("maxRange").intValue = source.maxRange;
        parameters.FindPropertyRelative("statusEffect").enumValueIndex = (int)source.statusEffect;
        parameters.FindPropertyRelative("value").intValue = source.value;
        parameters.FindPropertyRelative("count").intValue = source.count;
    }

    /// <summary>
    /// パラメータがUnity生成直後の初期値か判定する。
    /// </summary>
    public static bool IsInitialParameters(SerializedProperty parameters)
    {
        return parameters.FindPropertyRelative("step").intValue == 1
            && parameters.FindPropertyRelative("damage").intValue == 1
            && parameters.FindPropertyRelative("duration").intValue == 1
            && parameters.FindPropertyRelative("range").intValue == 1
            && parameters.FindPropertyRelative("hitType").enumValueIndex == 0
            && parameters.FindPropertyRelative("radius").intValue == 1
            && parameters.FindPropertyRelative("distance").intValue == 1
            && parameters.FindPropertyRelative("maxRange").intValue == 0
            && parameters.FindPropertyRelative("statusEffect").enumValueIndex == 0
            && parameters.FindPropertyRelative("value").intValue == 0
            && parameters.FindPropertyRelative("count").intValue == 1;
    }

    /// <summary>
    /// 指定名のパラメータを1行描画する。
    /// </summary>
    private static void DrawField(ref Rect line, SerializedProperty parameters, string fieldName)
    {
        var field = parameters.FindPropertyRelative(fieldName);
        if (field != null)
        {
            EditorGUI.PropertyField(line, field);
        }
        else
        {
            EditorGUI.LabelField(line, "Unknown parameter field", fieldName);
        }

        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}
