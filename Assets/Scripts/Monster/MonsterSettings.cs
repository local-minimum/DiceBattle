using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DeCrawl.Utils;

[CreateAssetMenu(fileName = "MonsterSettings", menuName = "Battle/MonsterSettings")]
public class MonsterSettings : ScriptableObject 
{
    [Header("- General Settings -")]
    public string Name;
    public int DifficultyScore = 1;
    public int XPReward = 100;
    public int BaseDefence = 0;
    public int MaxHealth = 10;

    [Header("- Dice Settings -")]
    public int Dice;
    public int MinDicePerTurn = 1;
    public int MaxDicePerTurn = 2;
    public AnimationCurve ExtraDiceProb;

    [Header("- Action Settings -")]
    public int ActionPointsPerTurn;
    public int ActionEquipPoints;
    public MonsterActionSetting[] PossibleActions;

    public IEnumerable<MonsterActionSetting> EquipActions()
    {
        int points = ActionEquipPoints;
        List<MonsterActionSetting> usedActions = new List<MonsterActionSetting>();

        foreach (var free in PossibleActions.Where(a => a.EquipCost == 0))
        {
            usedActions.Add(free);
            yield return free;
        }

        while (points > 0)
        {
            var options = PossibleActions
                .Where(a => a.EquipCost <= points && !usedActions.Contains(a))
                .Shuffle()
                .ToArray();

            if (options.Length == 0) break;

            var option = options[0];

            usedActions.Add(option);
            points -= option.EquipCost;

            yield return option;
        }
    }
}
