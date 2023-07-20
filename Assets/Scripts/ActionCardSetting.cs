using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DieEffect { Add, Subtract };

public enum ActionType { Attack, Defence, Utility }

[System.Serializable]
public struct DiceSlot
{
    public int DefaultValue;
    public DieEffect Effect;
    public bool HasDefaultValue => DefaultValue == 0;
}

[CreateAssetMenu(fileName = "ActionCard", menuName = "Battle/ActionCardSetting")]
public class ActionCardSetting : ScriptableObject
{
    public string Name;
    public ActionType ActionType;
    public DiceSlot[] Slots;
}
