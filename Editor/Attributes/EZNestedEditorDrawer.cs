/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-07-18 15:41:57
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    [CustomPropertyDrawer(typeof(EZNestedEditorAttribute))]
    public class EZNestedEditorDrawer : PropertyDrawer
    {
        private Editor nestedEditor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
                if (property.objectReferenceValue != null)
                {
                    property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none);
                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        if (property.type == "PPtr<$Material>")
                        {
                            Editor.CreateCachedEditor(property.objectReferenceValue, typeof(MaterialEditor), ref nestedEditor);
                            (nestedEditor as MaterialEditor).PropertiesGUI();
                        }
                        else
                        {
                            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref nestedEditor);
                            nestedEditor.OnInspectorGUI();
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}