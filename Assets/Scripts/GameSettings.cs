using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static float MonsterPhasePreDelay => 1.0f;
    public static float MonsterPhaseAttackDuration => 1.0f;
    public static float MonsterPhasePostDelay => 1.0f;

    public static int MonsterDifficultyBase = 2;
    public static int MonsterDifficultyPerFight = 5;

    public static int MaxDiceHand = 8;
}
