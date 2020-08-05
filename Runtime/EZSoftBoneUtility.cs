/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-18 20:43:20
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public static class EZSoftBoneUtility
    {
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        public static float Max(this Vector3 v)
        {
            return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
        }

        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask | (1 << layer)) == mask;
        }

        public static void GetCapsuleParams(CapsuleCollider collider, out Vector3 center0, out Vector3 center1, out float radius)
        {
            Vector3 scale = collider.transform.lossyScale.Abs();
            radius = collider.radius;
            center0 = center1 = collider.center;
            float height = collider.height * 0.5f;
            switch (collider.direction)
            {
                case 0:
                    radius *= Mathf.Max(scale.y, scale.z);
                    height = Mathf.Max(0, height - radius / scale.x);
                    center0.x -= height;
                    center1.x += height;
                    break;
                case 1:
                    radius *= Mathf.Max(scale.x, scale.z);
                    height = Mathf.Max(0, height - radius / scale.y);
                    center0.y -= height;
                    center1.y += height;
                    break;
                case 2:
                    radius *= Mathf.Max(scale.x, scale.y);
                    height = Mathf.Max(0, height - radius / scale.z);
                    center0.z -= height;
                    center1.z += height;
                    break;
            }
            center0 = collider.transform.TransformPoint(center0);
            center1 = collider.transform.TransformPoint(center1);
        }
        public static void GetCylinderParams(Transform transform, out Vector3 center, out Vector3 direction, out float radius, out float height)
        {
            Vector3 size = transform.lossyScale.Abs();
            center = transform.position;
            direction = transform.up;
            radius = Mathf.Max(size.x, size.z) * 0.5f;
            height = size.y;
        }

        public static void PointOutsideSphere(ref Vector3 position, SphereCollider collider, float spacing)
        {
            Vector3 scale = collider.transform.lossyScale.Abs();
            float radius = collider.radius * scale.Max();
            PointOutsideSphere(ref position, collider.transform.TransformPoint(collider.center), radius + spacing);
        }
        private static void PointOutsideSphere(ref Vector3 position, Vector3 spherePosition, float radius)
        {
            Vector3 bounceDir = position - spherePosition;
            if (bounceDir.magnitude < radius)
            {
                position = spherePosition + bounceDir.normalized * radius;
            }
        }

        public static void PointInsideSphere(ref Vector3 position, SphereCollider collider, float spacing)
        {
            PointInsideSphere(ref position, collider.transform.TransformPoint(collider.center), collider.radius - spacing);
        }
        private static void PointInsideSphere(ref Vector3 position, Vector3 spherePosition, float radius)
        {
            Vector3 bounceDir = position - spherePosition;
            if (bounceDir.magnitude > radius)
            {
                position = spherePosition + bounceDir.normalized * radius;
            }
        }

        public static void PointOutsideCapsule(ref Vector3 position, CapsuleCollider collider, float spacing)
        {
            Vector3 center0, center1;
            float radius;
            GetCapsuleParams(collider, out center0, out center1, out radius);
            PointOutsideCapsule(ref position, center0, center1, radius + spacing);
        }
        private static void PointOutsideCapsule(ref Vector3 position, Vector3 center0, Vector3 center1, float radius)
        {
            Vector3 capsuleDir = center1 - center0;
            Vector3 pointDir = position - center0;

            float dot = Vector3.Dot(capsuleDir, pointDir);
            if (dot <= 0)
            {
                PointOutsideSphere(ref position, center0, radius);
            }
            else if (dot >= capsuleDir.sqrMagnitude)
            {
                PointOutsideSphere(ref position, center1, radius);
            }
            else
            {
                Vector3 bounceDir = pointDir - Vector3.Project(pointDir, capsuleDir);
                float bounceDis = radius - bounceDir.magnitude;
                if (bounceDis > 0)
                {
                    position += bounceDir.normalized * bounceDis;
                }
            }
        }

        public static void PointInsideCapsule(ref Vector3 position, CapsuleCollider collider, float spacing)
        {
            Vector3 center0, center1;
            float radius;
            GetCapsuleParams(collider, out center0, out center1, out radius);
            PointInsideCapsule(ref position, center0, center1, radius - spacing);
        }
        private static void PointInsideCapsule(ref Vector3 position, Vector3 center0, Vector3 center1, float radius)
        {
            Vector3 capsuleDir = center1 - center0;
            Vector3 pointDir = position - center0;

            float dot = Vector3.Dot(capsuleDir, pointDir);
            if (dot <= 0)
            {
                PointInsideSphere(ref position, center0, radius);
            }
            else if (dot >= capsuleDir.sqrMagnitude)
            {
                PointInsideSphere(ref position, center1, radius);
            }
            else
            {
                Vector3 bounceDir = pointDir - Vector3.Project(pointDir, capsuleDir);
                float bounceDis = radius - bounceDir.magnitude;
                if (bounceDis < 0)
                {
                    position += bounceDir.normalized * bounceDis;
                }
            }
        }

        public static void PointOutsideCylinder(ref Vector3 position, Transform transform, float spacing)
        {
            Vector3 center, direction;
            float radius, height;
            GetCylinderParams(transform, out center, out direction, out radius, out height);
            PointOutsideCylinder(ref position, center, direction, radius + spacing, height + spacing);
        }
        private static void PointOutsideCylinder(ref Vector3 position, Vector3 center, Vector3 direction, float radius, float height)
        {
            Vector3 pointDir = position - center;
            Vector3 directionAlong = Vector3.Project(pointDir, direction);
            float distanceAlong = height - directionAlong.magnitude;
            if (distanceAlong > 0)
            {
                Vector3 directionSide = pointDir - directionAlong;
                float distanceSide = radius - directionSide.magnitude;
                if (distanceSide > 0)
                {
                    if (distanceSide < distanceAlong)
                    {
                        position += directionSide.normalized * distanceSide;
                    }
                    else
                    {
                        position += directionAlong.normalized * distanceAlong;
                    }
                }
            }
        }

        public static void PointInsideCylinder(ref Vector3 position, Transform transform, float spacing)
        {
            Vector3 center, direction;
            float radius, height;
            GetCylinderParams(transform, out center, out direction, out radius, out height);
            PointInsideCylinder(ref position, center, direction, radius - spacing, height - spacing);
        }
        private static void PointInsideCylinder(ref Vector3 position, Vector3 center, Vector3 direction, float radius, float height)
        {
            Vector3 pointDir = position - center;
            Vector3 directionAlong = Vector3.Project(pointDir, direction);
            float distanceAlong = height - directionAlong.magnitude;
            Vector3 directionSide = pointDir - directionAlong;
            float distanceSide = radius - directionSide.magnitude;
            if (distanceAlong < 0 || distanceSide < 0)
            {
                if (distanceSide < distanceAlong)
                {
                    position += directionSide.normalized * distanceSide;
                }
                else
                {
                    position += directionAlong.normalized * distanceAlong;
                }
            }
        }

        public static void PointOutsideBox(ref Vector3 position, BoxCollider collider, float spacing)
        {
            Vector3 positionToCollider = collider.transform.InverseTransformPoint(position) - collider.center;
            PointOutsideBox(ref positionToCollider, collider.size.Abs() / 2 + collider.transform.InverseTransformVector(Vector3.one * spacing).Abs());
            position = collider.transform.TransformPoint(collider.center + positionToCollider);
        }
        private static void PointOutsideBox(ref Vector3 position, Vector3 boxSize)
        {
            Vector3 distanceToCenter = position.Abs();
            if (distanceToCenter.x < boxSize.x && distanceToCenter.y < boxSize.y && distanceToCenter.z < boxSize.z)
            {
                Vector3 distance = (distanceToCenter - boxSize).Abs();
                if (distance.x < distance.y)
                {
                    if (distance.x < distance.z)
                    {
                        position.x = Mathf.Sign(position.x) * boxSize.x;
                    }
                    else
                    {
                        position.z = Mathf.Sign(position.z) * boxSize.z;
                    }
                }
                else
                {
                    if (distance.y < distance.z)
                    {
                        position.y = Mathf.Sign(position.y) * boxSize.y;
                    }
                    else
                    {
                        position.z = Mathf.Sign(position.z) * boxSize.z;
                    }
                }
            }
        }

        public static void PointInsideBox(ref Vector3 position, BoxCollider collider, float spacing)
        {
            Vector3 positionToCollider = collider.transform.InverseTransformPoint(position) - collider.center;
            PointInsideBox(ref positionToCollider, collider.size.Abs() / 2 - collider.transform.InverseTransformVector(Vector3.one * spacing).Abs());
            position = collider.transform.TransformPoint(collider.center + positionToCollider);
        }
        private static void PointInsideBox(ref Vector3 position, Vector3 boxSize)
        {
            Vector3 distanceToCenter = position.Abs();
            if (distanceToCenter.x > boxSize.x) position.x = Mathf.Sign(position.x) * boxSize.x;
            if (distanceToCenter.y > boxSize.y) position.y = Mathf.Sign(position.y) * boxSize.y;
            if (distanceToCenter.z > boxSize.z) position.z = Mathf.Sign(position.z) * boxSize.z;
        }

        public static void PointOutsideCollider(ref Vector3 position, Collider collider, float spacing)
        {
            Vector3 closestPoint = collider.ClosestPoint(position);
            if (position == closestPoint) // inside collider
            {
                Vector3 bounceDir = position - collider.bounds.center;
                Debug.DrawLine(collider.bounds.center, closestPoint, Color.red);
                position = closestPoint + bounceDir.normalized * spacing;
            }
            else
            {
                Vector3 bounceDir = position - closestPoint;
                if (bounceDir.magnitude < spacing)
                {
                    position = closestPoint + bounceDir.normalized * spacing;
                }
            }
        }

        public static void DrawGizmosArrow(Vector3 startPoint, Vector3 direction, float halfWidth, Vector3 normal)
        {
            Vector3 sideDir = Vector3.Cross(direction, normal).normalized * halfWidth;
            Vector3[] vertices = new Vector3[8];
            vertices[0] = startPoint + sideDir * 0.5f;
            vertices[1] = vertices[0] + direction * 0.5f;
            vertices[2] = vertices[1] + sideDir * 0.5f;
            vertices[3] = startPoint + direction;
            vertices[4] = startPoint - sideDir + direction * 0.5f;
            vertices[5] = vertices[4] + sideDir * 0.5f;
            vertices[6] = startPoint - sideDir * 0.5f;
            vertices[7] = vertices[0];
            DrawGizmosPolyLine(vertices);
        }
        public static void DrawGizmosPolyLine(params Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            }
        }
    }
}
