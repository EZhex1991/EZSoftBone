/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-07-24 10:59:33
 * Organization:    #ORGANIZATION#
 * Description:     EZNestedEditorDrawer is not suitable for EZSoftBoneMaterial (Default-Material editing should be disabled)
 */
using UnityEditor;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    [CustomPropertyDrawer(typeof(EZSoftBoneMaterial))]
    public class EZSoftBoneMaterialDrawer : PropertyDrawer
    {
        private bool initialized;
        private SerializedObject serializedObject;
        private SerializedProperty m_Damping;
        private SerializedProperty m_Stiffness;
        private SerializedProperty m_Resistance;
        private SerializedProperty m_Slackness;
        private SerializedProperty m_DampingCurve;
        private SerializedProperty m_StiffnessCurve;
        private SerializedProperty m_ResistanceCurve;
        private SerializedProperty m_SlacknessCurve;

        private void GetSerializedProperties(Object material)
        {
            if (material == null)
            {
                serializedObject = null;
            }
            else
            {
                serializedObject = new SerializedObject(material);
                m_Damping = serializedObject.FindProperty("m_Damping");
                m_Stiffness = serializedObject.FindProperty("m_Stiffness");
                m_Resistance = serializedObject.FindProperty("m_Resistance");
                m_Slackness = serializedObject.FindProperty("m_Slackness");
                m_DampingCurve = serializedObject.FindProperty("m_DampingCurve");
                m_StiffnessCurve = serializedObject.FindProperty("m_StiffnessCurve");
                m_ResistanceCurve = serializedObject.FindProperty("m_ResistanceCurve");
                m_SlacknessCurve = serializedObject.FindProperty("m_SlacknessCurve");
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label);
                if (EditorGUI.EndChangeCheck() || !initialized)
                {
                    initialized = true;
                    GetSerializedProperties(property.objectReferenceValue);
                }
                if (serializedObject != null)
                {
                    property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none, false);
                    if (property.isExpanded)
                    {
                        serializedObject.Update();
                        EditorGUI.indentLevel++;
                        GUI.enabled = property.objectReferenceValue != EZSoftBoneMaterial.defaultMaterial;
                        EditorGUILayout.PropertyField(m_Damping);
                        EditorGUILayout.PropertyField(m_DampingCurve);
                        EditorGUILayout.PropertyField(m_Stiffness);
                        EditorGUILayout.PropertyField(m_StiffnessCurve);
                        EditorGUILayout.PropertyField(m_Resistance);
                        EditorGUILayout.PropertyField(m_ResistanceCurve);
                        EditorGUILayout.PropertyField(m_Slackness);
                        EditorGUILayout.PropertyField(m_SlacknessCurve);
                        GUI.enabled = true;
                        EditorGUI.indentLevel--;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
