/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-18 19:33:50
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public delegate Vector3 CustomForce(float normalizedLength);

    public class EZSoftBone : MonoBehaviour
    {
        public static readonly float DeltaTime_Min = 1e-6f;

        public enum UnificationMode
        {
            None,
            Rooted,
            Unified,
        }

        public enum DeltaTimeMode
        {
            DeltaTime,
            UnscaledDeltaTime,
            Constant,
        }

        private class Bone
        {
            public Bone parentBone;
            public Vector3 localPosition;
            public Quaternion localRotation;

            public Bone leftBone;
            public Vector3 leftPosition;
            public Bone rightBone;
            public Vector3 rightPosition;

            public List<Bone> childBones = new List<Bone>();

            public Transform transform;
            public Vector3 worldPosition;

            public Transform systemSpace;
            public Vector3 systemPosition;

            public int depth;
            public float boneLength;
            public float treeLength;
            public float normalizedLength;

            public float radius;
            public float damping;
            public float stiffness;
            public float resistance;
            public float slackness;

            public Vector3 speed;

            public Bone(Transform systemSpace, Transform transform, IEnumerable<Transform> endBones, int startDepth, int depth, float nodeLength, float boneLength)
            {
                this.transform = transform;
                this.systemSpace = systemSpace;
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
                this.depth = depth;
                if (depth > startDepth)
                {
                    this.boneLength = boneLength + nodeLength;
                }
                treeLength = Mathf.Max(treeLength, this.boneLength);
                if (transform.childCount > 0 && !endBones.Contains(transform))
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        if (!child.gameObject.activeSelf) continue;
                        Bone childBone = new Bone(systemSpace, child, endBones, startDepth, depth + 1, Vector3.Distance(child.position, transform.position), this.boneLength);
                        childBone.parentBone = this;
                        childBones.Add(childBone);
                        treeLength = Mathf.Max(treeLength, childBone.treeLength);
                    }
                }
                normalizedLength = treeLength == 0 ? 0 : (this.boneLength / treeLength);
            }

            public void SetTreeLength()
            {
                SetTreeLength(treeLength);
            }
            public void SetTreeLength(float treeLength)
            {
                this.treeLength = treeLength;
                normalizedLength = treeLength == 0 ? 0 : (boneLength / treeLength);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].SetTreeLength(treeLength);
                }
            }

            public void SetLeftSibling(Bone left)
            {
                if (left == this || left == rightBone) return;
                leftBone = left;
                leftPosition = transform.InverseTransformPoint(left.worldPosition);
            }
            public void SetRightSibling(Bone right)
            {
                if (right == this || right == leftBone) return;
                rightBone = right;
                rightPosition = transform.InverseTransformPoint(right.worldPosition);
            }

            public void Inflate(float baseRadius, AnimationCurve radiusCurve)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].Inflate(baseRadius, radiusCurve);
                }
            }
            public void Inflate(float baseRadius, AnimationCurve radiusCurve, EZSoftBoneMaterial material)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                damping = material.GetDamping(normalizedLength);
                stiffness = material.GetStiffness(normalizedLength);
                resistance = material.GetResistance(normalizedLength);
                slackness = material.GetSlackness(normalizedLength);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].Inflate(baseRadius, radiusCurve, material);
                }
            }

            public void RevertTransforms(int startDepth)
            {
                if (depth > startDepth)
                {
                    transform.localPosition = localPosition;
                    transform.localRotation = localRotation;
                }
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].RevertTransforms(startDepth);
                }
            }
            public void UpdateTransform(bool siblingRotationConstraints, int startDepth)
            {
                if (depth > startDepth)
                {
                    if (childBones.Count == 1)
                    {
                        Bone childBone = childBones[0];
                        transform.rotation *= Quaternion.FromToRotation(childBone.localPosition,
                                                                        transform.InverseTransformVector(childBone.worldPosition - worldPosition));

                        if (siblingRotationConstraints)
                        {
                            if (leftBone != null && rightBone != null)
                            {
                                Vector3 directionLeft0 = leftPosition;
                                Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                                Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);

                                Vector3 directionRight0 = rightPosition;
                                Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                                Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);

                                transform.rotation *= Quaternion.Lerp(rotationLeft, rotationRight, 0.5f);
                            }
                            else if (leftBone != null)
                            {
                                Vector3 directionLeft0 = leftPosition;
                                Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                                Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);
                                transform.rotation *= rotationLeft;
                            }
                            else if (rightBone != null)
                            {
                                Vector3 directionRight0 = rightPosition;
                                Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                                Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);
                                transform.rotation *= rotationRight;
                            }
                        }
                    }
                    transform.position = worldPosition;
                }

                if (systemSpace != null) systemPosition = systemSpace.InverseTransformPoint(worldPosition);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].UpdateTransform(siblingRotationConstraints, startDepth);
                }
            }

            public void SetRestState()
            {
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                speed = Vector3.zero;
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].SetRestState();
                }
            }
            public void UpdateSpace()
            {
                if (systemSpace == null) return;
                worldPosition = systemSpace.TransformPoint(systemPosition);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].UpdateSpace();
                }
            }
        }

        [SerializeField]
        private List<Transform> m_RootBones;
        public List<Transform> rootBones { get { return m_RootBones; } }
        [SerializeField]
        private List<Transform> m_EndBones;
        public List<Transform> endBones { get { return m_EndBones; } }

        [SerializeField]
        private EZSoftBoneMaterial m_Material;
        private EZSoftBoneMaterial m_InstanceMaterial;
        public EZSoftBoneMaterial sharedMaterial
        {
            get
            {
                if (m_Material == null)
                    m_Material = EZSoftBoneMaterial.defaultMaterial;
                return m_Material;
            }
            set
            {
                m_Material = value;
            }
        }
        public EZSoftBoneMaterial material
        {
            get
            {
                if (m_InstanceMaterial == null)
                {
                    m_InstanceMaterial = m_Material = Instantiate(sharedMaterial);
                }
                return m_InstanceMaterial;
            }
            set
            {
                m_InstanceMaterial = m_Material = value;
            }
        }

        #region Structure
        [SerializeField]
        private int m_StartDepth;
        public int startDepth { get { return m_StartDepth; } set { m_StartDepth = value; } }

        [SerializeField]
        private UnificationMode m_SiblingConstraints = UnificationMode.None;
        public UnificationMode siblingConstraints { get { return m_SiblingConstraints; } set { m_SiblingConstraints = value; } }
        [SerializeField]
        private bool m_ClosedSiblings = false;
        public bool closedSiblings { get { return m_ClosedSiblings; } set { m_ClosedSiblings = value; } }
        [SerializeField]
        private bool m_SiblingRotationConstraints = true;
        public bool siblingRotationConstraints { get { return m_SiblingRotationConstraints; } set { m_SiblingRotationConstraints = value; } }
        [SerializeField]
        private UnificationMode m_LengthUnification = UnificationMode.None;
        public UnificationMode lengthUnification { get { return m_LengthUnification; } set { m_LengthUnification = value; } }
        #endregion

        #region Collision
        [SerializeField]
        private LayerMask m_CollisionLayers = 1;
        public LayerMask collisionLayers { get { return m_CollisionLayers; } set { m_CollisionLayers = value; } }
        [SerializeField]
        private List<Collider> m_ExtraColliders = new List<Collider>();
        public List<Collider> extraColliders { get { return m_ExtraColliders; } }
        [SerializeField]
        private float m_Radius = 0;
        public float radius { get { return m_Radius; } set { m_Radius = value; } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_RadiusCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve radiusCurve { get { return m_RadiusCurve; } }
        #endregion

        #region Performance
        [SerializeField]
        private DeltaTimeMode m_DeltaTimeMode = DeltaTimeMode.DeltaTime;
        public DeltaTimeMode deltaTimeMode { get { return m_DeltaTimeMode; } set { m_DeltaTimeMode = value; } }
        [SerializeField]
        private float m_ConstantDeltaTime = 0.03f;
        public float constantDeltaTime { get { return m_ConstantDeltaTime; } set { m_ConstantDeltaTime = value; } }

        [SerializeField, Range(1, 10)]
        private int m_Iterations = 1;
        public int iterations { get { return m_Iterations; } set { m_Iterations = value; } }

        [SerializeField]
        private float m_SleepThreshold = 0.005f;
        public float sleepThreshold { get { return m_SleepThreshold; } set { m_SleepThreshold = Mathf.Max(0, value); } }
        #endregion

        #region Gravity
        [SerializeField]
        private Transform m_GravityAligner;
        public Transform gravityAligner { get { return m_GravityAligner; } set { m_GravityAligner = value; } }
        [SerializeField]
        private Vector3 m_Gravity;
        public Vector3 gravity { get { return m_Gravity; } set { m_Gravity = value; } }
        #endregion

        #region Force
        [SerializeField]
        private EZSoftBoneForceField m_ForceModule;
        public EZSoftBoneForceField forceModule { get { return m_ForceModule; } set { m_ForceModule = value; } }
        [SerializeField]
        private float m_ForceScale = 1;
        public float forceScale { get { return m_ForceScale; } set { m_ForceScale = value; } }
        #endregion

        #region References
        [SerializeField]
        private Transform m_SimulateSpace;
        public Transform simulateSpace { get { return m_SimulateSpace; } set { m_SimulateSpace = value; } }
        #endregion

        public float globalRadius { get; private set; }
        public Vector3 globalForce { get; private set; }

        public CustomForce customForce;

        private List<Bone> m_Structures = new List<Bone>();

        private void Awake()
        {
            InitStructures();
        }
        private void OnEnable()
        {
            SetRestState();
        }
        private void Update()
        {
            RevertTransforms(startDepth);
        }
        private void LateUpdate()
        {
            switch (deltaTimeMode)
            {
                case DeltaTimeMode.DeltaTime:
                    UpdateStructures(Time.deltaTime);
                    break;
                case DeltaTimeMode.UnscaledDeltaTime:
                    UpdateStructures(Time.unscaledDeltaTime);
                    break;
                case DeltaTimeMode.Constant:
                    UpdateStructures(constantDeltaTime);
                    break;
            }
            UpdateTransforms();
        }
        private void OnDisable()
        {
            RevertTransforms(startDepth);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_StartDepth = Mathf.Max(0, m_StartDepth);
            m_ConstantDeltaTime = Mathf.Max(DeltaTime_Min, m_ConstantDeltaTime);
            m_Iterations = Mathf.Max(1, m_Iterations);
            m_SleepThreshold = Mathf.Max(0, m_SleepThreshold);
            m_Radius = Mathf.Max(0, m_Radius);
        }
        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            if (!Application.isPlaying)
            {
                InitStructures();
            }

            for (int i = 0; i < m_Structures.Count; i++)
            {
                DrawBoneGizmos(m_Structures[i]);
            }

            if (forceModule != null)
            {
                forceModule.DrawGizmos();
            }
        }
        private void DrawBoneGizmos(Bone bone)
        {
            for (int i = 0; i < bone.childBones.Count; i++)
            {
                DrawBoneGizmos(bone.childBones[i]);
            }

            Gizmos.color = Color.Lerp(Color.white, Color.red, bone.normalizedLength);
            if (bone.parentBone != null)
                Gizmos.DrawLine(bone.worldPosition, bone.parentBone.worldPosition);
            if (bone.depth > startDepth)
                Gizmos.DrawWireSphere(bone.worldPosition, bone.radius);
            if (siblingConstraints != UnificationMode.None)
            {
                if (bone.leftBone != null)
                    Gizmos.DrawLine(bone.leftBone.worldPosition, bone.worldPosition);
                if (bone.rightBone != null)
                    Gizmos.DrawLine(bone.rightBone.worldPosition, bone.worldPosition);
            }
        }
#endif

        public void RevertTransforms()
        {
            RevertTransforms(startDepth);
        }
        public void RevertTransforms(int startDepth)
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].RevertTransforms(startDepth);
            }
        }
        public void InitStructures()
        {
            CreateBones();
            SetSiblings();
            SetTreeLength();
            RefreshRadius();
        }
        public void SetRestState()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].SetRestState();
            }
        }

        private void CreateBones()
        {
            m_Structures.Clear();
            if (rootBones == null || rootBones.Count == 0) return;
            for (int i = 0; i < rootBones.Count; i++)
            {
                if (rootBones[i] == null) continue;
                Bone bone = new Bone(simulateSpace, rootBones[i], endBones, startDepth, 0, 0, 0);
                m_Structures.Add(bone);
            }
        }
        private void SetSiblings()
        {
            if (siblingConstraints == UnificationMode.Rooted)
            {
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    Queue<Bone> bones = new Queue<Bone>();
                    bones.Enqueue(m_Structures[i]);
                    SetSiblingsByDepth(bones, closedSiblings);
                }
            }
            else if (siblingConstraints == UnificationMode.Unified)
            {
                Queue<Bone> bones = new Queue<Bone>();
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    bones.Enqueue(m_Structures[i]);
                }
                if (bones.Count > 0) SetSiblingsByDepth(bones, closedSiblings);
            }
        }
        private void SetSiblingsByDepth(Queue<Bone> bones, bool closed)
        {
            Bone first = bones.Dequeue();
            for (int i = 0; i < first.childBones.Count; i++)
            {
                bones.Enqueue(first.childBones[i]);
            }
            Bone left = first;
            Bone right = null;
            while (bones.Count > 0)
            {
                right = bones.Dequeue();
                for (int i = 0; i < right.childBones.Count; i++)
                {
                    bones.Enqueue(right.childBones[i]);
                }
                if (left.depth == right.depth)
                {
                    // same depth
                    left.SetRightSibling(right);
                    right.SetLeftSibling(left);
                }
                else
                {
                    // connect the last node to the first of this tier
                    if (closed)
                    {
                        left.SetRightSibling(first);
                        first.SetLeftSibling(left);
                    }
                    // next depth
                    first = right;
                }
                left = right;
            }
            // connect the last node to the first of the last tier
            if (right != null && closed)
            {
                first.SetLeftSibling(right);
                right.SetRightSibling(first);
            }
        }
        private void SetTreeLength()
        {
            if (lengthUnification == UnificationMode.Rooted)
            {
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    m_Structures[i].SetTreeLength();
                }
            }
            else if (lengthUnification == UnificationMode.Unified)
            {
                float maxLength = 0;
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    maxLength = Mathf.Max(maxLength, m_Structures[i].treeLength);
                }
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    m_Structures[i].SetTreeLength(maxLength);
                }
            }
        }
        public void RefreshRadius()
        {
            globalRadius = transform.lossyScale.Abs().Max() * radius;
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].Inflate(globalRadius, radiusCurve);
            }
        }

        private void UpdateStructures(float deltaTime)
        {
            if (deltaTime <= DeltaTime_Min) return;

            // radius
            globalRadius = transform.lossyScale.Abs().Max() * radius;

            // parameters
            for (int j = 0; j < m_Structures.Count; j++)
            {
                m_Structures[j].Inflate(globalRadius, radiusCurve, sharedMaterial);
                if (simulateSpace != null) m_Structures[j].UpdateSpace();
            }

            // force
            globalForce = gravity;
            if (gravityAligner != null)
            {
                Vector3 alignedDir = gravityAligner.TransformDirection(gravity).normalized;
                Vector3 globalDir = gravity.normalized;
                float attenuation = Mathf.Acos(Vector3.Dot(alignedDir, globalDir)) / Mathf.PI;
                globalForce *= attenuation;
            }

            deltaTime /= iterations;
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < m_Structures.Count; j++)
                {
                    UpdateBones(m_Structures[j], deltaTime);
                }
            }
        }
        private void UpdateBones(Bone bone, float deltaTime)
        {
            if (bone.depth > startDepth)
            {
                Vector3 oldWorldPosition, newWorldPosition, expectedPosition;
                oldWorldPosition = newWorldPosition = bone.worldPosition;

                // Resistance (force resistance)
                Vector3 force = globalForce;
                if (forceModule != null && forceModule.isActiveAndEnabled)
                {
                    force += forceModule.GetForce(bone.normalizedLength) * forceScale;
                }
                if (customForce != null)
                {
                    force += customForce(bone.normalizedLength);
                }
                force.x *= transform.localScale.x;
                force.y *= transform.localScale.y;
                force.z *= transform.localScale.z;
                bone.speed += force * (1 - bone.resistance) / iterations;

                // Damping (inertia attenuation)
                bone.speed *= 1 - bone.damping;
                if (bone.speed.sqrMagnitude > sleepThreshold)
                {
                    newWorldPosition += bone.speed * deltaTime;
                }

                // Stiffness (shape keeper)
                Vector3 parentMovement = bone.parentBone.worldPosition - bone.parentBone.transform.position;
                expectedPosition = bone.parentBone.transform.TransformPoint(bone.localPosition) + parentMovement;
                newWorldPosition = Vector3.Lerp(newWorldPosition, expectedPosition, bone.stiffness / iterations);

                // Slackness (length keeper)
                // Length needs to be calculated with TransformVector to match runtime scaling
                Vector3 dirToParent = (newWorldPosition - bone.parentBone.worldPosition).normalized;
                float lengthToParent = bone.parentBone.transform.TransformVector(bone.localPosition).magnitude;
                expectedPosition = bone.parentBone.worldPosition + dirToParent * lengthToParent;
                int lengthConstraints = 1;
                // Sibling constraints
                if (siblingConstraints != UnificationMode.None)
                {
                    if (bone.leftBone != null)
                    {
                        Vector3 dirToLeft = (newWorldPosition - bone.leftBone.worldPosition).normalized;
                        float lengthToLeft = bone.transform.TransformVector(bone.leftPosition).magnitude;
                        expectedPosition += bone.leftBone.worldPosition + dirToLeft * lengthToLeft;
                        lengthConstraints++;
                    }
                    if (bone.rightBone != null)
                    {
                        Vector3 dirToRight = (newWorldPosition - bone.rightBone.worldPosition).normalized;
                        float lengthToRight = bone.transform.TransformVector(bone.rightPosition).magnitude;
                        expectedPosition += bone.rightBone.worldPosition + dirToRight * lengthToRight;
                        lengthConstraints++;
                    }
                }
                expectedPosition /= lengthConstraints;
                newWorldPosition = Vector3.Lerp(expectedPosition, newWorldPosition, bone.slackness / iterations);

                // Collision
                if (bone.radius > 0)
                {
                    foreach (EZSoftBoneColliderBase collider in EZSoftBoneColliderBase.EnabledColliders)
                    {
                        if (bone.transform != collider.transform && collisionLayers.Contains(collider.gameObject.layer))
                            collider.Collide(ref newWorldPosition, bone.radius);
                    }
                    foreach (Collider collider in extraColliders)
                    {
                        if (bone.transform != collider.transform && collider.enabled)
                            EZSoftBoneUtility.PointOutsideCollider(ref newWorldPosition, collider, bone.radius);
                    }
                }

                bone.speed = (bone.speed + (newWorldPosition - oldWorldPosition) / deltaTime) * 0.5f;
                bone.worldPosition = newWorldPosition;
            }
            else
            {
                bone.worldPosition = bone.transform.position;
            }

            for (int i = 0; i < bone.childBones.Count; i++)
            {
                UpdateBones(bone.childBones[i], deltaTime);
            }
        }
        private void UpdateTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].UpdateTransform(siblingRotationConstraints, startDepth);
            }
        }
    }
}
