using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitShopButton : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("Exiting XP Shop");
        GameProgress.InvokeBattle();
    }
}
