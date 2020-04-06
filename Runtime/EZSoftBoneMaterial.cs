/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-20 11:07:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    [CreateAssetMenu(fileName = "SBMat", menuName = "EZSoftBone/SBMaterial")]
    public class EZSoftBoneMaterial : ScriptableObject
    {
        [SerializeField, Range(0, 1)]
        private float m_Damping = 0.2f;
        public float damping { get { return m_Damping; } set { m_Damping = Mathf.Clamp01(value); } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_DampingCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1);
        public AnimationCurve dampingCurve { get { return m_DampingCurve; } }

        [SerializeField, Range(0, 1)]
        private float m_Stiffness = 0.1f;
        public float stiffness { get { return m_Stiffness; } set { m_Stiffness = Mathf.Clamp01(value); } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_StiffnessCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve stiffnessCurve { get { return m_StiffnessCurve; } }

        [SerializeField, Range(0, 1)]
        private float m_Resistance = 0.9f;
        public float resistance { get { return m_Resistance; } set { m_Resistance = Mathf.Clamp01(value); } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_ResistanceCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve resistanceCurve { get { return m_ResistanceCurve; } }

        [SerializeField, Range(0, 1)]
        private float m_Slackness = 0.1f;
        public float slackness { get { return m_Slackness; } set { m_Slackness = Mathf.Clamp01(value); } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_SlacknessCurve = AnimationCurve.Linear(0, 1, 1, 0.8f);
        public AnimationCurve slacknessCurve { get { return m_SlacknessCurve; } }

        private static EZSoftBoneMaterial m_DefaultMaterial;
        public static EZSoftBoneMaterial defaultMaterial
        {
            get
            {
                if (m_DefaultMaterial == null)
                    m_DefaultMaterial = CreateInstance<EZSoftBoneMaterial>();
                m_DefaultMaterial.name = "SBMat_Default";
                return m_DefaultMaterial;
            }
        }

        public float GetDamping(float t)
        {
            return damping * dampingCurve.Evaluate(t);
        }
        public float GetStiffness(float t)
        {
            return stiffness * stiffnessCurve.Evaluate(t);
        }
        public float GetResistance(float t)
        {
            return resistance * resistanceCurve.Evaluate(t);
        }
        public float GetSlackness(float t)
        {
            return slackness * slacknessCurve.Evaluate(t);
        }
    }
}