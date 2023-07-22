using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MonsterActionPreviewUI : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI CooldownText;

    [SerializeField]
    GameObject CooldownCover;

    [SerializeField]
    Image Preview;

    [SerializeField]
    TooltipedItem Tooltip;

    [SerializeField]
    TMPro.TextMeshProUGUI ValueText;


    public void Sync(MonsterAction action)
    {
        Preview.sprite = action.Sprite;
        ValueText.text = action.Value.ToString();

        Tooltip.SetTooltip(action.Name);

        if (action.CanBeUsed)
        {
            CooldownCover.SetActive(false);
            return;
        }

        CooldownCover.SetActive(true);
        var cooldown = action.Cooldown;

        CooldownText.text = cooldown == 0 ? "" : cooldown.ToString();
    }
}
