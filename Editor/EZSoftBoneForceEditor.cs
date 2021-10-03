/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-06-10 14:01:41
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    [CustomEditor(typeof(EZSoftBoneForce))]
    public class EZSoftBoneForceEditor : Editor
    {
        private SerializedProperty m_Force;

        private SerializedProperty m_Turbulence;
        private SerializedProperty m_TurbulenceMode;

        private SerializedProperty m_Frequency;

        private SerializedProperty m_TimeCycle;
        private SerializedProperty m_CurveX;
        private SerializedProperty m_CurveY;
        private SerializedProperty m_CurveZ;

        private void OnEnable()
        {
            m_Force = serializedObject.FindProperty(nameof(m_Force));
            m_Turbulence = serializedObject.FindProperty(nameof(m_Turbulence));
            m_TurbulenceMode = serializedObject.FindProperty(nameof(m_TurbulenceMode));

            m_Frequency = serializedObject.FindProperty(nameof(m_Frequency));

            m_TimeCycle = serializedObject.FindProperty(nameof(m_TimeCycle));
            m_CurveX = serializedObject.FindProperty(nameof(m_CurveX));
            m_CurveY = serializedObject.FindProperty(nameof(m_CurveY));
            m_CurveZ = serializedObject.FindProperty(nameof(m_CurveZ));
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(target as ScriptableObject), typeof(MonoScript), false);
            GUI.enabled = true;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Force);
            EditorGUILayout.PropertyField(m_Turbulence);
            EditorGUILayout.PropertyField(m_TurbulenceMode);

            if (m_TurbulenceMode.intValue == (int)EZSoftBoneForce.TurbulenceMode.Curve)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_TimeCycle);
                EditorGUILayout.PropertyField(m_CurveX);
                EditorGUILayout.PropertyField(m_CurveY);
                EditorGUILayout.PropertyField(m_CurveZ);
                EditorGUI.indentLevel--;
            }
            else if (m_TurbulenceMode.intValue == (int)EZSoftBoneForce.TurbulenceMode.Perlin)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Frequency);
                EditorGUI.indentLevel--;
            }

            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }
    }
}
