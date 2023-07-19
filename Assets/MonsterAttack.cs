using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct MonsterDieSlot
{
    public DieEffect Effect;

    [SerializeField]
    int StartValue;

    public int Value { get; set; }

    public MonsterDieSlot(DieEffect effect, int value)
    {
        Effect = effect;
        StartValue = value;
        Value = value;
    }

    public void Decay()
    {
        Value = Mathf.Max(0, Value - 1);
    }
    public void Reset()
    {
        Value = StartValue;
    }
}

[System.Serializable]
public class MonsterAttack
{

    [SerializeField]
    private string name;

    [SerializeField]
    int cooldown;

    public int Cooldown => cooldown;

    public string Name => name;

    [SerializeField]
    MonsterDieSlot[] slots;

    int length => slots.Length;

    public int Attack
    {
        get
        {
            int value = 0;
            for (int i = 0,l=length; i<l; i++)
            {
                var slot = slots[i];
                switch (slot.Effect)
                {
                    case DieEffect.Add:
                        value += slot.Value;
                        break;
                    case DieEffect.Subtract:
                        value -= slot.Value;
                        break;
                    default:
                        Debug.LogWarning($"Monster attack '{name}', die index {i} has effect {slot.Effect} which is unknown");
                        break;
                }
            }

            return value;
        }
    }

    int cooldownCounter;
    public bool CanBeUsed => cooldownCounter >= cooldown;

    public void NewTurn()
    {
        cooldownCounter++;
    }

    public void DecayValues()
    {
        for (int i = 0,l=length; i<l; i++)
        {
            slots[i].Decay();
        }
    }

    public bool TakeDie(int value)
    {
        int pos = -1;
        int delta = 0;
        int currentDelta = 0;
        for (int i = 0,l=length; i<l; i++)
        {
            var slot = slots[i];
            switch (slot.Effect)
            {
                case DieEffect.Add:
                    currentDelta = value - slot.Value;
                    break;
                case DieEffect.Subtract:
                    currentDelta = slot.Value - value;
                    break;
                default:
                    Debug.LogWarning($"Monster attack '{name}', die index {i} has effect {slot.Effect} which is unknown");
                    break;
            }
            if (currentDelta > delta)
            {
                delta = currentDelta;
                pos = i;
            }
        }

        if (pos == -1) return false;

        slots[pos].Value = value;
        return true;
    }

    public int SuggestDiceThrowCount()
    {
        int maybes = 0;
        int wants = 0;
        for (int i = 0,l=length; i<l; i++)
        {
            var slot = slots[i];
            int value = slot.Value;
            switch (slot.Effect)
            {
                case DieEffect.Add:
                    if (value < 3)
                    {
                        wants++;
                    } else if (value == 4)
                    {
                        maybes++;
                    }
                    break;
                case DieEffect.Subtract:
                    if (value > 2)
                    {
                        wants++;
                    } else if (value == 2)
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

    public string Notation
    {
        get
        {
            var grouped = slots.GroupBy(s => s.Effect).ToDictionary(
                g => g.Key, 
                g => g.Select(slot => slot.Value == 0 ? "?" : slot.Value.ToString()).ToList()
            );

            var text = "";

            if (grouped.ContainsKey(DieEffect.Add))
            {
                text = string.Join(" + ", grouped[DieEffect.Add]);
            }

            if (grouped.ContainsKey(DieEffect.Subtract))
            {
                if (text != "") text += " ";

                var items = grouped[DieEffect.Subtract];

                if (items.Count == 1)
                {
                    text += $"-{items[0]}";
                } else
                {
                    var subtraction = string.Join(" + ", items);
                    text += $"- ({subtraction})";
                }
            }

            return text;
        }
    }

    public void Reset()
    {
        cooldownCounter = 0;

        for (int i = 0; i<slots.Length; i++)
        {
            slots[i].Reset();
        }
    }
}
