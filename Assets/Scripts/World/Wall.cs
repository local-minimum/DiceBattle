using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Wall : MonoBehaviour
{
    [SerializeField, Range(1, 20)]
    float height = 5f;

    [SerializeField, Range(0, 10)]
    float baseWidth = 0.5f;

    [SerializeField, Range(0, 10)]
    float topWidth = 0.3f;

    [SerializeField]
    bool connectEndWithStart;

    [SerializeField]
    List<Vector3> anchors = new List<Vector3>();

    [SerializeField, HideInInspector]
    Mesh mesh;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        for (int i = 0, l = anchors.Count; i<l; i++)
        {
            Gizmos.DrawWireSphere(anchors[i], 1f);
        }

        UpdateMesh();
    }

    struct MeshSegment {
        public Vector3[] Verts;
        public int[] Tris;

        public MeshSegment(Vector3[] verts, int[] tris)
        {
            Verts = verts;
            Tris = tris;
        }
    }

    Vector3 InterpolateCorner(Vector3 origo, Vector3 inPos, Vector3 outPos)
    {
        var midPoint = (outPos - inPos) * 0.5f + inPos;
        var midUnitVec = (midPoint - origo).normalized;
        var inVec = inPos - origo;
        var alpha = Vector3.Angle(midUnitVec, inVec) * Mathf.Deg2Rad;
        if (alpha < 0.05) return midPoint;
        return midUnitVec * inVec.magnitude / Mathf.Abs(Mathf.Cos(alpha)) + origo;
    }

    (Vector3, Vector3, Vector3, Vector3, Vector3, Vector3, Vector3, Vector3) SegmentVerts(
        Vector3 start, Vector3 end, Vector3 right, Vector3 heightOffset
    )
    {
        var baseOffset = right * baseWidth;
        var topOffset = right * topWidth;
        
        return (
            start + baseOffset,
            start + topOffset + heightOffset,
            start - baseOffset,
            start - topOffset + heightOffset,
            end + baseOffset,
            end + topOffset + heightOffset,
            end - baseOffset,
            end - topOffset + heightOffset
            );
    }

    IEnumerable<MeshSegment> GenerateSegments()
    {
        var nAnchors = anchors.Count();
        var lastIndex = nAnchors - 1;
        var heightOffset = Vector3.up * height;
        var triStart = 0;
        for (int i = 0, l = nAnchors + (connectEndWithStart ? 0 : -1); i<l; i++)
        {
            var previous = anchors[connectEndWithStart ? (i - 1).Mod(nAnchors) : Mathf.Max(0, i - 1)];
            var start = anchors[i];
            var end = anchors[(i + 1) % nAnchors];
            var next = anchors[connectEndWithStart ? (i + 2) % nAnchors : Mathf.Min(nAnchors - 1, i + 2)];

            var prevAxis = (start - previous).normalized;
            var axis = (end - start).normalized;
            var nextAxis = (next - end).normalized;

            var isStartCap = i == 0 && !connectEndWithStart;
            var isEndCap = i == lastIndex - 1 && !connectEndWithStart;

            var prevRight = Vector3.Cross(prevAxis, Vector3.up);
            var right = Vector3.Cross(axis, Vector3.up);
            var nextRight = Vector3.Cross(nextAxis, Vector3.up);

            var (
                prevStartRightLower, 
                prevStartRightUpper, 
                prevStartLeftLower, 
                prevStartLeftUpper, 
                prevEndRightLower, 
                prevEndRightUpper, 
                prevEndLeftLower, 
                prevEndLeftUpper
                ) = SegmentVerts(previous, start, prevRight, heightOffset);

            /*
            Gizmos.color = Color.red;
            Gizmos.DrawCube(prevEndRightLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(prevEndRightUpper, Vector3.one * 0.1f);
            Gizmos.DrawCube(prevEndLeftLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(prevEndLeftUpper, Vector3.one * 0.1f);
            */
            var (
                startRightLower, 
                startRightUpper, 
                startLeftLower, 
                startLeftUpper, 
                endRightLower, 
                endRightUpper, 
                endLeftLower, 
                endLeftUpper
                ) = SegmentVerts(start, end, right, heightOffset);

            /*
            Gizmos.color = Color.green;
            Gizmos.DrawCube(endRightLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(endRightUpper, Vector3.one * 0.1f);
            Gizmos.DrawCube(endLeftLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(endLeftUpper, Vector3.one * 0.1f);
            */

            var (
                nextStartRightLower, 
                nextStartRightUpper, 
                nextStartLeftLower, 
                nextStartLeftUpper, 
                nextEndRightLower, 
                nextEndRightUpper, 
                nextEndLeftLower, 
                nextEndLeftUpper
                ) = SegmentVerts(end, next, nextRight, heightOffset);

            /*
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(nextStartRightLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(nextStartRightUpper, Vector3.one * 0.1f);
            Gizmos.DrawCube(nextStartLeftLower, Vector3.one * 0.1f);
            Gizmos.DrawCube(nextStartLeftUpper, Vector3.one * 0.1f);
            */

            var iStartRightUpper = isStartCap ? startRightUpper : InterpolateCorner(start + heightOffset, prevEndRightUpper, startRightUpper);
            var iEndRightUpper = isEndCap ? endRightUpper : InterpolateCorner(end + heightOffset, endRightUpper, nextStartRightUpper);
            var iStartLeftUpper = isStartCap ? startLeftUpper : InterpolateCorner(start + heightOffset, prevEndLeftUpper, startLeftUpper);
            var iEndLeftUpper = isEndCap ? endLeftUpper : InterpolateCorner(end + heightOffset, endLeftUpper, nextStartLeftUpper);

            if (isStartCap)
            {
                yield return new MeshSegment(
                    new Vector3[] { 
                        startLeftLower, 
                        startLeftUpper, 
                        startRightUpper, 
                        startRightLower 
                    },
                    new int[]
                    {
                        triStart + 0, triStart + 2, triStart + 1,
                        triStart + 0, triStart + 3, triStart + 2,
                    }
                );
                triStart += 4;
            }

            yield return new MeshSegment(
                new Vector3[]
                {
                    isStartCap ? startRightLower : InterpolateCorner(start, prevEndRightLower, startRightLower), // +0
                    iStartRightUpper,
                    isEndCap ? endRightLower : InterpolateCorner(end, endRightLower, nextStartRightLower),
                    iEndRightUpper,

                    isStartCap ? startLeftLower : InterpolateCorner(start, prevEndLeftLower, startLeftLower), // +4
                    iStartLeftUpper,
                    isEndCap ? endLeftLower : InterpolateCorner(end, endLeftLower, nextStartLeftLower),
                    iEndLeftUpper,

                    iStartRightUpper, // +8
                    iEndRightUpper,
                    iEndLeftUpper,
                    iStartLeftUpper,
                },
                new int[]
                {
                    triStart + 0, triStart + 2, triStart + 1, // Left Side Lower Tri
                    triStart + 1, triStart + 2, triStart + 3, // Left Side Upper Tri

                    triStart + 4, triStart + 5, triStart + 6, // Right Side Lower Tri
                    triStart + 5, triStart + 7, triStart + 6, // Right Side Upper Tri

                    triStart + 8, triStart + 9, triStart + 11, // Top First Part
                    triStart + 9, triStart + 10, triStart + 11, // Top Second Part
                }
            );

            triStart += 12;

            if (isEndCap)
            {
                yield return new MeshSegment(
                    new Vector3[]
                    {
                        endRightLower,
                        endRightUpper,
                        endLeftUpper,
                        endLeftLower,
                    },
                    new int[]
                    {
                        triStart + 0, triStart + 2, triStart + 1,
                        triStart + 0, triStart + 3, triStart + 2,
                    }
                );

                triStart += 4;
            }

        }
    } 


    void UpdateMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = $"Procedural Wall";
            GetComponent<MeshFilter>().mesh = mesh;
        } else
        {
            mesh.Clear();
        }

        var segments = GenerateSegments().ToArray();

        var verts = segments.SelectMany(s => s.Verts).ToArray();
        var tris = segments.SelectMany(s => s.Tris).ToArray();

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
    }
}
