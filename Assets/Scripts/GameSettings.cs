using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static float MonsterPhasePreDelay => 1.0f;
    public static float MonsterPhaseAttackDuration => 1.0f;
    public static float MonsterPhasePostDelay => 1.0f;

    public static int MonsterDifficultyBase = 2;
    public static int MonsterDifficultyPerFight = 2;
    public static int MaxMonstersInFight = 3;

    public static int MaxPlayerRollSize = 8;
    public static int MaxPlayerCardHandSize = 4;

    public static int MaxShowWares = 5;

}
