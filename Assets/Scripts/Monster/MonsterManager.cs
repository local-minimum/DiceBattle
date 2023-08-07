using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void MonstersWipeEvent();

public class MonsterManager : MonoBehaviour
{
    public static event MonstersWipeEvent OnWipe;

    List<Monster> Monsters = new List<Monster>();

    bool AnyAlive
    {
        get
        {
            return Monsters.Any(m => m.Alive);
        }
    }

    private void Awake()
    {
        Monsters.AddRange(GetComponentsInChildren<Monster>());
    }

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
        Monster.OnDeath += Monster_OnDeath;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        Monster.OnDeath -= Monster_OnDeath;
    }

    private void Monster_OnDeath(Monster monster)
    {
        if (!AnyAlive)
        {
            OnWipe?.Invoke();
        }
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
