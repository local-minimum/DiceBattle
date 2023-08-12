using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseDoneBtn : MonoBehaviour
{
    [SerializeField]
    GameObject button;

    [SerializeField]
    List<BattlePhase> VisiblePhases = new List<BattlePhase>() { BattlePhase.UseDice, BattlePhase.PlayerAttack };

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
        button.SetActive(VisiblePhases.Contains(phase));
    }

    public void OnClickButton()
    {
        Battle.Phase = Battle.Phase.NextPhase();
    }

    private void Update()
    {
        if (button.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            OnClickButton();
        }
    }
}
