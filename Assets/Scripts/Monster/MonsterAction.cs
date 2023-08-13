using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public delegate void UseMonsterActionEvent(MonsterAction action);

public class MonsterAction : MonoBehaviour
{
    public static event UseMonsterActionEvent OnUse;

    MonsterActionSetting settings;

    [SerializeField]
    Image actionSprite;

    [SerializeField]
    TMPro.TextMeshProUGUI titleUI;

    [SerializeField]
    TMPro.TextMeshProUGUI valueUI;

    [SerializeField]
    TMPro.TextMeshProUGUI actionPointsUI;

    [SerializeField]
    Image typeUI;

    [SerializeField]
    Sprite attackSprite;

    [SerializeField]
    Sprite defenceSprite;

    [SerializeField]
    Sprite healingSprite;

    int cooldown = -2;

    int[] diceValues;

    public IEnumerable<int> DiceValues(bool withSign)
    {
        for (int i = 0; i<diceValues.Length; i++)
        {
            var dieValue = diceValues[i] != 0 ? diceValues[i] : settings.Slots[i].DefaultValue;
            yield return withSign && settings.Slots[i].Effect == DieEffect.Subtract ? -dieValue : dieValue;
        }
    }

    public IEnumerable<int> HighestDiceValues
    {
        get
        {
            for (int i = 0; i<diceValues.Length; i++)
            {
                yield return settings.Slots[i].Effect == DieEffect.Subtract ? -1 : 6;
            }
        }
    }

    public string Name => settings.Name;

    public Sprite Sprite => settings.Sprite;

    public int Value => Mathf.Max(0, DiceValues(true).Sum());
    public int HighestPossibleValue => HighestDiceValues.Sum();

    public string ValueRange => $"{Value} - {HighestPossibleValue}";

    public void Config(MonsterActionSetting setting)
    {
        this.settings = setting;
        diceValues = new int[setting.Slots.Length];
        actionSprite.sprite = setting.Sprite;
        titleUI.text = setting.Name;
        valueUI.text = ValueRange.ToString();
        actionPointsUI.text = setting.ActionPoints.ToString();

        switch (setting.ActionType)
        {
            case ActionType.Attack:
                typeUI.sprite = attackSprite;
                break;
            case ActionType.Defence:
                typeUI.sprite = defenceSprite;
                break;
            case ActionType.Healing:
                typeUI.sprite = healingSprite;
                break;
        }
    }

    public ActionType ActionType => settings.ActionType;
    public bool IsAttack => settings.ActionType == ActionType.Attack;
    public bool IsHeal => settings.ActionType == ActionType.Healing;
    public bool IsDefence => settings.ActionType == ActionType.Defence;

    public bool IsOnCooldown => settings == null || cooldown < settings.Cooldown; 
    public bool IsUsableActiveAction => settings != null && !IsDefence && !IsOnCooldown && Value > 0;
    public bool IsUsablePassiveAction => settings != null && IsDefence && !IsOnCooldown && Value > 0;

    public int ActionPoints => settings.ActionPoints;

    public int Cooldown => Mathf.Max(0, settings.Cooldown - cooldown);

    public void RevealValue()
    {
        valueUI.text = Value.ToString();
    }

    public void Use()
    {
        cooldown = -1;
        OnUse?.Invoke(this);
    }

    public void NewTurn()
    {
        if (cooldown == -2)
        {
            cooldown = settings.StartCooldown;
        }

        if (cooldown < 0)
        {
            cooldown = 0;
        } else
        {
            cooldown++;
        }

        valueUI.text = ValueRange.ToString();

        Debug.Log($"[{Name}] can be used in {Cooldown} turns");
    }

    public void DecayDice()
    {
        for (int i=0; i<diceValues.Length; i++)
        {
            if (diceValues[i] == 0) continue;

            diceValues[i] = Mathf.Max(1, diceValues[i] - 1);
        }
    }

    public bool TakeDie(int value)
    {
        int pos = -1;
        int delta = 0;
        int currentDelta = 0;
        var currentDiceValues = DiceValues(false).ToArray();
        for (int i = 0,l=settings.Slots.Length; i<l; i++)
        {
            var slot = settings.Slots[i];
            switch (slot.Effect)
            {
                case DieEffect.Add:
                    currentDelta = value - currentDiceValues[i];
                    break;
                case DieEffect.Subtract:
                    currentDelta = currentDiceValues[i] - value;
                    break;
                default:
                    Debug.LogWarning($"[{Name}] Die index {i} has effect {slot.Effect} which is unknown");
                    break;
            }
            if (currentDelta > delta)
            {
                delta = currentDelta;
                pos = i;
            }
        }

        if (pos == -1) return false;

        diceValues[pos] = value;
        Debug.Log($"[{Name}] Slotted die with value {value} into slot {pos}");

        return true;
    }
    public int SuggestDiceThrowCount()
    {
        int maybes = 0;
        int wants = 0;
        var currentDiceValues = DiceValues(false).ToArray();
        for (int i = 0, l = settings.Slots.Length; i < l; i++)
        {
            var slot = settings.Slots[i];
            int value = currentDiceValues[i];
            switch (slot.Effect)
            {
                case DieEffect.Add:
                    if (value < 3)
                    {
                        wants++;
                    }
                    else if (value == 4)
                    {
                        maybes++;
                    }
                    break;
                case DieEffect.Subtract:
                    if (value > 2)
                    {
                        wants++;
                    }
                    else if (value == 2)
                    {
                        maybes++;
                    }
                    break;
                default:
                    Debug.LogWarning($"Monster attack '{name}', die index {i} has effect {slot.Effect} which is unknown");
                    break;
            }
        }

        return wants + (maybes > 1 ? maybes / 2 : maybes);
    }
}
