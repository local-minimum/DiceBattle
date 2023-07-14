using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipedItem : MonoBehaviour
{
    private void OnMouseEnter()
    {
        Debug.Log("hello");
    }

    private void OnMouseExit()
    {
        Debug.Log("Bye");
    }
}
