using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FixedWidthRatio : MonoBehaviour
{
    [SerializeField, Range(0, 10)]
    float aspect = 1;

    static bool AlmostEqual(float a, float b, float threshold = 1e-5f) => Mathf.Abs(a - b) < threshold;

    private void Update()
    {
        var rt = transform as RectTransform;
        var size = rt.rect.size;

        if (!AlmostEqual(size.x / size.y, aspect))
        {
            var sizeDelta = rt.sizeDelta;

            rt.sizeDelta = new Vector2(size.y * aspect, sizeDelta.y);
        }
    }
}
