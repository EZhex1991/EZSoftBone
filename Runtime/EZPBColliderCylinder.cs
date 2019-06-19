/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-06-19 13:39:19
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZUnity.PhysicsBone
{
    public class EZPBColliderCylinder : EZPhysicsBoneColliderBase
    {
        public override void Collide(ref Vector3 position, float spacing)
        {
            EZPhysicsBoneUtility.PointOutsideCylinder(ref position, transform, spacing);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center, direction;
            float radius, height;
            EZPhysicsBoneUtility.GetCylinderParams(transform, out center, out direction, out radius, out height);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.matrix = Matrix4x4.identity;
            Vector3 p0 = center + direction * height;
            Vector3 p1 = center - direction * height;
            UnityEditor.Handles.DrawWireDisc(p0, transform.up, radius);
            UnityEditor.Handles.DrawWireDisc(p1, transform.up, radius);
            UnityEditor.Handles.matrix *= Matrix4x4.Translate(transform.forward * radius);
            UnityEditor.Handles.DrawLine(p0, p1);
            UnityEditor.Handles.matrix *= Matrix4x4.Translate(-transform.forward * 2 * radius);
            UnityEditor.Handles.DrawLine(p0, p1);
            UnityEditor.Handles.matrix *= Matrix4x4.Translate((transform.right + transform.forward) * radius);
            UnityEditor.Handles.DrawLine(p0, p1);
            UnityEditor.Handles.matrix *= Matrix4x4.Translate(-transform.right * 2 * radius);
            UnityEditor.Handles.DrawLine(p0, p1);
        }
#endif
    }
}
