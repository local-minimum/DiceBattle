using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static float Inclination(this Vector3 v) => Mathf.Atan2(v.y, Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.z, 2)));
}
