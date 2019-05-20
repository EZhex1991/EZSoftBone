/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-20 10:45:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System.Collections.Generic;
using UnityEngine;

namespace EZUnity.PhysicsBone
{
    public abstract class EZPhysicsBoneColliderBase : MonoBehaviour
    {
        public static List<EZPhysicsBoneColliderBase> EnabledColliders = new List<EZPhysicsBoneColliderBase>();

        protected void OnEnable()
        {
            EnabledColliders.Add(this);
        }
        protected void OnDisable()
        {
            EnabledColliders.Remove(this);
        }

        public abstract void Collide(ref Vector3 position, float spacing);
    }
}
