using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScalarExtensions{
    public static int Mod(this int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    public static float Mod(this float x, float m)
    {
        float r = x % m;
        return r < 0 ? r + m : r;
    }
    public static float Mod(this float x, int m)
    {
        float r = x % m;
        return r < 0 ? r + m : r;
    }
}
