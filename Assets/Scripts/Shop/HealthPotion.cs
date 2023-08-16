using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "Shop/HealthPotion")]
public class HealthPotion : ScriptableObject 
{
    public string Name;

    [Range(0, 1)]
    public float RestoreFraction;

    [Range(1, 2)]
    public float CostPerHealthUnit;

    public Sprite Sprite;

    public int Amount => Mathf.RoundToInt(GameProgress.MaxHealth * RestoreFraction);

    public int Cost => Mathf.RoundToInt(Amount * CostPerHealthUnit);
}
