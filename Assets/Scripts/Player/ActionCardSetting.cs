using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DieEffect { Add, Subtract };

public enum ActionType { Attack, Defence, Healing }

[System.Serializable]
public struct DiceSlot
{
    public int DefaultValue;
    public DieEffect Effect;
    public bool HasDefaultValue => DefaultValue == 0;

    public override string ToString()
    {
        var effect = Effect == DieEffect.Add ? "+" : "-";
        var value = HasDefaultValue ? "D6" : DefaultValue.ToString();
        return $"{effect} {value}";
    }
}

[CreateAssetMenu(fileName = "ActionCard", menuName = "Battle/ActionCardSetting")]
public class ActionCardSetting : ScriptableObject
{
    public string Name;
    public ActionType ActionType;
    public DiceSlot[] Slots;
    public Sprite Sprite;
    public int ShopCost;

    public string Summary()
    {
        var dice = string.Join(" ", Slots);
        if (dice.StartsWith("+"))
        {
            dice = dice.Substring(2);
        }

        return $"[{ActionType}] {dice}";
    }
}
