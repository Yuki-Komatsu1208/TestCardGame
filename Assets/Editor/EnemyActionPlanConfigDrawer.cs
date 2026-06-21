using TestCardGame.Character.Enemies;
using TestCardGame.Actions.Effects;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyActionPlanConfig))]
public class EnemyActionPlanConfigDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var effect = property.FindPropertyRelative("effect").objectReferenceValue as ActionEffectSO;
        var useCustomParameters = property.FindPropertyRelative("useCustomParameters").boolValue;

        float baseHeight = 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        if (useCustomParameters && effect != null)
        {
            baseHeight += ActionEffectParameterEditorGUI.GetVisibleParameterCount(effect) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
        return baseHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var targetKind = property.FindPropertyRelative("targetKind");
        var effect = property.FindPropertyRelative("effect");
        var useCustomParameters = property.FindPropertyRelative("useCustomParameters");
        var parameters = property.FindPropertyRelative("parameters");
        
        var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Draw Target Kind
        EditorGUI.PropertyField(line, targetKind);
        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Draw Effect
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(line, effect);
        bool effectChanged = EditorGUI.EndChangeCheck();
        if (effect.objectReferenceValue is ActionEffectSO effectAsset
            && (effectChanged || ActionEffectParameterEditorGUI.IsInitialParameters(parameters)))
        {
            ActionEffectParameterEditorGUI.ApplyParameters(parameters, effectAsset.CreateDefaultParameters());
        }
        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Draw Use Custom Parameters Toggle
        EditorGUI.PropertyField(line, useCustomParameters, new GUIContent("Use Custom Parameters?"));
        
        if (useCustomParameters.boolValue && effect.objectReferenceValue is ActionEffectSO currentEffect)
        {
            EditorGUI.indentLevel++;
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            ActionEffectParameterEditorGUI.DrawFields(line, currentEffect, parameters);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}