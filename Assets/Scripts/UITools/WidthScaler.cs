using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WidthScaler : MonoBehaviour
{
    RectTransform rt;
    Vector2 originalSize;

    [SerializeField]
    float maxWidth = -1;

    [SerializeField]
    float minWidth = -1;

    [SerializeField]
    float minWidthFraction = -1;

    [SerializeField]
    float maxWidthFraction = -1;

    Canvas canvas;

    static float OptionalClamp(float value, float lower, float upper)
    {
        if (lower >= 0)
        {
            value = Mathf.Max(value, lower);
        }
        if (upper >= 0)
        {
            value = Mathf.Min(value, upper);
        }
        return value;
    }

    static bool AlmostEqual(float a, float b) => Mathf.Abs(a - b) < 0.0001;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rt = transform as RectTransform;
        originalSize = rt.sizeDelta;
    }

    Vector2 SizeDeltaFromWidth(float width) => new Vector2(width, originalSize.y / originalSize.x * width);

    void Sync()
    {
        var canvasWidth = (canvas.transform as RectTransform).sizeDelta.x;
        var currentWidth = Mathf.Max(rt.sizeDelta.x, originalSize.x);
        var currentScale = currentWidth / canvasWidth;
        var scaled = OptionalClamp(currentScale, minWidthFraction, maxWidthFraction);
        if (AlmostEqual(scaled, currentScale))
        {
            var newWidth = OptionalClamp(currentWidth, minWidth, maxWidth);
            if (!AlmostEqual(newWidth, currentWidth))
            {
                rt.sizeDelta = SizeDeltaFromWidth(newWidth);
            }
        } else
        {
            var newWidth = canvasWidth * scaled;
            rt.sizeDelta = SizeDeltaFromWidth(newWidth);
        }
    }

    private void Update()
    {
        Sync();
    }
}
