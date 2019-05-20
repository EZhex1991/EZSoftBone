/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-24 13:22:59
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EZUnity.PhysicsBone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EZCurveRangeAttribute : PropertyAttribute
    {
        public Color color = Color.green;
        public Rect range;

        public EZCurveRangeAttribute(Rect range)
        {
            this.range = range;
        }
        public EZCurveRangeAttribute(float x, float y, float width, float height)
        {
            this.range = new Rect(x, y, width, height);
        }
        public EZCurveRangeAttribute(Rect range, Color color)
        {
            this.range = range;
            this.color = color;
        }
        public EZCurveRangeAttribute(float x, float y, float width, float height, Color color)
        {
            this.range = new Rect(x, y, width, height);
            this.color = color;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EZCurveRangeAttribute))]
    public class EZCurveRangePropertyDrawer : PropertyDrawer
    {
        private EZCurveRangeAttribute curveRangeAttribute { get { return attribute as EZCurveRangeAttribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.CurveField(position, property, curveRangeAttribute.color, curveRangeAttribute.range, label);
            EditorGUI.EndProperty();
        }
    }
#endif
}
