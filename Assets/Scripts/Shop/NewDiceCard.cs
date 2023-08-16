using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiceCard", menuName = "Shop/DiceCard")]
public class NewDiceCard : ScriptableObject 
{
    public string Name => Dice == 1 ? "A die" : $"{Dice} dice";
    public int Dice = 1;
    [Range(0, 2)]
    public float CostPerOwned;
    public Sprite Sprite;

    public int Cost => Mathf.RoundToInt(GameProgress.Dice * CostPerOwned * Dice);
}
