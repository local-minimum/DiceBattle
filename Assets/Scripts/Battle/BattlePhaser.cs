using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePhaser : MonoBehaviour
{
    [SerializeField]
    List<BattlePhase> AutoPhases = new List<BattlePhase>();

    [SerializeField]
    BattlePhase WakeupPhase = BattlePhase.Cleanup;

    [SerializeField]
    float autophaseDelay = 0.2f;

    bool autophase;
    float nextPhaseTime;

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
        Battle.Phase = WakeupPhase;

        MonsterManager.OnWipe += MonsterManager_OnWipe;
    }


    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        MonsterManager.OnWipe -= MonsterManager_OnWipe;
    }

    private void MonsterManager_OnWipe()
    {
        Battle.Phase = BattlePhase.Outro;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        Debug.Log($"{phase} ({AutoPhases.Contains(phase)})");
        if (AutoPhases.Contains(phase))
        {
            nextPhaseTime = Time.timeSinceLevelLoad + autophaseDelay;
            autophase = true;
        } else if (autophase)
        {
            autophase = false;
        }
    }

    private void Update()
    {
        if (autophase && Time.timeSinceLevelLoad > nextPhaseTime)
        {
            autophase = false;

            if (Battle.Phase == BattlePhase.Outro)
            {
                GameProgress.NextScene();
            } else
            {
                Battle.Phase = Battle.Phase.NextPhase();
            }
        }
    }
}
