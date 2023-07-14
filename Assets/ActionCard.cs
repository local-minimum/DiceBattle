using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionCard : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI SlottedDiceUI;

    [SerializeField]
    TMPro.TextMeshProUGUI ActionCostUI;

    [SerializeField, Range(0, 3)]
    int actionPointCost = 1;

    List<DieDropZone> dropZones = new List<DieDropZone>();

    private void Awake()
    {
        dropZones.AddRange(GetComponentsInChildren<DieDropZone>(true));
        SyncSlottedDice();
    }

    private void OnEnable()
    {
        SyncActionPointCost();
        DieDropZone.OnChange += DieDropZone_OnChange;
    }

    private void OnDisable()
    {
        DieDropZone.OnChange -= DieDropZone_OnChange;
    }

    private void DieDropZone_OnChange(DieDropZone dropZone)
    {
        if (!dropZones.Contains(dropZone)) return;

        SyncSlottedDice();
    }

    void SyncSlottedDice()
    {
        var slotted = dropZones.Where(dz => dz.HoldsDie).Count();
        SlottedDiceUI.text = $"{slotted} / {dropZones.Count}";
    }

    void SyncActionPointCost()
    {
        ActionCostUI.text = new string('X', actionPointCost);
    }
}
