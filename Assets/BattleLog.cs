using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Monster.OnReport += Monster_OnReport;
    }

    private void OnDisable()
    {
        ActionCard.OnAction -= ActionCard_OnAction;
        Monster.OnReport -= Monster_OnReport;
    }

    private void Monster_OnReport(Monster monster, string report)
    {
        History.Enqueue($"[{monster.Name}] {report}");
        Publish();
    }

    private void ActionCard_OnAction(ActionCard card, Monster receiver)
    {
        if (receiver == null)
        {
            History.Enqueue($"Player applies {card.ItemName} with value {card.Value}");
        } else
        {
            var attack = card.Value - receiver.Defence;
            if (attack <= 0)
            {
                History.Enqueue($"Player attacks {receiver.Name} with {card.ItemName} but fails to damage them");
            } else
            {
                receiver.Health -= attack;
                History.Enqueue($"Player attacks {receiver.Name} with {card.ItemName} causing {attack} damage");

                if (receiver.Health == 0)
                {
                    History.Enqueue($"{receiver.Name} dies");
                    History.Enqueue($"Player gains {receiver.XpReward} XP");
                }
            }
        }
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
        TextUI.text = string.Join("\n", History.Reverse());
    }
}