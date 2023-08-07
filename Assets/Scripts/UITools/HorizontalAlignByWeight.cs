using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class HorizontalAlignByWeight : MonoBehaviour
{
    AlignmentWeight[] weights;

    [SerializeField, Range(-1, 1)]
    float anchorTop = 0.99f;

    [SerializeField, Range(-1, 1)]
    float anchorBottom = 0.01f;

    [SerializeField, Range(-1, 1)]
    float anchorLeft = 0.01f;

    [SerializeField, Range(-1, 1)]
    float anchorRight = 0.99f;

    [SerializeField, Range(0, 1)]
    float spacing = 0.01f;

    void Start()
    {
        weights = GetComponentsInChildren<AlignmentWeight>();
    }

    private void OnValidate()
    {
        weights = GetComponentsInChildren<AlignmentWeight>();
    }

    float TotalWeight => weights.Where(w => w.gameObject.activeSelf).Sum(w => w.weight);
    float TotalSpacings => (weights.Count(w => w.gameObject.activeSelf) - 1) * spacing;

    void Update()
    {
        var total = TotalWeight;
        var totalWidth = anchorRight - anchorLeft;
        var totalSpacing = TotalSpacings;
        var freeWidth = totalWidth - totalSpacing;
        var scale = freeWidth / total;

        float left = anchorLeft;

        foreach (var s in weights.Where(w => w.gameObject.activeSelf).Select(w => new { rt = w.transform as RectTransform, width = w.weight * scale}))
        {
            s.rt.anchorMin = new Vector2(left, anchorBottom);
            left += s.width;
            s.rt.anchorMax = new Vector2(left, anchorTop);
            left += spacing;
        }

    }
}
