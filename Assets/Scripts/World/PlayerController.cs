using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Utils;

public class PlayerController : DeCrawl.Primitives.FindingSingleton<PlayerController>
{
    [SerializeField, WithAction("SnapToFloorGrid", "Snap", "Snap position to ground")]
    bool snap;

    public void SnapToFloorGrid()
    {
        transform.position = DynamicGrid.SnapGridGroundCenter(transform.position);
    }
}
