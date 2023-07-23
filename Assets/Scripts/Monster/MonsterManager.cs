using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    List<Monster> Monsters = new List<Monster>();

    private void Awake()
    {
        Monsters.AddRange(GetComponentsInChildren<Monster>());
    }

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;    
    }

    bool mayTriggerMonsterAction;
    float nextMonsterAction;

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.MonsterAttack:
                nextMonsterAction = Time.timeSinceLevelLoad + GameSettings.MonsterPhasePreDelay;
                mayTriggerMonsterAction = true;
                break;
        }
    }

    bool SelectMonsterToDoAction()
    {
        var options = Monsters.Where(m => m.CanDoAction).ToArray();
        if (options.Length == 0)
        {
            return false;
        }

        options[Random.Range(0, options.Length)].DoAction();

        return true;
    }

    bool mayEndPhase;
    float endPhaseTime;

    private void Update()
    {
        if (mayTriggerMonsterAction && Time.timeSinceLevelLoad > nextMonsterAction)
        {
            if (SelectMonsterToDoAction())
            {
                nextMonsterAction = Time.timeSinceLevelLoad + GameSettings.MonsterPhaseAttackDuration;
            } else {
                mayTriggerMonsterAction = false;
                endPhaseTime = Time.timeSinceLevelLoad + GameSettings.MonsterPhasePostDelay;
                mayEndPhase = true;
            }
        } else if (mayEndPhase && Time.timeSinceLevelLoad > endPhaseTime)
        {
            mayEndPhase = false;
            Battle.Phase = Battle.Phase.NextPhase();
        }
    }
}
