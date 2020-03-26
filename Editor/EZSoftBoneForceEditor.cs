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
        private SerializedProperty m_Direction;
        private SerializedProperty m_Conductivity;

        private SerializedProperty m_Turbulence;
        private SerializedProperty m_TurbulenceMode;

        private SerializedProperty m_TurbulenceTimeCycle;
        private SerializedProperty m_TurbulenceXCurve;
        private SerializedProperty m_TurbulenceYCurve;
        private SerializedProperty m_TurbulenceZCurve;

        private SerializedProperty m_TurbulenceSpeed;
        private SerializedProperty m_TurbulenceRandomSeed;

        private void OnEnable()
        {
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_Conductivity = serializedObject.FindProperty("m_Conductivity");

            m_Turbulence = serializedObject.FindProperty("m_Turbulence");
            m_TurbulenceMode = serializedObject.FindProperty("m_TurbulenceMode");

            m_TurbulenceTimeCycle = serializedObject.FindProperty("m_TurbulenceTimeCycle");
            m_TurbulenceXCurve = serializedObject.FindProperty("m_TurbulenceXCurve");
            m_TurbulenceYCurve = serializedObject.FindProperty("m_TurbulenceYCurve");
            m_TurbulenceZCurve = serializedObject.FindProperty("m_TurbulenceZCurve");

            m_TurbulenceSpeed = serializedObject.FindProperty("m_TurbulenceSpeed");
            m_TurbulenceRandomSeed = serializedObject.FindProperty("m_TurbulenceRandomSeed");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(target as ScriptableObject), typeof(MonoScript), false);
            GUI.enabled = true;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Direction);
            EditorGUILayout.PropertyField(m_Conductivity);

            EditorGUILayout.PropertyField(m_Turbulence);
            EditorGUILayout.PropertyField(m_TurbulenceMode);

            if (m_TurbulenceMode.intValue == (int)EZSoftBoneForce.TurbulenceMode.Curve)
            {
                EditorGUILayout.PropertyField(m_TurbulenceTimeCycle);
                EditorGUILayout.PropertyField(m_TurbulenceXCurve);
                EditorGUILayout.PropertyField(m_TurbulenceYCurve);
                EditorGUILayout.PropertyField(m_TurbulenceZCurve);
            }
            else if (m_TurbulenceMode.intValue == (int)EZSoftBoneForce.TurbulenceMode.Perlin)
            {
                EditorGUILayout.PropertyField(m_TurbulenceSpeed);
                EditorGUILayout.PropertyField(m_TurbulenceRandomSeed);
            }

            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }
    }
}
