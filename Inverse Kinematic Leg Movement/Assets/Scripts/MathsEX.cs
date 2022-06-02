using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathsEX {
    
    //useful maths functions

    public const float TAU = 6.28318530718f;
    public static Vector2 GetVectorFromAngle(float a_angleRad) {
        return new Vector2(Mathf.Cos(a_angleRad), Mathf.Sin(a_angleRad));
    }
}
