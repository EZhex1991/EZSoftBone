/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-18 19:33:50
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public class EZSoftBone : MonoBehaviour
    {
        public static readonly double Delta_Min = 1e-6;

        public enum UnificationMode
        {
            None,
            Rooted,
            Unified,
        }

        private class Bone : IDisposable
        {
            public Bone parentBone;
            public Bone leftBone;
            public Bone rightBone;
            public List<Bone> childBones = new List<Bone>();

            public Transform transform;
            public Transform systemSpace;

            public int depth;
            public float boneLength;
            public float treeLength;
            public float normalizedLength;

            public float radius;
            public float damping;
            public float stiffness;
            public float resistance;
            public float slackness;

            public Vector3 worldPosition;
            public Vector3 systemPosition;
            public Vector3 speed;

            public Vector3 originalLocalPosition;
            public Quaternion originalLocalRotation = Quaternion.identity;
            public Vector3 positionToLeft;
            public Vector3 positionToRight;

            public Bone(Transform systemSpace, Transform transform, IEnumerable<Transform> endBones, int startDepth, int depth, float nodeLength, float boneLength)
            {
                this.transform = transform;
                this.systemSpace = systemSpace;
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                originalLocalPosition = transform.localPosition;
                originalLocalRotation = transform.localRotation;
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

            public void SetLeftSibling(Bone bone)
            {
                if (bone == this || bone == rightBone) return;
                leftBone = bone;
                positionToLeft = transform.InverseTransformVector(bone.worldPosition - worldPosition);
            }
            public void SetRightSibling(Bone bone)
            {
                if (bone == this || bone == leftBone) return;
                rightBone = bone;
                positionToRight = transform.InverseTransformVector(bone.worldPosition - worldPosition);
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

            public void RevertTransforms()
            {
                transform.localPosition = originalLocalPosition;
                transform.localRotation = originalLocalRotation;
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].RevertTransforms();
                }
            }
            public void UpdateTransform(bool siblingRotationConstraints)
            {
                if (childBones.Count == 1)
                {
                    Bone childBone = childBones[0];
                    transform.rotation *= Quaternion.FromToRotation(childBone.originalLocalPosition,
                                                                    transform.InverseTransformVector(childBone.worldPosition - worldPosition));

                    if (siblingRotationConstraints)
                    {
                        if (leftBone != null && rightBone != null)
                        {
                            Vector3 directionLeft0 = positionToLeft;
                            Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                            Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);

                            Vector3 directionRight0 = positionToRight;
                            Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                            Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);

                            transform.rotation *= Quaternion.Lerp(rotationLeft, rotationRight, 0.5f);
                        }
                        else if (leftBone != null)
                        {
                            Vector3 directionLeft0 = positionToLeft;
                            Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                            Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);
                            transform.rotation *= rotationLeft;
                        }
                        else if (rightBone != null)
                        {
                            Vector3 directionRight0 = positionToRight;
                            Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                            Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);
                            transform.rotation *= rotationRight;
                        }
                    }
                }

                transform.position = worldPosition;
                if (systemSpace != null) systemPosition = systemSpace.InverseTransformPoint(worldPosition);

                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].UpdateTransform(siblingRotationConstraints);
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

            public void Dispose()
            {
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].Dispose();
                }
                parentBone = null;
                leftBone = null;
                rightBone = null;
                childBones.Clear();
                transform = null;
            }
        }

        [SerializeField]
        private List<Transform> m_RootBones;
        public List<Transform> rootBones { get { return m_RootBones; } }
        [SerializeField]
        private List<Transform> m_EndBones;
        public List<Transform> endBones { get { return m_EndBones; } }

        [Header("Structure")]
        [SerializeField]
        private int m_StartDepth;
        public int startDepth { get { return m_StartDepth; } set { m_StartDepth = value; } }

        [SerializeField]
        private UnificationMode m_SiblingConstraints = UnificationMode.None;
        public UnificationMode siblingConstraints { get { return m_SiblingConstraints; } set { m_SiblingConstraints = value; } }
        [SerializeField]
        private UnificationMode m_LengthUnification = UnificationMode.None;
        public UnificationMode lengthUnification { get { return m_LengthUnification; } set { m_LengthUnification = value; } }

        [SerializeField]
        private bool m_SiblingRotationConstraints = true;
        public bool siblingRotationConstraints { get { return m_SiblingRotationConstraints; } set { m_SiblingRotationConstraints = value; } }

        [SerializeField]
        private bool m_ClosedSiblings = false;
        public bool closedSiblings { get { return m_ClosedSiblings; } set { m_ClosedSiblings = value; } }

        [Header("Collision")]
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

        [Header("Performance")]
        [SerializeField, Range(1, 10)]
        private int m_Iterations = 1;
        public int iterations { get { return m_Iterations; } set { m_Iterations = value; } }

        [SerializeField, EZNestedEditor]
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

        [SerializeField]
        private float m_SleepThreshold = 0.005f;
        public float sleepThreshold { get { return m_SleepThreshold; } set { m_SleepThreshold = Mathf.Max(0, value); } }

        [Header("Force")]
        [SerializeField]
        private Vector3 m_Gravity;
        public Vector3 gravity { get { return m_Gravity; } set { m_Gravity = value; } }
        [SerializeField, EZNestedEditor]
        private EZSoftBoneForce m_ForceModule;
        public EZSoftBoneForce forceModule { get { return m_ForceModule; } set { m_ForceModule = value; } }

        [Header("References")]
        [SerializeField]
        private Transform m_GravityAligner;
        public Transform gravityAligner { get { return m_GravityAligner; } set { m_GravityAligner = value; } }
        [SerializeField]
        private Transform m_ForceSpace;
        public Transform forceSpace { get { return m_ForceSpace; } set { m_ForceSpace = value; } }
        [SerializeField]
        private Transform m_SimulateSpace;
        public Transform simulateSpace { get { return m_SimulateSpace; } set { m_SimulateSpace = value; } }

        public float globalRadius { get; private set; }

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
            RevertTransforms();
        }
        private void LateUpdate()
        {
            UpdateStructures(Time.deltaTime);
            UpdateTransforms();
        }
        private void OnDisable()
        {
            RevertTransforms();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_StartDepth = Mathf.Max(0, m_StartDepth);
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
                forceModule.DrawGizmos(transform, forceSpace);
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
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].RevertTransforms();
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
            if (deltaTime <= Delta_Min) return;
            globalRadius = transform.lossyScale.Abs().Max() * radius;
            for (int j = 0; j < m_Structures.Count; j++)
            {
                m_Structures[j].Inflate(globalRadius, radiusCurve, sharedMaterial);
                if (simulateSpace != null) m_Structures[j].UpdateSpace();
            }

            deltaTime /= iterations;
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < m_Structures.Count; j++)
                {
                    UpdateNode(m_Structures[j], deltaTime);
                }
            }
        }
        private void UpdateNode(Bone bone, float deltaTime)
        {
            if (bone.depth > startDepth)
            {
                Vector3 oldWorldPosition, newWorldPosition;
                oldWorldPosition = newWorldPosition = bone.worldPosition;

                // Damping (inertia attenuation)
                if (bone.speed.sqrMagnitude < sleepThreshold)
                {
                    bone.speed = Vector3.zero;
                }
                else
                {
                    newWorldPosition += bone.speed * deltaTime * (1 - bone.damping);
                }

                // Resistance (force resistance)
                Vector3 force = gravity;
                if (gravityAligner != null)
                {
                    Vector3 alignedDir = gravityAligner.TransformDirection(gravity).normalized;
                    Vector3 globalDir = gravity.normalized;
                    float attenuation = Mathf.Acos(Vector3.Dot(alignedDir, globalDir)) / Mathf.PI;
                    force *= attenuation;
                }
                if (forceModule != null)
                {
                    force += forceModule.GetForce(bone.normalizedLength, forceSpace);
                }
                force.x *= transform.localScale.x;
                force.y *= transform.localScale.y;
                force.z *= transform.localScale.z;
                newWorldPosition += force * (1 - bone.resistance) / iterations;

                // Stiffness (shape keeper)
                Vector3 parentOffset = bone.parentBone.worldPosition - bone.parentBone.transform.position;
                Vector3 expectedPos = bone.parentBone.transform.TransformPoint(bone.originalLocalPosition) + parentOffset;
                newWorldPosition = Vector3.Lerp(newWorldPosition, expectedPos, bone.stiffness / iterations);

                // Slackness (length keeper)
                Vector3 nodeDir = (newWorldPosition - bone.parentBone.worldPosition).normalized;
                float nodeLength = bone.parentBone.transform.TransformVector(bone.originalLocalPosition).magnitude;
                nodeDir = bone.parentBone.worldPosition + nodeDir * nodeLength;
                // Siblings
                if (siblingConstraints != UnificationMode.None)
                {
                    int constraints = 1;
                    if (bone.leftBone != null)
                    {
                        Vector3 leftDir = (newWorldPosition - bone.leftBone.worldPosition).normalized;
                        float leftLength = bone.transform.TransformVector(bone.positionToLeft).magnitude;
                        leftDir = bone.leftBone.worldPosition + leftDir * leftLength;
                        nodeDir += leftDir;
                        constraints++;
                    }
                    if (bone.rightBone != null)
                    {
                        Vector3 rightDir = (newWorldPosition - bone.rightBone.worldPosition).normalized;
                        float rightLength = bone.transform.TransformVector(bone.positionToRight).magnitude;
                        rightDir = bone.rightBone.worldPosition + rightDir * rightLength;
                        nodeDir += rightDir;
                        constraints++;
                    }
                    nodeDir /= constraints;
                }
                newWorldPosition = Vector3.Lerp(nodeDir, newWorldPosition, bone.slackness / iterations);

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

                bone.speed = (newWorldPosition - oldWorldPosition) / deltaTime;
                bone.worldPosition = newWorldPosition;
            }
            else
            {
                bone.transform.localPosition = bone.originalLocalPosition;
                bone.worldPosition = bone.transform.position;
            }

            for (int i = 0; i < bone.childBones.Count; i++)
            {
                UpdateNode(bone.childBones[i], deltaTime);
            }
        }
        private void UpdateTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].UpdateTransform(siblingRotationConstraints);
            }
        }
    }
}
