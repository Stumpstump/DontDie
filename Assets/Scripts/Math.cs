using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Math
{
        public static class MathParabola
        {
            public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float currentTime, float MaxTime)
            {

                currentTime = currentTime / MaxTime;
                Func<float, float> f = x => -4 * height * x * x + 4 * height * x;
    //             // 
                var mid = Vector3.Lerp(start, end, currentTime);
            if (currentTime > 1)
                mid = end * currentTime;

                return new Vector3(mid.x, f(currentTime) + Mathf.Lerp(start.y, end.y, currentTime), mid.z);
                      
            }
    }
    
}
