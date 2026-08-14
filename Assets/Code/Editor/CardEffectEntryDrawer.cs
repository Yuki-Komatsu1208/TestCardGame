using TestCardGame.Actions.Effects;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CardEffectEntry))]
public class CardEffectEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var effect = property.FindPropertyRelative("effect").objectReferenceValue as ActionEffectSO;
        return (1 + ActionEffectParameterEditorGUI.GetVisibleParameterCount(effect)) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var effect = property.FindPropertyRelative("effect");
        var parameters = property.FindPropertyRelative("parameters");
        var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(line, effect, label);
        bool effectChanged = EditorGUI.EndChangeCheck();
        if (effect.objectReferenceValue is ActionEffectSO effectAsset
            && (effectChanged || ActionEffectParameterEditorGUI.IsInitialParameters(parameters)))
        {
            ActionEffectParameterEditorGUI.ApplyParameters(parameters, effectAsset.CreateDefaultParameters());
        }

        EditorGUI.indentLevel++;
        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        ActionEffectParameterEditorGUI.DrawFields(line, effect.objectReferenceValue as ActionEffectSO, parameters);
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }
}
