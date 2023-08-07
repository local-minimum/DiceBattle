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
}

public delegate void ChangeBattlePhaseEvent(BattlePhase phase);

public static class Battle
{
    private static BattlePhase _phase;

    public static event ChangeBattlePhaseEvent OnChangePhase;

    public static BattlePhase Phase {
        get => _phase;
        set {
            _phase = value;
            OnChangePhase?.Invoke(value);
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
                return BattlePhase.Cleanup;
            default:
                return BattlePhase.None;
        }
    }
}
