﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Math
{
        public static class MathParabola
        {
        static Vector3 LastMid;
        static float LastTime;
            public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float currentTime, float MaxTime)
            {
                currentTime = currentTime / MaxTime;
                Func<float, float> f = x => -4 * height * x * x + 4 * height * x;
    //             // 
                var mid = Vector3.LerpUnclamped(start, end, currentTime);


                return new Vector3(mid.x, f(currentTime) + Mathf.LerpUnclamped(start.y, end.y, currentTime), mid.z);
                      
            }
    }
    
}
