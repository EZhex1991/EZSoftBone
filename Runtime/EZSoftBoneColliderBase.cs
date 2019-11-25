/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-20 10:45:33
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System.Collections.Generic;
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public abstract class EZSoftBoneColliderBase : MonoBehaviour
    {
        public static HashSet<EZSoftBoneColliderBase> EnabledColliders = new HashSet<EZSoftBoneColliderBase>();

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
