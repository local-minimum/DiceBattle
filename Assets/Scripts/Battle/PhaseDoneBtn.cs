using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseDoneBtn : MonoBehaviour
{
    [SerializeField]
    GameObject button;

    [SerializeField]
    List<BattlePhase> VisiblePhases = new List<BattlePhase>() { BattlePhase.UseDice, BattlePhase.PlayerAttack };

    [SerializeField]
    DelayedGate delayedVisibility;

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
        button.SetActive(false);
        if (VisiblePhases.Contains(phase))
        {
            delayedVisibility.Lock();
        } else
        {
            delayedVisibility.Reset();
        }
    }

    public void OnClickButton()
    {
        if (delayedVisibility.Locked) return;

        button.SetActive(false);
        Battle.Phase = Battle.Phase.NextPhase();
    }

    private void Update()
    {
        if (delayedVisibility.Open(out bool toggled))
        {
            if (toggled)
            {
                button.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickButton();
            }
        }
    }
}
