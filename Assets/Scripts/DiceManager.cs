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

    public int diceCount = 20;

    public int maxHandSize = 4;

    private void OnEnable()
    {
        diceBag.text = diceCount.ToString();

        DieDropZone.OnRecycleDie += DieDropZone_OnRecycleDie;
        Battle.OnChangePhase += Battle_OnChangePhase;
    }


    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        DieDropZone.OnRecycleDie -= DieDropZone_OnRecycleDie;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.SelectNumberOfDice:
                PrepareDiceCountSelection();
                if (diceCount == 0)
                {
                    Battle.Phase = Battle.Phase.NextPhase();
                }
                break;
            case BattlePhase.RollDice:
                RollSelectedDice();
                break;
            case BattlePhase.UseDice:
                if (!HasDiceRemaining())
                {
                    Battle.Phase = Battle.Phase.NextPhase();
                }
                break;
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
        for (int i = 0; i<dice.Length; i++)
        {
            if (dice[i].Rolled)
            {
                diceTrash.Trashed++;
            }

            dice[i].Clear();
            dice[i].NoDice();
        }

        rollToDie = null;
    }

    public void PrepareDiceCountSelection()
    {
        for (int i = 0; i < dice.Length; i++)
        {
            if (i < Mathf.Min(diceCount, maxHandSize))
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

    public bool HasDiceRemaining()
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
}
