using System.Collections;
using System.Collections.Generic;

namespace Samples
{
    public static class VectorUtils
    {
        public static AillieoUtils.Vector2 ToAVec(this UnityEngine.Vector2 v)
        {
            return new AillieoUtils.Vector2(v.x, v.y);
        }

        public static UnityEngine.Vector2 ToUVec2(this AillieoUtils.Vector2 v)
        {
            return new UnityEngine.Vector2(v.x, v.y);
        }

        public static AillieoUtils.Vector2 ToAVec(this UnityEngine.Vector3 v)
        {
            return new AillieoUtils.Vector2(v.x, v.z);
        }

        public static UnityEngine.Vector3 ToUVec3(this AillieoUtils.Vector2 v, float y = 0)
        {
            return new UnityEngine.Vector3(v.x, y, v.y);
        }
    }

}

