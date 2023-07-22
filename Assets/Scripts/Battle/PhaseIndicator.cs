using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseIndicator : MonoBehaviour
{
    [SerializeField]
    List<BattlePhase> Phases;

    [SerializeField]
    List<string> Messages;

    [SerializeField]
    bool debugEmpty;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;    
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        var idx = Phases.IndexOf(phase);
        if (idx >= 0 && idx < Messages.Count)
        {
            TextUI.text = Messages[idx];
        } else if (debugEmpty)
        {
            TextUI.text = phase.ToString();
        }
    }
}
