using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackUI : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI Text;

    [SerializeField]
    TooltipedItem Tooltip;

    [SerializeField]
    Color readyColor;

    [SerializeField]
    Color cooldownColor;

    public void Sync(MonsterAttack attack)
    {
        if (attack == null)
        {
            Text.text = "";
            Tooltip.enabled = false;
        } else
        {
            Tooltip.enabled = true;
            Tooltip.SetTooltip(attack.Name);
            Text.text = attack.Notation;
            Text.color = attack.CanBeUsed ? readyColor : cooldownColor;
        }
    }
}
