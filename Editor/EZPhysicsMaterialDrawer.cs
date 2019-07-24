/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-07-24 10:59:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor;
using UnityEngine;

namespace EZhex1991.EZPhysicsBone
{
    [CustomPropertyDrawer(typeof(EZPhysicsBoneMaterial))]
    public class EZPhysicsBoneMaterialDrawer : PropertyDrawer
    {
        private SerializedObject m_SerializedObject;
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
                m_SerializedObject = null;
            }
            else
            {
                m_SerializedObject = new SerializedObject(material);
                m_Damping = m_SerializedObject.FindProperty("m_Damping");
                m_Stiffness = m_SerializedObject.FindProperty("m_Stiffness");
                m_Resistance = m_SerializedObject.FindProperty("m_Resistance");
                m_Slackness = m_SerializedObject.FindProperty("m_Slackness");
                m_DampingCurve = m_SerializedObject.FindProperty("m_DampingCurve");
                m_StiffnessCurve = m_SerializedObject.FindProperty("m_StiffnessCurve");
                m_ResistanceCurve = m_SerializedObject.FindProperty("m_ResistanceCurve");
                m_SlacknessCurve = m_SerializedObject.FindProperty("m_SlacknessCurve");
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property);
            if (EditorGUI.EndChangeCheck() || m_SerializedObject == null)
            {
                GetSerializedProperties(property.objectReferenceValue);
            }
            if (m_SerializedObject != null)
            {
                property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none, false);
                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    GUI.enabled = property.objectReferenceValue != EZPhysicsBoneMaterial.defaultMaterial;
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
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
