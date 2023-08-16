using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MonsterActionPreviewUI : MonoBehaviour
{
    [SerializeField]
    Image IconImage;

    [SerializeField]
    TooltipedItem Tooltip;

    [SerializeField]
    TMPro.TextMeshProUGUI CooldownText;


    public void Sync(MonsterAction action, int remainingAP)
    {
        IconImage.sprite = Iconography.GetAction(action.ActionType);
        CooldownText.text = action.Cooldown.ToString();

        Tooltip.SetTooltip(action.Name);
    }
}
