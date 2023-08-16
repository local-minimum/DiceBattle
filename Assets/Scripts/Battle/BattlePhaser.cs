using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePhaser : MonoBehaviour
{
    List<BattlePhase> AutoPhases = new List<BattlePhase>() { 
        BattlePhase.Intro, 
        BattlePhase.RollDice, 
        BattlePhase.None,
        BattlePhase.Outro,
        BattlePhase.Cleanup,
    };

    [SerializeField]
    DelayedGate automaticPhaseGate = new DelayedGate(0.5f);
    [SerializeField]
    DelayedGate automaticSceneUnload = new DelayedGate(1f);

    private void Start()
    {
        Battle.Phase = BattlePhase.Intro;
    }

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;

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
        if (AutoPhases.Contains(phase))
        {
            automaticPhaseGate.Lock();
            Debug.Log($"[Battle Phaser] Preparing automatic exit of phase <{phase}>");
        } else if (automaticPhaseGate.Locked)
        {
            automaticPhaseGate.Reset();
        }
    }

    private void Update()
    {
        if (automaticPhaseGate.Open(out bool toggled))
        {
            if (Battle.Phase == BattlePhase.Outro)
            {
                if (toggled)
                {
                    Debug.Log($"[Battle Phaser] Adding extra wait time until scene change");
                    automaticSceneUnload.Lock();
                } else if (!automaticSceneUnload.Locked)
                {
                    Debug.Log($"[Battle Phaser] Exiting battle scene");
                    GameProgress.ExitBattle();
                }
            } else
            {
                if (toggled)
                {
                    Debug.Log($"[Battle Phaser] Changing phase");
                    Battle.Phase = Battle.Phase.NextPhase();
                }
            }
        }
    }
}
