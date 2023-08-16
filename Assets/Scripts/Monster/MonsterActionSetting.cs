using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterAction", menuName = "Battle/MonsterActionSettings")]
public class MonsterActionSetting : ScriptableObject 
{
    public string Name;
    public int ActionPoints = 1;
    public int StartCooldown = 0;
    public int Cooldown;
    public ActionType ActionType;
    public DiceSlot[] Slots;
    public Sprite Sprite;
    public int EquipCost;

    public string Notation
    {
        get
        {
            var dice = string.Join(" ", Slots);
            return $"<{ActionType}> {dice}";
        }
    }
}
