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
    float nextPhase;

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
        Battle.Phase = WakeupPhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;    
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (AutoPhases.Contains(phase))
        {
            nextPhase = Time.timeSinceLevelLoad + autophaseDelay;
            autophase = true;
        }
    }

    private void Update()
    {
        if (autophase && Time.timeSinceLevelLoad > nextPhase)
        {
            autophase = false;
            Battle.Phase = Battle.Phase.NextPhase();
        }
    }
}
