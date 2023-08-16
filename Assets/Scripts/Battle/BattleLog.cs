using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

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
            History.Enqueue($"[Player] Lost **{Mathf.Abs(delta)} health**");
        } else if (delta > 0)
        {
            History.Enqueue($"[Player] Restored **{delta} health**");
        }

        Publish();
    }

    private void Monster_OnAttack(Monster monster, MonsterAction action)
    {
        History.Enqueue($"[{monster.Name}] Does <{action.Name}> with **{action.Value} strength**");
        Publish();
    }

    private void Monster_OnReport(Monster monster, string report)
    {
        History.Enqueue($"[{monster.Name}] {report}");
        Publish();
    }

    private void ActionCard_OnAction(ActionCard card, Monster receiver, int damage)
    {
        if (receiver == null)
        {
            History.Enqueue($"[Player] Applies <{card.Name}> with **{card.Value} strength**");
        } else if (card.Value <= 0)
        {
            History.Enqueue($"[Player] Performs <{card.Name}> in the air in front of <{receiver.Name}>");
        } else
        {
            if (damage <= 0)
            {
                History.Enqueue($"[Player] Attacks <{receiver.Name}> with a {card.Value} points <{card.Name}> but fails to damage them");
            } else
            {
                History.Enqueue($"[Player] Attacks <{receiver.Name}> with a {card.Value} points <{card.Name}> causing **{damage} damage**");

                if (receiver.Health == 0)
                {
                    History.Enqueue($"[{receiver.Name}] Dies!");
                    History.Enqueue($"[Player] Gains **{receiver.XpReward} XP**");

                    if (!MonsterManager.instance.AnyAlive)
                    {
                        History.Enqueue("[Battle] **All monsters have been vaquished**");
                    }
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

    IEnumerable<string> MarkedUpHistory => History
        .Select(line => {
            var objects = Regex.Replace(line, @"(\<.*?\>)", "<color=#FF55FF>" + @"$1" + "</color>");
            var subjects = Regex.Replace(objects, @"(\[.*?\])", "<b><color=#119944>" + @"$1" + "</color></b>");
            return Regex.Replace(subjects, @"\*\*(.*?)\*\*", "<u>" + @"$1" + "</u>");
        });

    void Publish()
    {
        TruncateHistory();
        TextUI.text = string.Join("\n", MarkedUpHistory.Reverse());
    }
}
