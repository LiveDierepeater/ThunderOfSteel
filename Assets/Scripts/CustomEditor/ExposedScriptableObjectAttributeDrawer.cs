using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ExposedScriptableObjectAttribute))]
public class ExposedScriptableObjectAttributeDrawer : PropertyDrawer
{
    private Editor _editor;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw label
        EditorGUI.PropertyField(position, property, label, true);
        
        // Draw foldout arrow
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
        }
        
        // Draw foldout properties
        if (property.isExpanded)
        {
            // Make child field be indented
            EditorGUI.indentLevel++;
            
            // Draw object properties
            if (!_editor)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
            _editor.OnInspectorGUI();
            
            // Set indent back to what it was
            EditorGUI.indentLevel--;
        }
    }
}
