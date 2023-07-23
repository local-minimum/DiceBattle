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
        Monster.OnAttack += Monster_OnAttack;
        PlayerCharacter.OnHealthChange += PlayerCharacter_OnHealthChange;
    }

    private void OnDisable()
    {
        ActionCard.OnAction -= ActionCard_OnAction;
        Monster.OnReport -= Monster_OnReport;
        Monster.OnAttack -= Monster_OnAttack;
        PlayerCharacter.OnHealthChange -= PlayerCharacter_OnHealthChange;
    }

    private void PlayerCharacter_OnHealthChange(int newHealth, int delta)
    {
        if (delta < 0)
        {
            History.Enqueue($"[Player] Lost {Mathf.Abs(delta)} health ({newHealth})");
        } else if (delta > 0)
        {
            History.Enqueue($"[Player] Restored {delta} health ({newHealth})");
        }

        Publish();
    }

    private void Monster_OnAttack(Monster monster, MonsterAction action)
    {
        History.Enqueue($"[{monster.Name}] Does {action.Name} with strength {action.Value}");
        Publish();
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
        } else if (card.Value <= 0)
        {
            History.Enqueue($"Player performs {card.ItemName} in the air in front of {receiver.Name}");
        } else
        {
            var defence = receiver.ConsumeDefenceForAttack(card.Value);
            var damage = card.Value - defence;
            if (damage <= 0)
            {
                History.Enqueue($"Player attacks {receiver.Name} with a {card.Value} points {card.ItemName} but fails to damage them");
            } else
            {
                receiver.Health -= damage;
                History.Enqueue($"Player attacks {receiver.Name} with a {card.Value} points {card.ItemName} causing {damage} damage");

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
