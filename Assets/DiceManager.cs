using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DiceManagerPhases { PreRoll, Rolled };

public class DiceManager : MonoBehaviour
{
    public DiceManagerPhases phase;

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
        ResetDice();
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

        diceBag.text = diceCount.ToString();
        trashBag.text = discardedDice.ToString();

        phase = DiceManagerPhases.PreRoll;
    }

    public void StartHoverDie(Die die)
    {
        if (phase != DiceManagerPhases.PreRoll) return;

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
        if (phase != DiceManagerPhases.PreRoll) return;

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
        if (phase != DiceManagerPhases.PreRoll) return;

        phase = DiceManagerPhases.Rolled;
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
            }
        }
    }
}
