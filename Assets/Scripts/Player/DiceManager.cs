using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI diceBag;

    [SerializeField]
    DieTrashZone diceTrash;

    [SerializeField]
    Die[] dice;

    [SerializeField]
    ActionCardGroup actionCardGroup;

    public int diceCount = -1;

    private void OnEnable()
    {
        diceCount = GameProgress.Dice;
        diceBag.text = diceCount.ToString();
        diceTrash.Trashed = 0;

        DieDropZone.OnRecycleDie += DieDropZone_OnRecycleDie;
        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        DieDropZone.OnRecycleDie -= DieDropZone_OnRecycleDie;
    }

    [SerializeField]
    DelayedGate triggerNextPhase = new DelayedGate();

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        triggerNextPhase.Reset();

        switch (phase)
        {
            case BattlePhase.SelectNumberOfDice:
                PrepareDiceCountSelection();
                if (diceCount == 0)
                {
                    Debug.Log("[Dice Manager] Player has no dice to roll, go to next phase");
                    triggerNextPhase.Lock();
                }
                break;
            case BattlePhase.RollDice:
                RollSelectedDice();
                break;
            case BattlePhase.UseDice:
                if (!HasDiceThatCanBeSlotted())
                {
                    Debug.Log("[Dice Manager] Player has no dice to use, go to next phase");
                    triggerNextPhase.Lock();
                }
                break;
            case BattlePhase.PlayerAttack:
            case BattlePhase.Cleanup:
                ResetDice();
                break;
        }
    }

    Die rollToDie;

    private void RollSelectedDice()
    {
        if (rollToDie == null) return;

        bool encountered = false;
        for (int i = 0; i<dice.Length; i++)
        {
            if (!encountered)
            {
                dice[i].Roll();
                diceCount--;

                encountered = rollToDie == dice[i];
            } else
            {
                dice[i].NoDice();
            }
        }

        diceBag.text = diceCount.ToString();
    }
    private void DieDropZone_OnRecycleDie(int value)
    {
        diceTrash.Trashed++;
    }

    public void ResetDice()
    {
        int toTrash = 0;
        for (int i = 0; i<dice.Length; i++)
        {
            if (dice[i].Rolled)
            {
                toTrash++;
            }

            dice[i].Clear();
            dice[i].NoDice();
        }

        if (toTrash > 0)
        {
            diceTrash.Trashed += toTrash;
        }
        rollToDie = null;
    }

    public void PrepareDiceCountSelection()
    {
        for (int i = 0; i < dice.Length; i++)
        {
            if (i < Mathf.Min(diceCount, GameProgress.RollSize))
            {
                dice[i].HasDie();
            } else
            {
                dice[i].NoDice();
            }

        }

        for (int i = 0; i< dice.Length; i++)
        {
            dice[i].transform.SetSiblingIndex(i);
        }
    }

    public void SetRollCandidates(Die untilDie)
    {
        if (Battle.Phase != BattlePhase.SelectNumberOfDice) return;

        bool encountered = false;
        for (int i = 0; i<dice.Length; i++)
        {
            if (!encountered && untilDie != null)
            {
                dice[i].RollCandidate = !encountered;

                if (untilDie == dice[i])
                {
                    encountered = true;
                }
            } else
            {
                dice[i].RollCandidate = false;
            }
        }
    }

    public void EndHoverDie(Die die)
    {
        if (Battle.Phase != BattlePhase.SelectNumberOfDice) return;

        for (int i = 0; i<dice.Length; i++)
        {
            if (die != dice[i])
            {
                dice[i].RollCandidate = false;
            }
        }
    }

    public void SelectDieToRoll(Die die)
    {
        if (Battle.Phase != BattlePhase.SelectNumberOfDice) return;

        rollToDie = die;
        Battle.Phase = BattlePhase.RollDice;
    }

    public bool HasRemainingRolledDice()
    {
        for (int i = 0; i<dice.Length; i++)
        {
            if (dice[i].Rolled)
            {
                return true;
            }
        }
        return false;
    }

    public void CheckIfMoreDiceCanBeSlotted()
    {
        if (!HasDiceThatCanBeSlotted())
        {
            triggerNextPhase.Lock();
        }
    }

    public bool HasDiceThatCanBeSlotted()
    {
        if (!HasRemainingRolledDice()) return false;

        return actionCardGroup.SlottablePositions > 0;
    }

    private void Update()
    {
        
        if (triggerNextPhase.Open(out bool toggled))
        {
            if (toggled && (Battle.Phase == BattlePhase.UseDice || Battle.Phase == BattlePhase.SelectNumberOfDice))
            {
                Battle.Phase = Battle.Phase.NextPhase();
            }
        }
    }
}
