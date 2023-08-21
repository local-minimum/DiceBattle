using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicGrid : DeCrawl.Primitives.FindingSingleton<DynamicGrid>
{
    [SerializeField]
    Vector3 gridSize = Vector3.one * 3;

    [SerializeField]
    bool flexibleY = true;

    [SerializeField]
    Vector3 gridOrigin = Vector3.zero;

    public static Vector3 GridSize => instance.gridSize;
    public static Vector3 HalfSize => instance.gridSize * 0.5f;

    public static float HalfHeight => instance.gridSize.y * 0.5f;

    public static Vector3 GetXZOffset(int x, int z) => new Vector3(instance.gridSize.x * x, 0, instance.gridSize.z * z);

    public static Vector3 SnapGridCenter(Vector3 worldPosition, out int height)
    {
        var origin = instance.gridOrigin;
        var offset = (worldPosition - origin);
        var size = GridSize;
        var terrainFloor = Terrain.activeTerrain.SampleHeight(worldPosition) + Terrain.activeTerrain.GetPosition().y;

        height = instance.flexibleY ? Mathf.FloorToInt((worldPosition.y - terrainFloor) / size.y) : Mathf.FloorToInt(offset.y / size.y);

        return new Vector3(
            Mathf.Floor(offset.x / size.x) * size.x + origin.x,
            instance.flexibleY ? 
                height * size.y + terrainFloor :
                height * size.y + origin.y,
            Mathf.Floor(offset.z / size.z) * size.z + origin.z
        ) + HalfSize;
    }

    public static Vector3 SnapGridGroundCenter(Vector3 worldPosition)
    {
        var pos = SnapGridCenter(worldPosition, out int height);
        return pos + new Vector3(0, -height * instance.gridSize.y);
    }

#if UNITY_EDITOR

    [SerializeField]
    Transform trackedObject;

    [SerializeField, Range(3, 20)]
    int griddedPlanarOffset = 7;

    [SerializeField, Range(0, 5)]
    int griddedVerticalOffset = 3;

    [SerializeField]
    bool showAboveGround = false;
    [SerializeField]
    bool showBelowGround = false;

    [SerializeField]
    Color groundFloorColor = Color.magenta;

    [SerializeField, Range(0, 0.5f)]
    float groundThickness = 0.1f;


    private void OnDrawGizmosSelected()
    {
        if (trackedObject == null) return;

        int height;
        var refPoint = SnapGridCenter(trackedObject.position, out height);
        var upOne = new Vector3(0, gridSize.y);

        var floorSize = new Vector3(gridSize.x, groundThickness, gridSize.z);

        for (int dx = -griddedPlanarOffset; dx <= griddedPlanarOffset; dx++)
        {
            for (int dz = -griddedPlanarOffset; dz <= griddedPlanarOffset; dz++)
            {
                var pt = SnapGridCenter(refPoint + GetXZOffset(dx, dz), out height);

                for (int dy = -griddedVerticalOffset; dy <= griddedVerticalOffset; dy++)
                {
                    var pos = pt + dy * upOne;
                    var belowGround = height + dy < 0;
                    var aboveGround = height + dy > 0;

                    if (belowGround && !showBelowGround || aboveGround && !showAboveGround) continue;

                    bool ground = !aboveGround && !belowGround;

                    Gizmos.color = ground ? Color.magenta : Color.gray;                    
                    Gizmos.DrawWireCube(pos, gridSize);

                    if (ground)
                    {
                        Gizmos.color = groundFloorColor;
                        Gizmos.DrawCube(pos - 0.5f * upOne, floorSize);


                        if (dx == 0 && dz == 0)
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawSphere(pos, 0.5f);
                        }
                    }
                }
            }
        }

    }
#endif
}
