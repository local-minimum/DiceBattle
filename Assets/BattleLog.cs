using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLog : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    [SerializeField, Range(0, 20)]
    int MaxHistory = 10;

    Queue<string> History = new Queue<string>();

    private void OnEnable()
    {
        History.Clear();
        ActionCard.OnAction += ActionCard_OnAction;
    }

    private void OnDisable()
    {
        ActionCard.OnAction -= ActionCard_OnAction;
    }

    private void ActionCard_OnAction(ActionCard card)
    {
        History.Enqueue($"Player does a {card.Value} attack with {card.ItemName}");
        Publish();
    }

    void TruncateHistory()
    {
        int toRemove = Mathf.Max(0, History.Count - MaxHistory);
        if (toRemove == 0) return;
        for (int i = 0; i<toRemove; i++)
        {
            History.Dequeue();
        }
    }

    void Publish()
    {
        TruncateHistory();
        TextUI.text = string.Join("\n", History);
    }
}
