using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField]
    GameObject Root;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    [SerializeField, Range(0, 1)]
    float anchorPoint = 0.4f;

    [SerializeField, Range(0, 2)]
    float anchorYOffset = 0.5f;

    private void OnEnable()
    {
        HideTooltip();
        TooltipedItem.OnTooltip += TooltipedItem_OnTooltip;        
    }

    private void OnDisable()
    {
        TooltipedItem.OnTooltip -= TooltipedItem_OnTooltip;
    }

    Vector3 Anchor(Vector2 canvasSize, Vector3[] rectCorners, out bool above, out bool right)
    {
        var centerY = canvasSize.y / 2;
        var seed = rectCorners.Min(c => Mathf.Abs(c.y - centerY));
        var line = rectCorners.OrderBy(c => Mathf.Abs(c.y - centerY)).Take(2).OrderBy(c => c.x).ToArray();

        var centerX = canvasSize.x / 2;

        var t = (line[0].x + line[1].x) / 2 < centerX ? 1f - anchorPoint : anchorPoint;

        above = line[0].y < centerY;
        right = line[0].x > centerY;
        return Vector3.Lerp(line[0], line[1], t);
    }

    private void TooltipedItem_OnTooltip(TooltipedItem tooltip, bool show)
    {
        if (show)
        {
            transform.SetAsLastSibling();
            TextUI.text = tooltip.Tooltip;

            var canvas = GetComponentInParent<Canvas>();
            var canvasSize = (canvas.transform as RectTransform).sizeDelta;
            var corners = new Vector3[4];
            (tooltip.transform as RectTransform).GetWorldCorners(corners);

            bool above;
            bool right;
            transform.position = Anchor(canvasSize, corners, out above, out right);

            var pivot = new Vector2(right ? 1 : 0, above ? -anchorYOffset : 1 + anchorYOffset);
            var rootRT = Root.transform as RectTransform;
            rootRT.pivot = pivot;
            Root.SetActive(true);
        } else
        {
            HideTooltip();
        }
    }

    void HideTooltip()
    {
        Root.SetActive(false);
    }
}
