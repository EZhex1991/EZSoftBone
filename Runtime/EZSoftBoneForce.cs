/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-27 15:33:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    [CreateAssetMenu(fileName = "SBForce", menuName = "EZSoftBone/SBForce")]
    public class EZSoftBoneForce : ScriptableObject
    {
        public enum TurbulenceMode
        {
            Curve,
            Perlin,
        }

        [SerializeField]
        private Vector3 m_Direction;
        public Vector3 direction { get { return m_Direction; } set { m_Direction = value; } }

        [SerializeField, Range(0, 1)]
        private float m_Conductivity = 0.15f;
        public float conductivity { get { return m_Conductivity; } set { m_Conductivity = value; } }

        [SerializeField]
        private Vector3 m_Turbulence = new Vector3(0.1f, 0.02f, 0.1f);
        public Vector3 turbulence { get { return m_Turbulence; } set { m_Turbulence = value; } }

        [SerializeField]
        private TurbulenceMode m_TurbulenceMode = TurbulenceMode.Perlin;
        public TurbulenceMode turbulenceMode { get { return m_TurbulenceMode; } }

        [SerializeField]
        private float m_TurbulenceTimeCycle = 2f;
        public float turbulenceTimeCycle { get { return m_TurbulenceTimeCycle; } set { m_TurbulenceTimeCycle = Mathf.Max(0, value); } }

        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceXCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceYCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_TurbulenceZCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [SerializeField]
        private Vector3 m_TurbulenceSpeed = new Vector3(0.2f, 0.2f, 0.2f);
        public Vector3 turbulenceSpeed { get { return m_TurbulenceSpeed; } set { m_TurbulenceSpeed = value; } }

        [SerializeField]
        private Vector3 m_TurbulenceRandomSeed = new Vector3(0, 0.5f, 1);
        public Vector3 turbulenceRandomSeed { get { return m_TurbulenceRandomSeed; } set { m_TurbulenceRandomSeed = value; } }

        public Vector3 GetForce(float normalizedLength, Transform forceSpace)
        {
            Vector3 tbl = turbulence;
            float time = Time.time - conductivity * normalizedLength;
            switch (turbulenceMode)
            {
                case TurbulenceMode.Curve:
                    time = Mathf.Repeat(time, m_TurbulenceTimeCycle) / m_TurbulenceTimeCycle;
                    tbl.x *= m_TurbulenceXCurve.Evaluate(time);
                    tbl.y *= m_TurbulenceYCurve.Evaluate(time);
                    tbl.z *= m_TurbulenceZCurve.Evaluate(time);
                    break;
                case TurbulenceMode.Perlin:
                    tbl.x *= Mathf.PerlinNoise(time * turbulenceSpeed.x, turbulenceRandomSeed.x);
                    tbl.y *= Mathf.PerlinNoise(time * turbulenceSpeed.y, turbulenceRandomSeed.y);
                    tbl.z *= Mathf.PerlinNoise(time * turbulenceSpeed.z, turbulenceRandomSeed.z);
                    break;
            }
            if (forceSpace != null)
            {
                return forceSpace.TransformDirection(direction + tbl);
            }
            else
            {
                return direction + tbl;
            }
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform gizmosReference, Transform forceSpace)
        {
            Vector3 force0 = GetForce(0, forceSpace) * UnityEditor.HandleUtility.GetHandleSize(gizmosReference.position);
            float width = force0.magnitude * 0.2f;

            if (forceSpace == null)
            {
                Gizmos.DrawRay(gizmosReference.position, force0);
                EZSoftBoneUtility.DrawGizmosArrow(gizmosReference.position, force0, width, Vector3.up);
                EZSoftBoneUtility.DrawGizmosArrow(gizmosReference.position, force0, width, Vector3.right);
            }
            else
            {
                Gizmos.DrawRay(gizmosReference.position, force0);
                EZSoftBoneUtility.DrawGizmosArrow(gizmosReference.position, force0, width, gizmosReference.up);
                EZSoftBoneUtility.DrawGizmosArrow(gizmosReference.position, force0, width, gizmosReference.right);
            }
        }
#endif
    }
}
