using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrid
{
    public Vector3 GridCenter(Vector3 worldPosition, out int height);

    public Vector3 GridGroundCenter(Vector3 worldPosition);

    public Vector3 GridSize { get;  }
    public Vector3 HalfSize { get; }
    public float HalfHeight { get; }
}
