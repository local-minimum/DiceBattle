using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialLayout : MonoBehaviour
{
    [SerializeField]
    float placementStart;

    [SerializeField]
    float anglePadding;


    // TODO: Nothing implemented

    private void OnDrawGizmosSelected()
    {
        var rt = transform as RectTransform;
        var radius = rt.sizeDelta.y;
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
