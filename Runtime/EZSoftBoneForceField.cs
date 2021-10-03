/* Author:          ezhex1991@outlook.com
 * CreateTime:      2021-10-03 09:19:36
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public class EZSoftBoneForceField : MonoBehaviour
    {
        [SerializeField, Range(0, 1)]
        private float m_Conductivity = 0.15f;
        public float conductivity { get { return m_Conductivity; } set { m_Conductivity = value; } }

        [SerializeField, EZNestedEditor]
        private EZSoftBoneForce m_Force;
        public EZSoftBoneForce force { get { return m_Force; } set { m_Force = value; } }

        public Vector3 GetForce(float time, float normalizedLength)
        {
            time -= conductivity * normalizedLength;
            return transform.TransformDirection(force.GetForce(time));
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }
        public void DrawGizmos()
        {
            if (force == null) return;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            if (Application.isPlaying)
            {
                Vector3 forceVector = force.GetForce(Time.time);
                float width = forceVector.magnitude * 0.2f;
                EZSoftBoneUtility.DrawGizmosArrow(Vector3.zero, forceVector, width, Vector3.up);
                EZSoftBoneUtility.DrawGizmosArrow(Vector3.zero, forceVector, width, Vector3.right);
                Gizmos.DrawRay(Vector3.zero, forceVector);
            }
            else
            {
                Vector3 forceVector = new Vector3(0, 0, force.force);
                float width = force.force * 0.2f;
                EZSoftBoneUtility.DrawGizmosArrow(Vector3.zero, forceVector, width, Vector3.up);
                EZSoftBoneUtility.DrawGizmosArrow(Vector3.zero, forceVector, width, Vector3.right);
                Gizmos.DrawRay(Vector3.zero, forceVector);
            }
            Gizmos.DrawWireCube(new Vector3(0, 0, force.force), force.turbulence);
        }
#endif
    }
}
