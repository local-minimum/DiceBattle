using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWidthRatio : MonoBehaviour
{
    [SerializeField, Range(0, 10)]
    float aspect = 1;

    static bool AlmostEqual(float a, float b, float threshold = 1e-5f) => Mathf.Abs(a - b) < threshold;

    Vector2 ScaledWidth(float height) => new Vector2(height, height / aspect);

    private void Update()
    {
        var rt = transform as RectTransform;
        var size = rt.sizeDelta;
        if (!AlmostEqual(size.y / size.x, aspect))
        {
            rt.sizeDelta = ScaledWidth(size.y);
        }
    }
}
