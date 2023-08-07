using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPlayerStats : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI CurrentHealthUI;

    [SerializeField]
    TMPro.TextMeshProUGUI MaxHealthUI;

    [SerializeField]
    TMPro.TextMeshProUGUI CurrentRollSizeUI;

    [SerializeField]
    TMPro.TextMeshProUGUI MaxRollSizeUI;

    [SerializeField]
    TMPro.TextMeshProUGUI CurrentDiceCountUI;

    [SerializeField]
    TMPro.TextMeshProUGUI CurrentXPUI;

    public void UpdateStats()
    {
        CurrentHealthUI.text = GameProgress.Health.ToString();
        MaxHealthUI.text = GameProgress.MaxHealth.ToString();

        CurrentRollSizeUI.text = GameProgress.RollSize.ToString();
        MaxRollSizeUI.text = GameSettings.MaxPlayerRollSize.ToString();

        CurrentDiceCountUI.text = GameProgress.Dice.ToString();

        CurrentXPUI.text = GameProgress.XP.ToString();
    }

    private void Start()
    {
        UpdateStats();
    }
}
