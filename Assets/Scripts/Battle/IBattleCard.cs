using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleCard
{
    public ActionType ActionType { get;  }
    public string Name { get; }

    public int Value { get; }
    public int HighestPossibleValue { get;  }
    public string ValueRange { get; }

    public string Notation { get; }

    public void Use();
}
