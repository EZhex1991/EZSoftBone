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
        [SerializeField]
        private float m_Force = 1;
        public float force { get { return m_Force; } set { m_Force = value; } }

        public enum TurbulenceMode
        {
            Curve,
            Perlin,
        }

        [SerializeField]
        private Vector3 m_Turbulence = new Vector3(1f, 0.5f, 2f);
        public Vector3 turbulence { get { return m_Turbulence; } set { m_Turbulence = value; } }

        [SerializeField]
        private TurbulenceMode m_TurbulenceMode = TurbulenceMode.Perlin;
        public TurbulenceMode turbulenceMode { get { return m_TurbulenceMode; } set { m_TurbulenceMode = value; } }

        #region Perlin
        [SerializeField]
        private Vector3 m_Frequency = new Vector3(1f, 1f, 1.5f);
        public Vector3 frequency { get { return m_Frequency; } set { m_Frequency = value; } }
        #endregion

        #region Curve
        [SerializeField]
        private float m_TimeCycle = 2f;
        public float timeCycle { get { return m_TimeCycle; } set { m_TimeCycle = Mathf.Max(0, value); } }

        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_CurveX = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_CurveY = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField, EZCurveRect(0, -1, 1, 2)]
        private AnimationCurve m_CurveZ = AnimationCurve.EaseInOut(0, 1, 1, 0);
        #endregion

        public Vector3 GetForce(float time)
        {
            Vector3 tbl = turbulence;
            switch (turbulenceMode)
            {
                case TurbulenceMode.Curve:
                    time = Mathf.Repeat(time, m_TimeCycle) / m_TimeCycle;
                    tbl.x *= Curve(m_CurveX, time);
                    tbl.y *= Curve(m_CurveY, time);
                    tbl.z *= Curve(m_CurveZ, time);
                    break;
                case TurbulenceMode.Perlin:
                    tbl.x *= Perlin(time * frequency.x, 0);
                    tbl.y *= Perlin(time * frequency.y, 0.5f);
                    tbl.z *= Perlin(time * frequency.z, 1.0f);
                    break;
            }
            return new Vector3(0, 0, force) + tbl;
        }

        private float Perlin(float x, float y)
        {
            return Mathf.PerlinNoise(x, y) * 2 - 1;
        }
        private float Curve(AnimationCurve curve, float time)
        {
            return curve.Evaluate(time);
        }
    }
}
