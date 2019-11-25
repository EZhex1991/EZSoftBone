/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-24 13:22:59
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace EZhex1991.EZSoftBone
{
    public class EZCurveRectAttribute : PropertyAttribute
    {
        public Rect rect;
        public Color color = Color.green;

        public EZCurveRectAttribute()
        {
            this.rect = new Rect(0, 0, 1, 1);
        }
        public EZCurveRectAttribute(Rect rect)
        {
            this.rect = rect;
        }
        public EZCurveRectAttribute(float x, float y, float width, float height)
        {
            this.rect = new Rect(x, y, width, height);
        }
        public EZCurveRectAttribute(Rect rect, Color color)
        {
            this.rect = rect;
            this.color = color;
        }
        public EZCurveRectAttribute(float x, float y, float width, float height, Color color)
        {
            this.rect = new Rect(x, y, width, height);
            this.color = color;
        }
    }
}
