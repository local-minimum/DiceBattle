using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitShopButton : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("[Exit Shop Button] Exiting shop");
        GameProgress.InvokeBattle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClick();
        }
    }
}
