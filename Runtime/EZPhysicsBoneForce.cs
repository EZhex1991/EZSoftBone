/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-27 15:33:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZUnity.PhysicsBone
{
    public class EZPhysicsBoneForce : MonoBehaviour
    {
        [SerializeField]
        private bool m_UseLocalDirection;
        public bool useLocalDirection { get { return m_UseLocalDirection; } }

        [SerializeField]
        private Vector3 m_Direction;
        public Vector3 direction { get { return m_Direction; } set { m_Direction = value; } }

        [SerializeField]
        private Vector3 m_Turbulence = new Vector3(0.1f, 0.02f, 0.1f);
        public Vector3 turbulence { get { return m_Turbulence; } set { m_Turbulence = value; } }

        [SerializeField]
        private float m_TurbulenceTimeCycle = 2f;
        public float turbulenceTimeCycle { get { return m_TurbulenceTimeCycle; } set { m_TurbulenceTimeCycle = Mathf.Max(0, value); } }

        [SerializeField, Range(0, 1)]
        private float m_Conductivity = 0.15f;
        public float conductivity { get { return m_Conductivity; } set { m_Conductivity = value; } }

        [SerializeField, EZCurveRange(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceXCurve = AnimationCurve.Linear(0, -1, 1, 1);
        [SerializeField, EZCurveRange(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceYCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField, EZCurveRange(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceZCurve = AnimationCurve.EaseInOut(0, 1, 1, -1);

        public Vector3 GetForce(float normalizedLength)
        {
            if (!isActiveAndEnabled) return Vector3.zero;
            float time = (Time.time % m_TurbulenceTimeCycle) / m_TurbulenceTimeCycle;
            time = (time - conductivity * normalizedLength) % 1f;
            Vector3 tbl = turbulence;
            tbl.x *= m_TurbulenceXCurve.Evaluate(time);
            tbl.y *= m_TurbulenceYCurve.Evaluate(time);
            tbl.z *= m_TurbulenceZCurve.Evaluate(time);
            if (useLocalDirection)
            {
                return transform.TransformDirection(direction + tbl);
            }
            else
            {
                return direction + tbl;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 force0 = GetForce(0) * 50;
            float width = force0.magnitude * 0.2f;
            Gizmos.DrawRay(transform.position, force0);
            EZPhysicsBoneUtility.DrawGizmosArrow(transform.position, force0, width, transform.up);
            EZPhysicsBoneUtility.DrawGizmosArrow(transform.position, force0, width, transform.right);
        }
    }
}
