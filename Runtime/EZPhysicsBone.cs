/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-18 19:33:50
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EZhex1991.EZPhysicsBone
{
    public class EZPhysicsBone : MonoBehaviour
    {
        public static readonly double Delta_Min = 1e-6;

        public enum SiblingConstraints
        {
            None,
            Root,
            Depth,
        }

        public class TreeNode : IDisposable
        {
            public TreeNode parent;
            public TreeNode leftSibling;
            public TreeNode rightSibling;
            public List<TreeNode> children = new List<TreeNode>();

            public Transform transform;
            public Transform systemSpace;

            public int depth;
            public float nodeLength;
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

            public TreeNode() { }
            public TreeNode(Transform systemSpace, Transform transform, int startDepth, int depth, float nodeLength, float boneLength)
            {
                if (transform == null) return;
                this.transform = transform;
                this.systemSpace = systemSpace;
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                originalLocalPosition = transform.localPosition;
                originalLocalRotation = transform.localRotation;
                this.depth = depth;
                this.nodeLength = nodeLength;
                if (depth > startDepth)
                {
                    this.boneLength = boneLength + this.nodeLength;
                }
                treeLength = this.boneLength;
                if (transform.childCount > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        TreeNode node = new TreeNode(systemSpace, child, startDepth, depth + 1, (child.position - transform.position).magnitude, this.boneLength);
                        node.parent = this;
                        children.Add(node);
                        treeLength = Mathf.Max(treeLength, node.treeLength);
                    }
                }
                normalizedLength = treeLength == 0 ? 0 : this.boneLength / treeLength;
            }

            public void SetLeftSibling(TreeNode node)
            {
                if (node == this || node == rightSibling) return;
                leftSibling = node;
                positionToLeft = transform.InverseTransformVector(node.worldPosition - worldPosition);
            }
            public void SetRightSibling(TreeNode node)
            {
                if (node == this || node == leftSibling) return;
                rightSibling = node;
                positionToRight = transform.InverseTransformVector(node.worldPosition - worldPosition);
            }

            public void Inflate(float baseRadius, AnimationCurve radiusCurve)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Inflate(baseRadius, radiusCurve);
                }
            }
            public void Inflate(float baseRadius, AnimationCurve radiusCurve, EZPBMaterial material)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                damping = material.GetDamping(normalizedLength);
                stiffness = material.GetStiffness(normalizedLength);
                resistance = material.GetResistance(normalizedLength);
                slackness = material.GetSlackness(normalizedLength);
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Inflate(baseRadius, radiusCurve, material);
                }
            }

            public void RevertTransforms()
            {
                transform.localPosition = originalLocalPosition;
                transform.localRotation = originalLocalRotation;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].RevertTransforms();
                }
            }
            public void ApplyToTransform(bool siblingRotationConstraints)
            {
                if (children.Count == 1)
                {
                    TreeNode child = children[0];
                    transform.rotation *= Quaternion.FromToRotation(child.originalLocalPosition,
                                                                    transform.InverseTransformVector(child.worldPosition - worldPosition));

                    if (siblingRotationConstraints)
                    {
                        if (leftSibling != null && rightSibling != null)
                        {
                            Vector3 directionLeft0 = positionToLeft;
                            Vector3 directionLeft1 = transform.InverseTransformVector(leftSibling.worldPosition - worldPosition);
                            Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);

                            Vector3 directionRight0 = positionToRight;
                            Vector3 directionRight1 = transform.InverseTransformVector(rightSibling.worldPosition - worldPosition);
                            Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);

                            transform.rotation *= Quaternion.Lerp(rotationLeft, rotationRight, 0.5f);
                        }
                        else if (leftSibling != null)
                        {
                            Vector3 directionLeft0 = positionToLeft;
                            Vector3 directionLeft1 = transform.InverseTransformVector(leftSibling.worldPosition - worldPosition);
                            Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);
                            transform.rotation *= rotationLeft;
                        }
                        else if (rightSibling != null)
                        {
                            Vector3 directionRight0 = positionToRight;
                            Vector3 directionRight1 = transform.InverseTransformVector(rightSibling.worldPosition - worldPosition);
                            Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);
                            transform.rotation *= rotationRight;
                        }
                    }
                }

                transform.position = worldPosition;
                if (systemSpace != null) systemPosition = systemSpace.InverseTransformPoint(worldPosition);

                for (int i = 0; i < children.Count; i++)
                {
                    children[i].ApplyToTransform(siblingRotationConstraints);
                }
            }

            public void ResetSystem()
            {
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                speed = Vector3.zero;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].ResetSystem();
                }
            }
            public void UpdateSpace()
            {
                if (systemSpace == null) return;
                worldPosition = systemSpace.TransformPoint(systemPosition);
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].UpdateSpace();
                }
            }

            public void Dispose()
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Dispose();
                }
                parent = null;
                leftSibling = null;
                rightSibling = null;
                children.Clear();
                transform = null;
            }
        }

        [SerializeField]
        private List<Transform> m_RootBones;
        public List<Transform> rootBones { get { return m_RootBones; } }

        [Header("Structure")]
        [SerializeField]
        private int m_StartDepth;
        public int startDepth { get { return m_StartDepth; } }

        [SerializeField]
        private SiblingConstraints m_SiblingConstraints = SiblingConstraints.None;
        public SiblingConstraints siblingConstraints { get { return m_SiblingConstraints; } }

        [SerializeField]
        private bool m_SiblingRotationConstraints = true;
        public bool siblingRotationConstraints { get { return m_SiblingRotationConstraints; } }

        [SerializeField]
        private bool m_ClosedSiblings = false;
        public bool closedSiblings { get { return m_ClosedSiblings; } }

        [Header("Performance")]
        [SerializeField, Range(1, 10)]
        private int m_Iterations = 1;
        public int iterations { get { return m_Iterations; } }

        [SerializeField]
        private EZPBMaterial m_Material;
        private EZPBMaterial m_InstanceMaterial;
        public EZPBMaterial sharedMaterial
        {
            get
            {
                if (m_Material == null)
                    m_Material = EZPBMaterial.defaultMaterial;
                return m_Material;
            }
            set
            {
                m_Material = value;
            }
        }
        public EZPBMaterial material
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

        [Header("Collision")]
        [SerializeField]
        private LayerMask m_CollisionLayers = 0;
        public LayerMask collisionLayers { get { return m_CollisionLayers; } }
        [SerializeField]
        private List<Collider> m_ExtraColliders = new List<Collider>();
        public List<Collider> extraColliders { get { return m_ExtraColliders; } }
        [SerializeField]
        private float m_Radius = 0;
        public float radius { get { return m_Radius; } }
        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_RadiusCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve radiusCurve { get { return m_RadiusCurve; } }

        [Header("Force")]
        [SerializeField]
        private Vector3 m_Gravity;
        public Vector3 gravity { get { return m_Gravity; } set { m_Gravity = value; } }
        [SerializeField]
        private EZPBForce m_ForceModule;
        public EZPBForce forceModule { get { return m_ForceModule; } set { m_ForceModule = value; } }

        [Header("References")]
        [SerializeField]
        private Transform m_GravityAligner;
        public Transform gravityAligner { get { return m_GravityAligner; } }
        [SerializeField]
        private Transform m_SimulateSpace;
        public Transform simulateSpace { get { return m_SimulateSpace; } }

        public float globalRadius { get; private set; }

        private List<TreeNode> m_PhysicsTrees = new List<TreeNode>();

        private void Start()
        {
            InitPhysicsTrees();
        }
        private void OnEnable()
        {
            ResetSystem();
        }
        private void LateUpdate()
        {
            RevertTransforms();
            UpdatePhysicsTrees(Time.deltaTime);
            ApplyPhysicsTrees();
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
            if (Application.isPlaying)
            {
                RevertTransforms();
                InitPhysicsTrees();
            }
            else
            {
                InitPhysicsTrees();
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            if (!Application.isPlaying && transform.hasChanged)
            {
                InitPhysicsTrees();
            }

            for (int i = 0; i < m_PhysicsTrees.Count; i++)
            {
                DrawNodeGizmos(m_PhysicsTrees[i]);
            }
        }
#endif

        private void InitPhysicsTrees()
        {
            m_PhysicsTrees.Clear();
            if (rootBones == null || rootBones.Count == 0) return;
            globalRadius = transform.lossyScale.Abs().Max() * radius;
            for (int i = 0; i < rootBones.Count; i++)
            {
                if (rootBones[i] == null) continue;
                TreeNode tree = new TreeNode(simulateSpace, rootBones[i], startDepth, 0, 0, 0);
                tree.Inflate(globalRadius, radiusCurve);
                m_PhysicsTrees.Add(tree);
            }
            if (siblingConstraints == SiblingConstraints.Root)
            {
                for (int i = 0; i < m_PhysicsTrees.Count; i++)
                {
                    Queue<TreeNode> nodes = new Queue<TreeNode>();
                    nodes.Enqueue(m_PhysicsTrees[i]);
                    SetSiblingsByDepth(nodes, closedSiblings);
                }
            }
            else if (siblingConstraints == SiblingConstraints.Depth)
            {
                Queue<TreeNode> nodes = new Queue<TreeNode>();
                for (int i = 0; i < m_PhysicsTrees.Count; i++)
                {
                    nodes.Enqueue(m_PhysicsTrees[i]);
                }
                if (nodes.Count > 0) SetSiblingsByDepth(nodes, closedSiblings);
            }
        }
        private void SetSiblingsByDepth(Queue<TreeNode> nodes, bool closed)
        {
            TreeNode first = nodes.Dequeue();
            for (int i = 0; i < first.children.Count; i++)
            {
                nodes.Enqueue(first.children[i]);
            }
            TreeNode left = first;
            TreeNode right = null;
            while (nodes.Count > 0)
            {
                right = nodes.Dequeue();
                for (int i = 0; i < right.children.Count; i++)
                {
                    nodes.Enqueue(right.children[i]);
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

        private void ResetSystem()
        {
            for (int i = 0; i < m_PhysicsTrees.Count; i++)
            {
                m_PhysicsTrees[i].ResetSystem();
            }
        }
        private void RevertTransforms()
        {
            for (int i = 0; i < m_PhysicsTrees.Count; i++)
            {
                m_PhysicsTrees[i].RevertTransforms();
            }
        }

        private void UpdatePhysicsTrees(float deltaTime)
        {
            if (deltaTime <= Delta_Min) return;
            globalRadius = transform.lossyScale.Abs().Max() * radius;
            for (int j = 0; j < m_PhysicsTrees.Count; j++)
            {
                m_PhysicsTrees[j].Inflate(globalRadius, radiusCurve, sharedMaterial);
                if (simulateSpace != null) m_PhysicsTrees[j].UpdateSpace();
            }

            deltaTime /= iterations;
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < m_PhysicsTrees.Count; j++)
                {
                    UpdateNode(m_PhysicsTrees[j], deltaTime);
                }
            }
        }
        private void UpdateNode(TreeNode node, float deltaTime)
        {
            if (node.depth > startDepth)
            {
                Vector3 oldWorldPosition, newWorldPosition;
                oldWorldPosition = newWorldPosition = node.worldPosition;

                // Damping (inertia attenuation)
                if (node.speed.sqrMagnitude < sleepThreshold)
                {
                    node.speed = Vector3.zero;
                }
                else
                {
                    newWorldPosition += node.speed * deltaTime * (1 - node.damping);
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
                    force += forceModule.GetForce(node.normalizedLength);
                }
                force.x *= transform.localScale.x;
                force.y *= transform.localScale.y;
                force.z *= transform.localScale.z;
                newWorldPosition += force * (1 - node.resistance) / iterations;

                // Stiffness (shape keeper)
                Vector3 parentOffset = node.parent.worldPosition - node.parent.transform.position;
                Vector3 expectedPos = node.parent.transform.TransformPoint(node.originalLocalPosition) + parentOffset;
                newWorldPosition = Vector3.Lerp(newWorldPosition, expectedPos, node.stiffness / iterations);

                // Slackness (length keeper)
                Vector3 nodeDir = (newWorldPosition - node.parent.worldPosition).normalized;
                float nodeLength = node.parent.transform.TransformVector(node.originalLocalPosition).magnitude;
                nodeDir = node.parent.worldPosition + nodeDir * nodeLength;
                // Siblings
                if (siblingConstraints != SiblingConstraints.None)
                {
                    int constraints = 1;
                    if (node.leftSibling != null)
                    {
                        Vector3 leftDir = (newWorldPosition - node.leftSibling.worldPosition).normalized;
                        float leftLength = node.transform.TransformVector(node.positionToLeft).magnitude;
                        leftDir = node.leftSibling.worldPosition + leftDir * leftLength;
                        nodeDir += leftDir;
                        constraints++;
                    }
                    if (node.rightSibling != null)
                    {
                        Vector3 rightDir = (newWorldPosition - node.rightSibling.worldPosition).normalized;
                        float rightLength = node.transform.TransformVector(node.positionToRight).magnitude;
                        rightDir = node.rightSibling.worldPosition + rightDir * rightLength;
                        nodeDir += rightDir;
                        constraints++;
                    }
                    nodeDir /= constraints;
                }
                newWorldPosition = Vector3.Lerp(nodeDir, newWorldPosition, node.slackness / iterations);

                // Collision
                if (node.radius > 0)
                {
                    foreach (EZPBColliderBase collider in EZPBColliderBase.EnabledColliders)
                    {
                        if (node.transform != collider.transform && collisionLayers.Contains(collider.gameObject.layer))
                            collider.Collide(ref newWorldPosition, node.radius);
                    }
                    foreach (Collider collider in extraColliders)
                    {
                        if (node.transform != collider.transform && collider.enabled)
                            EZPhysicsBoneUtility.PointOutsideCollider(ref newWorldPosition, collider, node.radius);
                    }
                }

                node.speed = (newWorldPosition - oldWorldPosition) / deltaTime;
                node.worldPosition = newWorldPosition;
            }
            else
            {
                node.transform.localPosition = node.originalLocalPosition;
                node.worldPosition = node.transform.position;
            }

            for (int i = 0; i < node.children.Count; i++)
            {
                UpdateNode(node.children[i], deltaTime);
            }
        }

        private void ApplyPhysicsTrees()
        {
            for (int i = 0; i < m_PhysicsTrees.Count; i++)
            {
                m_PhysicsTrees[i].ApplyToTransform(siblingRotationConstraints);
            }
        }

        private void DrawNodeGizmos(TreeNode node)
        {
            for (int i = 0; i < node.children.Count; i++)
            {
                DrawNodeGizmos(node.children[i]);
            }
            Gizmos.color = Color.Lerp(Color.white, Color.red, node.normalizedLength);
            if (node.depth > startDepth)
                Gizmos.DrawWireSphere(node.worldPosition, node.radius);
            if (node.parent != null)
                Gizmos.DrawLine(node.parent.worldPosition, node.worldPosition);
            if (siblingConstraints != SiblingConstraints.None)
            {
                if (node.leftSibling != null)
                    Gizmos.DrawLine(node.leftSibling.worldPosition, node.worldPosition);
                if (node.rightSibling != null)
                    Gizmos.DrawLine(node.rightSibling.worldPosition, node.worldPosition);
            }
        }
    }
}
