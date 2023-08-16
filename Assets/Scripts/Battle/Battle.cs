using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattlePhase
{
    None,
    SelectNumberOfDice,
    RollDice,
    UseDice,
    PlayerAttack,
    MonsterAttack,
    Cleanup,
    Outro,
    Intro
}

public delegate void ChangeBattlePhaseEvent(BattlePhase phase);
public delegate void BattleBeginEvent();
public delegate void BattleEndEvent();

public static class Battle
{
    private static BattlePhase _phase;

    public static event ChangeBattlePhaseEvent OnChangePhase;
    public static event BattleBeginEvent OnBeginBattle;
    public static event BattleEndEvent OnEndBattle;

    public static BattlePhase Phase {
        get => _phase;
        set {
            _phase = value;
            OnChangePhase?.Invoke(value);
            if (_phase == BattlePhase.Outro)
            {
                OnEndBattle?.Invoke();
            } else if (_phase == BattlePhase.Intro)
            {
                OnBeginBattle?.Invoke();
            }
        }
    }

    public static BattlePhase NextPhase(this BattlePhase phase) {
        switch (phase)
        {
            case BattlePhase.Cleanup:
            case BattlePhase.None:
                return BattlePhase.SelectNumberOfDice;
            case BattlePhase.SelectNumberOfDice:
                return BattlePhase.RollDice;
            case BattlePhase.RollDice:
                return BattlePhase.UseDice;
            case BattlePhase.UseDice:
                return BattlePhase.PlayerAttack;
            case BattlePhase.PlayerAttack:
                return BattlePhase.MonsterAttack;
            case BattlePhase.MonsterAttack:
            case BattlePhase.Intro:
                return BattlePhase.Cleanup;
            default:
                return BattlePhase.None;
        }
    }
}
