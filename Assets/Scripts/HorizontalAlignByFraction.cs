using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HorizontalAlignByFraction : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    float itemWidthFraction = 0.3f;

    [SerializeField]
    float itemSpacingFraction = 0.05f;

    [SerializeField]
    bool center = true;

    private void Update()
    {
        var rt = transform as RectTransform;
        if (center)
        {
            Center(rt);
        } else
        {
            Left(rt);
        }
    }

    float TotalWidth(RectTransform rt)
    {
        return rt.childCount * itemWidthFraction + (rt.childCount - 1) * itemSpacingFraction;
    }

    void Center(RectTransform rt)
    {
        var pos = 0.5f - TotalWidth(rt) / 2f;
        for (int i=0, l = rt.childCount; i<l; i++)
        {
            var child = rt.GetChild(i) as RectTransform;
            child.anchorMin = new Vector2(pos, 0f);
            child.anchorMax = new Vector2(pos + itemWidthFraction, 1f);
            pos += itemWidthFraction + itemSpacingFraction;
        }
    }

    void Left(RectTransform rt)
    {
        var pos = 0f;
        for (int i=0, l = rt.childCount; i<l; i++)
        {
            var child = rt.GetChild(i) as RectTransform;
            child.anchorMin = new Vector2(pos, 0f);
            child.anchorMax = new Vector2(pos + itemWidthFraction, 1f);
            pos += itemWidthFraction + itemSpacingFraction;
        }
    }
}
