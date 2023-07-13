using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DiceManagerPhases { PreRoll, Rolled };

public delegate void DicePhaseEvent(DiceManagerPhases phase);

public class DiceManager : MonoBehaviour
{
    public static event DicePhaseEvent OnPhaseChange;

    private DiceManagerPhases _phase;

    public DiceManagerPhases Phase
    {
        get => _phase;
        set
        {
            _phase = value;
            OnPhaseChange?.Invoke(value);
        }
    }

    [SerializeField]
    TMPro.TextMeshProUGUI diceBag;

    [SerializeField]
    TMPro.TextMeshProUGUI trashBag;

    [SerializeField]
    Die[] dice;

    public int diceCount = 20;
    public int discardedDice = 0;

    public int maxHandSize = 4;

    private void OnEnable()
    {
        DieDropZone.OnRecycleDie += DieDropZone_OnRecycleDie;
        ResetDice();
    }

    private void OnDisable()
    {
        DieDropZone.OnRecycleDie -= DieDropZone_OnRecycleDie;
    }

    private void DieDropZone_OnRecycleDie(int value)
    {
        discardedDice++;
        trashBag.text = discardedDice.ToString();
    }

    public void ResetDice()
    {
        for (int i = 0; i<dice.Length; i++)
        {
            if (dice[i].Rolled)
            {
                discardedDice++;
                diceCount--;
            }

            dice[i].Clear();
        }

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

        diceBag.text = diceCount.ToString();
        trashBag.text = discardedDice.ToString();

        Phase = DiceManagerPhases.PreRoll;
    }

    public void StartHoverDie(Die die)
    {
        if (Phase != DiceManagerPhases.PreRoll) return;

        bool encountered = false;
        for (int i = 0; i<dice.Length; i++)
        {
            if (die == dice[i])
            {
                encountered = true;
            }
            else
            {
                dice[i].BeforeRollHovered = !encountered;
            }
        }
    }

    public void EndHoverDie(Die die)
    {
        if (Phase != DiceManagerPhases.PreRoll) return;

        for (int i = 0; i<dice.Length; i++)
        {
            if (die != dice[i])
            {
                dice[i].BeforeRollHovered = false;
            }
        }
    }

    public void RollDie(Die die)
    {
        if (Phase != DiceManagerPhases.PreRoll) return;

        Phase = DiceManagerPhases.Rolled;
        bool encountered = false;
        for (int i = 0; i<dice.Length; i++)
        {
            if (die == dice[i])
            {
                encountered = true;
            }
            else if (!encountered)
            {
                dice[i].Roll();
            } else
            {
                dice[i].NoDice();
            }
        }
    }
}
