using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iconography : DeCrawl.Primitives.FindingSingleton<Iconography> 
{
    [SerializeField]
    Sprite AttackSprite;
    [SerializeField]
    Sprite DefenceSprite;
    [SerializeField]
    Sprite HealingSprite;
    [SerializeField]
    Sprite UnknownActionSprite;

    public static Sprite GetAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Attack:
                return instance.AttackSprite;
            case ActionType.Defence:
                return instance.DefenceSprite;
            case ActionType.Healing:
                return instance.HealingSprite;
            default:
                return instance.UnknownActionSprite;
        }
    }

    [SerializeField]
    Sprite DieNegOneSprite;
    [SerializeField]
    Sprite DieNegTwoSprite;
    [SerializeField]
    Sprite DieNegThreeSprite;
    [SerializeField]
    Sprite DieNegFourSprite;
    [SerializeField]
    Sprite DieNegFiveSprite;
    [SerializeField]
    Sprite DieNegSixSprite;
    [SerializeField]
    Sprite DieOneSprite;
    [SerializeField]
    Sprite DieTwoSprite;
    [SerializeField]
    Sprite DieThreeSprite;
    [SerializeField]
    Sprite DieFourSprite;
    [SerializeField]
    Sprite DieFiveSprite;
    [SerializeField]
    Sprite DieSixSprite;
    [SerializeField]
    Sprite NoDie;

    public static Sprite GetDie(int value)
    {
        switch (value)
        {
            case 1:
                return instance.DieOneSprite;
            case 2:
                return instance.DieTwoSprite;
            case 3:
                return instance.DieThreeSprite;
            case 4:
                return instance.DieFourSprite;
            case 5:
                return instance.DieFiveSprite;
            case 6:
                return instance.DieSixSprite;
            case -1:
                return instance.DieNegOneSprite;
            case -2:
                return instance.DieNegTwoSprite;
            case -3:
                return instance.DieNegThreeSprite;
            case -4:
                return instance.DieNegFourSprite;
            case -5:
                return instance.DieNegFiveSprite;
            case -6:
                return instance.DieNegSixSprite;
            default:
                return instance.NoDie;
        }
    }
}
