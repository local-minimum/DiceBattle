using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void MonsterReport(Monster monster, string report);

public class Monster : MonoBehaviour
{
    public static event MonsterReport OnReport;

    public static Monster HoveredMonster { get; set; }

    [SerializeField]
    TMPro.TextMeshProUGUI HealthText;

    [SerializeField]
    TMPro.TextMeshProUGUI NameText;

    [SerializeField]
    TMPro.TextMeshProUGUI StatusText;

    [SerializeField]
    int startHealth;

    [SerializeField]
    GameObject BaseCard;

    private int _health;
    public int Health { 
        get => _health; 
        set
        {
            _health = Mathf.Max(0, value);
            HealthText.text = _health.ToString();

            if (_health == 0)
            {
                deathHide = Time.timeSinceLevelLoad + DelayDeathHide;
            }
        }
    }

    [SerializeField]
    float DelayDeathHide = 0.5f;

    float deathHide;

    [SerializeField]
    TMPro.TextMeshProUGUI DefenceText;

    [SerializeField]
    int startDefence;

    [SerializeField]
    int xpReward;

    [SerializeField]
    int startDice;

    [SerializeField]
    AnimationCurve extraDiceProb;

    [SerializeField]
    MonsterAttack[] attacks;

    [SerializeField]
    MonsterAttackUI[] attackUI;

    public int XpReward => xpReward;

    int _defence;
    public int Defence { 
        get => _defence; 
        set
        {
            _defence = value;
            DefenceText.text = _defence.ToString();
        }
    }

    public string Name => NameText.text;

    public bool Alive => Health > 0;

    private void OnEnable()
    {
        diceHeld = startDice;
        for (int i = 0; i<attacks.Length; i++)
        {
            attacks[i].Reset();
        }

        Health = startHealth;
        Defence = startDefence;
        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Cleanup:
                Cleanup();
                break;
            case BattlePhase.SelectNumberOfDice:
                SelectNumberAndRollDice();
                break;
            case BattlePhase.RollDice:
                RollDice();
                break;
            case BattlePhase.UseDice:
                SlotDice();
                break;
        }
    }

    int diceHeld;
    int[] diceValues;

    void SlotDice()
    {
        for (int i = 0; i<diceValues.Length; i++)
        {
            bool usedDie = false;
            var value = diceValues[i];
            for (int j = 0; j<attacks.Length; j++)
            {
                var attack = attacks[j];
                if (!attack.CanBeUsed) continue;

                if (attack.TakeDie(value))
                {
                    usedDie = true;
                    break;
                }
            }

            if (usedDie) continue;

            for (int j = 0; j<attacks.Length; j++)
            {
                var attack = attacks[j];
                if (attack.CanBeUsed) continue;

                if (attack.TakeDie(value))
                {
                    break;
                }
            }

        }

        StatusText.text = "";

        SyncAttacksUI();
    }

    void RollDice()
    {
        diceValues = diceValues.Select(_ => Random.Range(1, 7)).ToArray();

        var text = "";
        if (diceValues.Length == 0)
        {
            text = "No dice to slot"; ;
        } else if (diceValues.Length == 1)
        {
            text = $"Got die: {diceValues[0]}";
        } else
        {
            var diceText = string.Join(" ", diceValues);
            text = $"Got dice: {diceText}";
        }

        StatusText.text = text;
        OnReport?.Invoke(this, text);
    }

    void SelectNumberAndRollDice()
    {
        var diceCount = Mathf.Min(attacks.Where(a => a.CanBeUsed).Sum(a => a.SuggestDiceThrowCount()), diceHeld);
        var t = 1f - ((float)diceHeld - diceCount) / startDice;

        if (diceCount < diceHeld && Random.value < extraDiceProb.Evaluate(t))
        {
            diceCount++;
        }

        diceHeld -= diceCount;
        diceValues = new int[diceCount];

        var text = $"Will roll {diceValues.Length} dice";
        StatusText.text = text;
        OnReport?.Invoke(this, text);
    }

    void Cleanup()
    {
        for (int i = 0; i<attacks.Length; i++)
        {
            var attack = attacks[i];

            attack.DecayValues();
            attack.NewTurn();
        }
        SyncAttacksUI();
    }

    void SyncAttacksUI()
    {
        var visible = attacks.OrderBy(a => !a.CanBeUsed).Take(attackUI.Length).ToArray();
        for (int i = 0; i<attackUI.Length; i++)
        {
            attackUI[i].Sync(i < visible.Length ? visible[i] : null);
        }
    }

    private void Update()
    {
        if (!Alive && BaseCard.activeSelf && Time.timeSinceLevelLoad > deathHide)
        {
            BaseCard.SetActive(false);
        }
    }
}
